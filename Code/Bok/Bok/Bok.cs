using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace Bok
{
    namespace DebugLog
    {
        public static class LogCenter
        {
            /*配置*/
            private static string paramFilePath;  // 文件路径(结尾需要带\)
            private static string paramFileNameHead;  // 文件头
            private static string paramFileNameTail;  // 文件尾

            public static uint paramMinLevel;  // 最小日志级别
            public static uint paramMaxLevel;  // 最大日志级别
            private static uint paramMaxLine;  // 日志文件最大行数

            private static uint paramSleepTime;  // 短休眠时间(毫秒)
            private static uint paramSleepCount;  // 进入长休眠所需的短次数

            private static Boolean isParam = false;

            /*主体*/
            private static int ProcessNum;  // 进程号
            private static Queue<DebugInfo> DebugList;

            private static eWorkState WorkState;
            private static uint LineCount;
            private static uint FileCount;

            /*初始化*/
            public static void Init()
            {
                ProcessNum = System.Diagnostics.Process.GetCurrentProcess().Id;  // 进程号
                DebugList = new Queue<DebugInfo>();

                WorkState = eWorkState.LongSleep;
                LineCount = 0;
                FileCount = 0;
            }
            // 参数配置
            public static void Param(string FilePath, string FileNameHead, string FileNameTail, uint MaxLine, uint ShortSleepTime, uint LongSleepCount)
            {
                if (isParam == false)
                {
                    paramFilePath = FilePath;
                    paramFileNameHead = FileNameHead;
                    paramFileNameTail = FileNameTail;
                    CreatePathFile(paramFilePath, paramFileNameHead + paramFileNameTail);

                    paramMinLevel = 0;
                    paramMaxLevel = 10;
                    paramMaxLine = MaxLine;

                    paramSleepTime = ShortSleepTime;
                    paramSleepCount = LongSleepCount;

                    isParam = true;
                }
            }
            /*日志推送*/
            public static void Push(DebugInfo info)
            {
                if (paramMinLevel <= info.DebugLevel && info.DebugLevel <= paramMaxLevel)
                {
                    info.ThreadNum = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    DebugList.Enqueue(info);
                    if (WorkState == eWorkState.LongSleep)
                    {
                        WorkState = eWorkState.Work;
                        Task task = new Task(Writing);
                        task.Start();
                    }
                }
            }
            public static void Push(uint level,uint debugtype,string text)
            {
                DebugInfo info = new DebugInfo(level, (eDebugType)debugtype, text);
                if (paramMinLevel <= info.DebugLevel && info.DebugLevel <= paramMaxLevel)
                {
                    info.ThreadNum = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    DebugList.Enqueue(info);
                    if (WorkState == eWorkState.LongSleep)
                    {
                        WorkState = eWorkState.Work;
                        Task task = new Task(Writing);
                        task.Start();
                    }
                }
            }
            /*执行日志输出*/
            private static void Writing()
            {
                uint SleepCount = 0;
                StreamWriter writer = new StreamWriter(paramFilePath + paramFileNameHead + paramFileNameTail, true);
                while (SleepCount <= paramSleepCount)
                {
                    switch (WorkState)
                    {
                        case eWorkState.LongSleep:
                            if (DebugList.Count == 0)
                            {
                                // 不会走到这里
                                break;
                            }
                            else
                            {
                                // 长眠期间来日志包，唤醒
                                SleepCount = 0;
                                WorkState = eWorkState.Work;
                                break;
                            }
                        case eWorkState.ShortSleep:
                            if (DebugList.Count == 0)
                            {
                                // 短眠，没日志包，继续睡一会儿
                                System.Threading.Thread.Sleep((int)paramSleepTime);
                                SleepCount++;
                                break;
                            }
                            else
                            {
                                // 短眠，来日志包，唤醒
                                SleepCount = 0;
                                WorkState = eWorkState.Work;
                                break;
                            }
                        case eWorkState.Pause:
                            if (DebugList.Count == 0)
                            {
                                // 暂停，不管有没有日志包，不计入短眠次数
                                System.Threading.Thread.Sleep((int)paramSleepTime);
                                break;
                            }
                            else
                            {
                                // 暂停，不管有没有日志包，不计入短眠次数
                                System.Threading.Thread.Sleep((int)paramSleepTime);
                                break;
                            }
                        case eWorkState.Work:
                            if (DebugList.Count == 0)
                            {
                                // 工作状态，发现没日志包了，进入短眠
                                WorkState = eWorkState.ShortSleep;
                                break;
                            }
                            else
                            {
                                // 干活
                                DebugInfo info = DebugList.Dequeue();
                                writer.WriteLine(DebugFormat(info));
                                writer.Flush();
                                LineCount++;
                                if (LineCount >= paramMaxLine)
                                {//日志文件太大了，换一份新的
                                    WorkState = eWorkState.Pause;
                                    writer.Close();
                                    UseNewFile();
                                    LineCount = 0;
                                    FileCount++;
                                    writer = new StreamWriter(paramFilePath + paramFileNameHead + paramFileNameTail, true);
                                    WorkState = eWorkState.Work;
                                }
                                break;
                            }
                    }//switch
                }//while
                WorkState = eWorkState.LongSleep;
                writer.Close();
            } // Writing
            

            /*如果目录或者文件不存在，则创建目录或文件*/
            private static void CreatePathFile(string filePath, string fileName)
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (!File.Exists(filePath + fileName))
                {
                    File.Create(filePath + fileName).Close();
                }
            }

            /*切换新的文件*/
            private static void UseNewFile()
            {
                System.Threading.Thread.Sleep(1000);

                string time = DateTime.Now.ToString("yyyyMMddTHHmmss");
                string throwFileName = paramFileNameHead + "_" + FileCount + "_" + time + paramFileNameTail;
                string throwFile = paramFilePath + throwFileName;
                string rowFile = paramFilePath + paramFileNameHead + paramFileNameTail;

                File.Move(rowFile, throwFile);
            }

            /*日志信息格式化输出*/
            public static string DebugFormat(DebugInfo debugInfo)
            {
                string debugPack = debugInfo.DebugTime.ToString("<yyyy-MM-dd HH:mm:ss:fff>");
                debugPack = debugPack + "[" + ProcessNum + "·" + debugInfo.ThreadNum + "]";
                debugPack = debugPack + "|" + debugInfo.DebugType.ToString() + "·" + debugInfo.DebugLevel + "| ";
                debugPack = debugPack + debugInfo.DebugText;
                return debugPack;
            }

        } //LogCenter

        enum eWorkState
        {
            LongSleep = 0,
            ShortSleep = 1,
            Work = 2,
            Pause = 3
        }

        public enum eDebugType
        {
            Info = 0,
            War = 1,
            Err = 2
        }

        public struct DebugInfo
        {
            public DateTime DebugTime;
            public uint DebugLevel;
            public int ProcessNum;
            public int ThreadNum;
            public eDebugType DebugType;
            public string DebugText;

            public DebugInfo(uint debugLevel, eDebugType debugType, string debugText)
            {
                DebugTime = DateTime.Now;
                DebugLevel = debugLevel;
                ProcessNum = -1;
                ThreadNum = -1;
                DebugType = debugType;
                DebugText = debugText;
            }
            public DebugInfo(uint debugLevel, string debugType, string debugText)
            {
                DebugTime = DateTime.Now;
                DebugLevel = debugLevel;
                ProcessNum = -1;
                ThreadNum = -1;
                DebugType = (eDebugType)Enum.Parse(typeof(eDebugType), debugType);
                DebugText = debugText;
            }
        }
        
    } // namespace DebugLog


    namespace EventDelegate
    {
        // 消息基类
        public class MsgBase : EventArgs
        {
            public DateTime HappenTime;
            public bool Alive = false;
            public string Describe;
        }

        // 消息包
        public class Packet
        {
            public Object Sender;
            public uint MsgType;
            public MsgBase Msg;
            public Packet(uint msgType, MsgBase msg)
            {
                Sender = null;
                MsgType = msgType;
                Msg = msg;
            }
            public Packet(Object sender, uint msgType, MsgBase msg)
            {
                Sender = sender;
                MsgType = msgType;
                Msg = msg;
            }
        }

        // 消息接收者的消息处理函数的委托
        public delegate void ListenerDelegate(Packet packet);

        // 消息中心
        public static class MsgCenter
        {
            // 监听列表
            private static Dictionary<uint, ListenerDelegate> Listeners = new Dictionary<uint, ListenerDelegate>();

            // 注册消息监听
            public static void AddListener(uint msgType, ListenerDelegate Receive)
            {
                if (!Listeners.ContainsKey(msgType))
                {
                    ListenerDelegate del = null; //定义方法
                    Listeners[msgType] = del;// 给委托变量赋值
                }
                Listeners[msgType] += Receive; //注册接收者的监听
            }

            // 注销消息监听
            public static void RemoveListener(uint msgType, ListenerDelegate Receive)
            {
                if (Listeners.ContainsKey(msgType))
                {
                    Listeners[msgType] -= Receive;
                    if (Listeners[msgType] == null)
                    {
                        Listeners.Remove(msgType);
                    }
                }
            }

            // 消息推送
            public static void Push(Packet packet)
            {
                if (Listeners.ContainsKey(packet.MsgType))
                {
                    packet.Msg.Alive = true;
                    Listeners[packet.MsgType](packet);
                }
            }
        }

        // 接收者的基类
        public class Listener
        {
            // 注册监听
            public void ReceiveOpen(uint msgType)
            {
                MsgCenter.AddListener(msgType, ReceiveToDo);
            }
            // 注销监听
            public void ReceiveClose(uint msgType)
            {
                MsgCenter.RemoveListener(msgType, ReceiveToDo);
            }
            // 消息处理
            public virtual void ReceiveToDo(Packet packet)
            {
                // Do Something
            }
        }

    } // namespace EventDelegate


    namespace Security
    {
        static class MachineCode
        {
            // 获取cpuID
            private static string GetCpuInfo()
            {
                string CpuInfo = " ";
                ManagementClass cimobject = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = cimobject.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    CpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                return CpuInfo.ToString();
            }

            // 获取硬盘ID 
            private static string GetHDid()
            {
                string HDid = " ";
                ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                foreach (ManagementObject mo in moc1)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                return HDid.ToString();
            }

            // 获取网卡硬件地址 
            private static string GetMoAddress()
            {
                string MoAddress = " ";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    if ((bool)mo["IPEnabled"] == true)
                        MoAddress = mo["MacAddress"].ToString();
                    mo.Dispose();
                }
                return MoAddress.ToString();
            }

            //综合硬件码
            public static string MachineCode3()
            {
                return (GetCpuInfo() + GetHDid() + GetMoAddress());
            }

        }

        static class Rander
        {
            static int SleepTime = 10;

            //字符表
            public static string Array1ToZ = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //0~Z双字节组合表
            public static string[] ArrayDouble()
            {
                string[] table = new string[62 * 62];
                for (int i = 0; i < 62; i++)
                {
                    for (int j = 0; j < 62; j++)
                    {
                        table[62 * i + j] = Array1ToZ.Substring(i, 1) + Array1ToZ.Substring(j, 1);
                    }
                }
                return table;
            }

            //输入随机种子 生成随机一个字符
            public static char Rand1ToZ(int seed)
            {
                Random ran = new Random(seed);
                int rand = ran.Next(0, 62);
                char result = Array1ToZ[rand];
                return result;
            }
            //不输入随机种子 生成随机一个字符
            public static char Rand1ToZ()
            {
                System.Threading.Thread.Sleep(SleepTime);
                Random ran = new Random();
                int rand = ran.Next(0, 61);
                char result = Array1ToZ[rand];
                return result;
            }
            //随机产生一个 数字+大/小写字母 组合成的字符串
            public static string RandString(int length)
            {
                if (length < 1)
                {
                    length = 1;
                }

                Random ran = new Random();
                string result = null;
                for (int i = 0; i < length; i++)
                {
                    result = result + Array1ToZ[ran.Next(0, 61)];
                }
                return result;
            }

            // 获取字符串的MD5码
            public static string MD5Hash(string input)
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                //md5.HashSize = 8;
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder output = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    output.Append(hashBytes[i].ToString("X2"));
                }
                return output.ToString();
            }
        }

        public class MapAndKey
        {
            int map_size = 62 * 62;//不支持大于62的数值
            int map_size_short = 1280;//为(map_size/3)
            int key_size = 32;//密码长度
            int first_indexes_short = 1024;//密码索引在map_size_short的第一个位置
            string key_word = "1a2b3c4d5E6F7G8H";
            string MyWord = "_boooook_cjk";

            //构造函数，初始化不输入密码
            public MapAndKey() { }
            //构造函数，初始化输入密码
            public MapAndKey(string in_key)
            {
                key_word = in_key;
            }
            //创建密码索引
            int[] CreateIndexes()
            {
                ArrayList used = new ArrayList();
                int[] out_indexes = new int[key_size];
                Random ran = new Random();
                int temp;
                for (int i = 0; i < key_size; i++)
                {
                    do
                    {
                        temp = ran.Next(0, map_size_short);
                    } while (used.Contains(temp));
                    out_indexes[i] = (temp * 3);
                    used.Add(temp);
                }
                return out_indexes;
            }
            //用 一个字符 替换 目标字符串 某位置处的 一个字符
            public string StringReplace(string in_string, int in_Indext, char in_replace_word)
            { return in_string.Remove(in_Indext, 1).Insert(in_Indext, in_replace_word.ToString()); }
            //用 一个字符串 替换 目标字符串 某位置处的 等长度字符串
            public string StringReplace(string in_string, int in_Indext, string in_replace_word)
            { return in_string.Remove(in_Indext, in_replace_word.Length).Insert(in_Indext, in_replace_word); }

            //生成一个map，并把key藏进去
            string HideKey()
            {
                string[] ArrayDouble = Rander.ArrayDouble();
                string map = Rander.RandString(map_size);
                int[] indexes = CreateIndexes();

                //隐藏密码
                for (int i = 0; i < key_size; i++)
                {
                    map = StringReplace(map, indexes[i], key_word.Substring(i, 1));
                }
                //隐藏索引
                int before_place = first_indexes_short * 3;
                for (int i = 0; i < key_size; i++)
                {
                    map = StringReplace(map, (before_place + 1), ArrayDouble[indexes[i]]);
                    before_place = indexes[i];
                }
                return map;
            }

            //从map中获得密码
            string FindKey(string map)
            {
                if (string.Equals(map, null))
                {
                    return "00000000";
                }
                string[] ArrayDouble = Rander.ArrayDouble();

                int before_place = first_indexes_short * 3;
                string key = null;
                for (int i = 0; i < key_size; i++)
                {
                    string z = map.Substring(before_place + 1, 2);
                    before_place = Array.IndexOf(ArrayDouble, z);
                    //累加key
                    key = key + map.Substring(before_place, 1);
                }
                return key;
            }

            //将map打包写入文件
            void PackWrite(string map, string file_name)
            {
                string md5code = Rander.MD5Hash(map + MyWord);

                //由于MD5码默认大写，这里把他随机一些字符变小写
                Random rand = new Random();
                int ran;
                for (int i = 0; i < md5code.Length; i++)
                {
                    ran = rand.Next(0, 2);
                    if (ran == 0) { md5code = StringReplace(md5code, i, md5code.ToLower().Substring(i, 1)); }
                }
                map = map + md5code;
                StreamWriter Secret_file = new StreamWriter(file_name);
                Secret_file.WriteLine(map);
                Secret_file.Flush();
                Secret_file.Close();
            }

            //将文件拆开获取map
            string ReadDecrypt(string file_name)
            {
                StreamReader secret_file = new StreamReader(file_name);
                string file_txt = secret_file.ReadToEnd();
                //如果文件长度有问题，返回null
                if (file_txt.Length < (map_size + 32))
                {
                    return null;
                }
                string map = file_txt.Substring(0, map_size);
                string md5code = file_txt.Substring(map_size, file_txt.Length - map_size);
                md5code = md5code.ToUpper();
                if (string.Equals(Rander.MD5Hash(map + MyWord), md5code))
                {
                    map = null;
                }
                return map;
            }

            public void Doing(string file_name)
            {
                string map = HideKey();
                PackWrite(map, file_name);
            }
            public string Undoing(string file_name)
            {
                string map = ReadDecrypt(file_name);
                string key = FindKey(map);
                return key;
            }
        }

        public static class Loginer
        {
            static string DataBase = "boooook.db";
            static string KeyMapFile = "booooo.txt";
            static string password = "sqlcipher";

            public static string Key()
            {
                string Key1, Key2, Password; //ED3A604ABF484FAE57D2B93E315C5078

                Key1 = MachineCode.MachineCode3();
                if (File.Exists(KeyMapFile))
                {
                    MapAndKey mak = new MapAndKey();
                    Key2 = mak.Undoing(KeyMapFile);
                    Password = Rander.MD5Hash(Key1 + Key2);
                }
                else
                {
                    Key2 = Rander.RandString(32);
                    MapAndKey mak = new MapAndKey(Key2);
                    mak.Doing(KeyMapFile);
                    Password = Rander.MD5Hash(Key1 + Key2);
                    BookSql.BookSqlDB.ChangePassword(DataBase, password, Password);
                }
                /*
                StreamWriter file = new StreamWriter(Password);
                file.WriteLine(Password);
                file.Flush();
                file.Close();
                */
                return Password;
            }

            public static BookSql.BookSqlCmd DBcmd()
            {
                BookSql.BookSqlCmd DBcmd = new BookSql.BookSqlCmd(DataBase, Key());
                return DBcmd;
            }
            public static BookSql.BookSqlCmd DBcmd(string inputKey)
            {
                BookSql.BookSqlCmd DBcmd = new BookSql.BookSqlCmd(DataBase, inputKey);
                return DBcmd;
            }

        }
    }  // namespace Security

    namespace BookSql
    {
        //sqlite数据库的建立、加/解密、连接
        public static class BookSqlDB
        {
            //新建一个带密码的sqlite数据库
            public static void Builder(string db_name, string password)
            {
                SQLiteConnectionStringBuilder new_db = new SQLiteConnectionStringBuilder();
                new_db.DataSource = db_name;
                new_db.Password = password;

                SQLiteConnection conn = new SQLiteConnection(new_db + "");
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("create table Book_DB (Version INTEGER,Info TEXT); insert into Book_DB(Version,Info) values (1,\"YES\")", conn);
                command.ExecuteNonQuery();
                command.Cancel();
                conn.Close();
            }

            //更改sqlite数据库的密码
            public static void ChangePassword(string db_name, string old_password, string new_password)
            {
                SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
                db_conn.SetPassword(old_password);
                db_conn.Open();
                db_conn.ChangePassword(new_password);
                db_conn.Close();
            }

            //打开一个无密码的sqlite连接
            public static SQLiteConnection Open(string db_name)
            {
                SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
                db_conn.Open();
                if (Check(db_conn))
                {
                    DebugLog.LogCenter.Push(2, 0, $"连接明文数据库<{db_name}>");
                    return db_conn;
                }
                else
                {
                    throw(new Exception("Err:<密码错误>或<数据库版本与预期不符>"));
                }
            }

            //打开一个带密码的sqlite连接
            public static SQLiteConnection Open(string db_name, string password)
            {
                SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
                db_conn.SetPassword(password);
                db_conn.Open();
                if (Check(db_conn))
                {
                    DebugLog.LogCenter.Push(2, 0, $"连接加密数据库<{db_name}>");
                    return db_conn;
                }
                else
                {
                    throw(new Exception("Err:\"密码错误\"或\"数据库版本与预期不符\""));
                }
            }

            //由于使用的SQLite3.dll版本bug，无法通过连接状态来判断密码是否正确，所以要加一个判断连接是否成功的动作
            public static bool Check(SQLiteConnection db_conn)
            {
                int rv = 0;
                SQLiteCommand command = new SQLiteCommand(db_conn);
                command.CommandText = "select 1";
                SQLiteDataReader reader = command.ExecuteReader();

                reader.Read();
                rv = Convert.ToInt32(reader["1"]);
                reader.Close();
                command.Dispose();
                return (rv == 1);
            }

            //删库，谨慎执行
            public static void Delete(string DBName, string PassWord)
            {
                if (System.IO.File.Exists(DBName))
                {
                    SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + DBName);
                    db_conn.SetPassword(PassWord);
                    db_conn.Open();
                    Check(db_conn);
                    db_conn.Dispose();
                    System.IO.File.Delete(DBName);
                }
            }
        }

        //sqlite命令执行器
        public class BookSqlCmd
        {
            SQLiteConnection Connection;
            SQLiteCommand Command;
            SQLiteDataReader Reader;

            //空构造
            public BookSqlCmd() { }
            //构造执行器时，连接到不带密码数据库
            public BookSqlCmd(string db_name)
            {
                DebugLog.LogCenter.Push(2, 0, $"连接到明文数据库<{db_name}>");
                Connection = new SQLiteConnection("Data Source=" + db_name);
                Connection.Open();
                Command = new SQLiteCommand(Connection);
            }
            //构造执行器时，连接到带有密码数据库
            public BookSqlCmd(string db_name, string password)
            {
                DebugLog.LogCenter.Push(2, 0, $"连接到密文数据库<{db_name}>");
                Connection = new SQLiteConnection("Data Source=" + db_name);
                Connection.SetPassword(password);
                Connection.Open();
                Command = new SQLiteCommand(Connection);
            }
            //构造执行器时，借用现有的连接
            public BookSqlCmd(SQLiteConnection connection)
            {
                Connection = connection;
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
                Command = new SQLiteCommand(Connection);
            }

            // 关闭数据库连接、命令、阅读器
            public void close()
            {
                if (Reader != null)
                {
                    Reader.Close();
                }
                Command.Dispose();
                Connection.Close();
            }


            //非查询命令
            public void NonQuery(string CommandText)
            {
                DebugLog.LogCenter.Push(12, 0, $"执行Sql：{CommandText}");
                if (Reader != null) { Reader.Close(); }
                Command.CommandText = CommandText;
                Command.ExecuteNonQuery();
            }

            //查询命令（是否自动执行.Read()）
            public void Reading(string CommandText, Boolean isRead)
            {
                DebugLog.LogCenter.Push(12, 0, $"Sql查询：{CommandText}");
                if (Reader != null) { Reader.Close(); }
                Command.CommandText = CommandText;
                Reader = Command.ExecuteReader();
                if (isRead) { Reader.Read(); }
            }
            //查询命令（默认自动执行.Read()）
            public void Reading(string CommandText)
            {
                DebugLog.LogCenter.Push(12, 0, $"Sql查询：{CommandText}");
                if (Reader != null) { Reader.Close(); }
                Command.CommandText = CommandText;
                Reader = Command.ExecuteReader();
                Reader.Read();
            }
            //继续查询
            public Boolean Reading()
            {
                return Reader.Read();
            }

            public bool ReadBool(string inKey) { return Convert.ToBoolean(Reader[inKey]); }
            public int ReadInt(string inKey) { return Convert.ToInt32(Reader[inKey]); }
            public uint ReadUInt(string inKey) { return Convert.ToUInt32(Reader[inKey]); }
            public double ReadDouble(string inKey) { return Convert.ToDouble(Reader[inKey]); }
            public char ReadChar(string inKey) { return Convert.ToChar(Reader[inKey]); }
            public string ReadString(string inKey) { return Convert.ToString(Reader[inKey]); }

            // 读取某字段的全部记录，以List<string>的形式返回结果
            public List<string> ReadList(string table, string field)
            {
                List<string> list = new List<string>();
                Reading($"select {field} from {table}", false);

                if (Reading())
                {
                    list.Add(ReadString(field));
                }
                return list;
            }

            // 读取某2个字段的全部记录，以Dictionary<string, string>的形式返回结果
            public Dictionary<string, string> ReadList(string table, string keyfield, string valuefield)
            {
                Dictionary<string, string> list = new Dictionary<string, string>();
                Reading($"select {keyfield},{valuefield} from {table}", false);
                while (Reading())
                {
                    list.Add(ReadString(keyfield), ReadString(valuefield));
                }
                return list;
            }

            public BookSqlCmd Clone()
            {
                BookSqlCmd NewCmd = new BookSqlCmd();
                NewCmd.Connection = (SQLiteConnection)Connection.Clone();
                if (NewCmd.Connection.State == ConnectionState.Closed)
                {
                    NewCmd.Connection.Open();
                }
                NewCmd.Command = new SQLiteCommand(Connection);
                return NewCmd;
            }

        } // BookSqlCmd

    } // namespace BookSql


    namespace BFC
    {
        public class Date
        {
            public static Dictionary<string, string> ExchangeExInfo(string inExInfo)
            {//转化各种如"物种=人类|性别=男|职业=乌干达"的字符串为Dictionary"
                Dictionary<string, string> outExInfo = new Dictionary<string, string>();
                string String1 = inExInfo;
                string String2 = inExInfo;
                int cut1 = 0, cut2 = 0;
                string add1 = null, add2 = null;

                while (String2.Contains("="))
                {
                    if (String2.Contains("|"))
                    {
                        cut1 = String2.IndexOf('|');
                        String1 = String2.Substring(0, cut1);
                        String2 = String2.Substring(cut1 + 1);

                        cut2 = String1.IndexOf('=');
                        add1 = String1.Substring(0, cut2);
                        add2 = String1.Substring(cut2 + 1);
                        outExInfo.Add(add1, add2);
                    }
                    else
                    {
                        cut2 = String2.IndexOf('=');
                        add1 = String2.Substring(0, cut2);
                        add2 = String2.Substring(cut2 + 1);
                        outExInfo.Add(add1, add2);
                        String2 = "";
                    }

                    if (String1.Contains("="))
                    {

                    }
                }
                return outExInfo;
            }
        }// Date

        public class Factory
        {
            public static T CreateInstance<T>(string nameSpace, string className, object[] parameters)
            {
                //try
                //{
                // className中，类中类中+号代替.号
                string fullName = nameSpace + "." + className;
                DebugLog.LogCenter.Push(5,0, "CreateInstance:"+fullName);
                object obj = Assembly.GetExecutingAssembly().CreateInstance(fullName, true, System.Reflection.BindingFlags.Default, null, parameters, null, null);//加载程序集，创建程序集里面的 命名空间.类型名 实例
                return (T)obj;
                //}
                //catch
                //{
                //发生异常，返回类型的默认值
                //    return default(T);
                //}
            }

        } //Factory

    } // namespace ABC


} // namespace Boook
