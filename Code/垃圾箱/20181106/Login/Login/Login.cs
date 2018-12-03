using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Collections;
using BookSql;

namespace Login
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
        public MapAndKey() {        }
        //构造函数，初始化输入密码
        public MapAndKey(string in_key)
        {
            key_word = in_key;
        }
        //创建密码索引
        int[] CreatIndexes()
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
            int[] indexes = CreatIndexes();

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
                return (string)null;
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


}
