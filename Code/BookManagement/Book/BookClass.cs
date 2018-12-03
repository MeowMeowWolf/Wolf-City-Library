using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Book
{

    public static class MachineCode
    {
        // 获取cpuID
        static string GetCpuInfo()
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
        static string GetHDid()
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
        static string GetMoAddress()
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

    public static class Rander
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
                BookSqlDB.ChangePassword(DataBase, password, Password);
            }
            /*
            StreamWriter file = new StreamWriter(Password);
            file.WriteLine(Password);
            file.Flush();
            file.Close();
            */
            return Password;
        }

        public static BookSqlCmd DBcmd()
        {
            BookSqlCmd DBcmd = new BookSqlCmd(DataBase, Key());
            return DBcmd;
        }
        public static BookSqlCmd DBcmd(string inputKey)
        {
            BookSqlCmd DBcmd = new BookSqlCmd(DataBase, inputKey);
            return DBcmd;
        }

    }

    
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
                return db_conn;
            }
            else
            {
                throw (new Exception("Err:\"密码错误\"或\"数据库版本与预期不符\""));
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
                return db_conn;
            }
            else
            {
                throw (new Exception("Err:\"密码错误\"或\"数据库版本与预期不符\""));
            }
        }

        //由于使用的SQLite3.dll版本bug，无法通过连接状态来判断密码是否正确，所以要加一个判断连接是否成功的动作
        public static bool Check(SQLiteConnection db_conn)
        {
            int rv = 0;
            SQLiteCommand command = new SQLiteCommand(db_conn);
            command.CommandText = "select count(1) from Book_DB";
            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();
            rv = Convert.ToInt32(reader["count(1)"]);
            reader.Close();
            command.Dispose();
            return (rv > 0);
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
            }
            System.IO.File.Delete(DBName);
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
            Connection = new SQLiteConnection("Data Source=" + db_name);
            Connection.Open();
            Command = new SQLiteCommand(Connection);
        }
        //构造执行器时，连接到带有密码数据库
        public BookSqlCmd(string db_name, string password)
        {
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
            if (Reader != null) { Reader.Close(); }
            Command.CommandText = CommandText;
            Command.ExecuteNonQuery();
        }

        //查询命令（是否自动执行.Read()）
        public void Reading(string CommandText, Boolean isRead)
        {
            if (Reader != null) { Reader.Close(); }
            Command.CommandText = CommandText;
            Reader = Command.ExecuteReader();
            if (isRead) { Reader.Read(); }
        }
        //查询命令（默认自动执行.Read()）
        public void Reading(string CommandText)
        {
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
    }

    public class tBook
    {//书
        public static Dictionary<int, tBook> List = new Dictionary<int, tBook>();
        public int tBookId;
        public string tName;
        public string Introduce;

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Cmd.Reading("select * from Book_T", false);
            while (Cmd.Reading())
            {
                tBook BookTemp = new tBook();
                BookTemp.tBookId = Cmd.ReadInt("Book_Id");
                BookTemp.tName = Cmd.ReadString("Book_Name");
                BookTemp.Introduce = Cmd.ReadString("Introduce");
                List.Add(BookTemp.tBookId, BookTemp);
            }
        }
    }

    public enum ePageType
    { TheOne, TheTime, ThePlace, TheEvent, TheLegend }
    public enum ePageTypeDesc
    { 生灵, 时间, 环境, 事件, 传奇 }

    public enum eRareLevel
    { Unreal, Papyrus, SilkBook, JadeBook, Wordless }
    public enum eRareLevelDesc
    { 言灵, 草纸, 帛书, 玉简, 无字碑 }

    public enum eShape
    { Circular, Triangle, Rectangle, Hexagon, FivePointedStar }
    public enum eShapeDesc
    { 圆形, 三角形, 长方形, 六边形, 五角星 }
    public struct Figure
    {
        public eShape Shape;
        public int SizeX;
        public int SizeY;
        public Figure(string shape, string Size)
        {
            Shape = (eShape)Enum.Parse(typeof(eShape), shape);
            int X = Size.IndexOf("×");
            SizeX = Convert.ToInt32(Size.Substring(0, X));
            SizeY = Convert.ToInt32(Size.Substring(X + 1));
        }
    }

    public class tPage
    {
        public static Dictionary<int, tPage> List = new Dictionary<int, tPage>();

        public int tPageId;
        public string tName;
        public tBook tBookBelong;
        public ePageType PageType;
        public int tRateLevel;
        public string Introduce;
        //public int tSummonRule;//在数据库找
        public int tCost;
        public List<AbiInfo> tAbilityList;
        public Dictionary<string, string> tExInfo;
        public string getExMsg(string inKey)
        {
            if (tExInfo.ContainsKey(inKey))
            { return tExInfo[inKey]; }
            else
            { return "none"; };
        }

        public void tPageFromDB(int PageId, BookSqlCmd Cmd)
        {
            Cmd.Reading("select * from All_Page_T where Page_Id=" + PageId);
            tPageId = Cmd.ReadInt("Page_ID");
            tName = Cmd.ReadString("Page_Name");
            tBookBelong = tBook.List[Cmd.ReadInt("Book_Id")];
            PageType = (ePageType)Enum.Parse(typeof(ePageType), Cmd.ReadString("Page_Type"));
            tRateLevel = Cmd.ReadInt("Rare_Level");
            tCost = Cmd.ReadInt("Cost");
            //tSummonRule = Cmd.ReadInt("Summon_Rule");
            tAbilityList = BFC.ExchangeAbility(Cmd.ReadString("Ability_List"));
            tExInfo = BFC.ExchangeExInfo(Cmd.ReadString("Ex_Info"));
            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Dictionary<string, string> list = Cmd.ReadList("All_Page_T", "Page_Id", "Page_Type");
            foreach (string BookId in list.Keys)
            {
                switch (list[BookId])
                {
                    case "TheOne":
                        tTheOnePage OnePageTemp = new tTheOnePage(Convert.ToInt32(BookId), Cmd);
                        List.Add(OnePageTemp.tPageId, OnePageTemp);
                        break;
                    case "ThePlace":
                        tThePlacePage PlacePageTemp = new tThePlacePage(Convert.ToInt32(BookId), Cmd);
                        List.Add(PlacePageTemp.tPageId, PlacePageTemp);
                        break;
                    case "TheEvent":
                        tTheEventPage TheEventPage = new tTheEventPage(Convert.ToInt32(BookId), Cmd);
                        List.Add(TheEventPage.tPageId, TheEventPage);
                        break;
                    case "TheTime":
                        tTheTimePage TheTimePage = new tTheTimePage(Convert.ToInt32(BookId), Cmd);
                        List.Add(TheTimePage.tPageId, TheTimePage);
                        break;
                }
            }
        }

    } //tPage

    public class tTheOnePage : tPage
    {
        public int tLife;
        public int tAtk;
        public int tAtkMax;

        //空构建
        public tTheOnePage() { }

        //根据PageId从数据库读取数据，进行构造
        public tTheOnePage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_One_Page_V where Page_Id=" + PageId);
            tLife = Cmd.ReadInt("Life");
            tAtk = Cmd.ReadInt("Atk");
            tAtkMax = Cmd.ReadInt("Atk_Max");
        }
    }

    public class tThePlacePage : tPage
    {
        public Figure tFigure;

        //空构建
        public tThePlacePage() { }
        //
        //根据PageId从数据库读取数据，进行构造
        public tThePlacePage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_Place_Page_V where Page_Id=" + PageId);
            tFigure = new Figure(Cmd.ReadString("Shape"), Cmd.ReadString("Size"));
        }
    }

    public class tTheEventPage : tPage
    {
        public String ActionObject;

        //空构建
        public tTheEventPage() { }
        //
        //根据PageId从数据库读取数据，进行构造
        public tTheEventPage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_Event_Page_V where Page_Id=" + PageId);
            ActionObject = Cmd.ReadString("Action_Object");
        }
    }

    public class tTheTimePage : tPage
    {
        public String AttObject;

        //空构建
        public tTheTimePage() { }
        //
        //根据PageId从数据库读取数据，进行构造
        public tTheTimePage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_Time_Page_V where Page_Id=" + PageId);
            AttObject = Cmd.ReadString("Att_Object");
        }
    }

    public static class BFC
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

        public static List<AbiInfo> ExchangeAbility(string inString)
        {//转化各种如"0,1,2,3"的字符串为List，没有附加信息时写"0"
            List<int> ListTemp = new List<int>();
            List<AbiInfo> ListOutput = new List<AbiInfo>();

            string String1 = inString;
            string String2 = inString;
            int cut = 0;
            while (String2.Length > 0)
            {
                if (String2.Contains(","))
                {
                    cut = String2.IndexOf(',');
                    String1 = String2.Substring(0, cut);
                    String2 = String2.Substring(cut + 1);
                    ListTemp.Add(Convert.ToInt32(String1));
                }
                else
                {
                    ListTemp.Add(Convert.ToInt32(String2));
                    String2 = "";
                }
            }

            foreach (int AbiId in ListTemp)
            {
                ListOutput.Add(AbiInfo.List[AbiId]);
            }

            return ListOutput;
        }
        public static string Desc(ePageType pageType)
        {
            return Enum.GetName(typeof(ePageTypeDesc), (int)pageType);
        }
        
        public static string Desc(eShape Shape)
        {
            return Enum.GetName(typeof(eShapeDesc), (int)Shape);
        }

        public static string Msg(tPage Page)
        {
            string msg = "";

            msg = msg + $"稀有度： {Page.tRateLevel}" + "\r\n";
            msg = msg + $"费用： {Page.tCost}" + "\r\n";
            /*
            foreach (string key in Page.tExInfo.Keys)
            { msg = msg + $"|{key}：{Page.tExInfo[key]} "; }
            msg = msg + "\r\n";
            */
            switch (Page.PageType)
            {
                case ePageType.TheOne:
                    tTheOnePage oPage = Page as tTheOnePage;
                    msg = msg + $"生命：{oPage.tLife}" + "\r\n";
                    msg = msg + $"攻击：{oPage.tAtk}" + "\r\n";
                    break;
                case ePageType.ThePlace:
                    tThePlacePage pPage = Page as tThePlacePage;
                    msg = msg + $"空间：{Desc(pPage.tFigure.Shape)}";
                    msg = msg + $"·{pPage.tFigure.SizeX}×{pPage.tFigure.SizeY}" + "\r\n";
                    break;
                case ePageType.TheEvent:
                    tTheEventPage ePage = Page as tTheEventPage;
                    msg = msg + $"目标：{ePage.ActionObject}" + "\r\n";
                    break;
                case ePageType.TheTime:
                    tTheTimePage tPage = Page as tTheTimePage;
                    msg = msg + $"目标：{tPage.AttObject}" + "\r\n";
                    break;
                default:
                    break;
            }

            msg = msg + "效果|————>" + "\r\n";
            foreach (AbiInfo info in Page.tAbilityList)
            {
                msg = msg + $"{info.Name}：{info.DescShort}";
            }
            msg = msg + "\r\n";

            return msg;
        }
    }


    public class AbiInfo
    {
        public int AbiTypeId;
        public string Name;
        public string AbiCode;
        public string DescShort;
        public string DescLong;

        public static Dictionary<int, AbiInfo> List = new Dictionary<int, AbiInfo>();
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Cmd.Reading("select * from Ability_T", false);
            while (Cmd.Reading())
            {
                AbiInfo abiInfo = new AbiInfo();
                abiInfo.AbiTypeId = Cmd.ReadInt("Abi_Type_Id");
                abiInfo.Name = Cmd.ReadString("Abi_Name");
                abiInfo.AbiCode = Cmd.ReadString("Abi_Code");
                abiInfo.DescShort = Cmd.ReadString("Desc_Short");
                abiInfo.DescLong = Cmd.ReadString("Desc_Long");
                List.Add(abiInfo.AbiTypeId, abiInfo);
            }
        }
    }


}