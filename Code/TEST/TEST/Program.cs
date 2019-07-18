using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.InteropServices;


namespace TEST
{
    class Program
    {
        public static T CreateInstance<T>(string nameSpace, string className, object[] parameters)
        {
            try
            {
                string fullName = nameSpace + "." + className;//命名空间.类型名
                object ect = Assembly.GetExecutingAssembly().CreateInstance(fullName, false, System.Reflection.BindingFlags.Default, null, parameters, null, null);//加载程序集，创建程序集里面的 命名空间.类型名 实例
                return (T)ect;//类型转换并返回
            }
            catch
            {
                //发生异常，返回类型的默认值
                return default(T);
            }
        }

        //[DllImport("user32.dll", EntryPoint = "FindWindow")]
        //private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        static void Main(string[] args)
        {
            Random rand = new Random();
            List<int> start = new List<int>();
            List<int> count = new List<int>();

            for (int i = 0; i < 7*7*7; i++)
            {
                start.Clear();
                int temp = 999;
                while (!(start.Contains(temp) || start.Contains(temp+1) || start.Contains(temp-1) ) )
                {
                    start.Add(temp);
                    temp = rand.Next(1, 8);
                }
                count.Add(start.Count + 1);
                Console.WriteLine($"出现第{start.Count + 1}颗星时翻车。");
            }

            
            Console.WriteLine($"平均{ (float)count.Sum() / (float)count.Count}颗星星");

            Console.ReadKey();
        }
    }
}
