using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace TEST
{
    public class all
    {

        public class a
        {
            public void print()
            {
                Console.WriteLine("A");
            }
        }
        public class b : baseclass
        {
            public override void print()
            {
                Console.WriteLine("B");
            }
        }

        public class baseclass
        {
            public virtual void print() { }
        }

        public void print()
        {
            Console.WriteLine("ALL");
        }

    }


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

        static void Main(string[] args)
        {
            all.a ab = (all.a)Assembly.GetExecutingAssembly().CreateInstance("TEST.all+a");

            ab.print();
            Console.ReadKey();
        }
    }
}
