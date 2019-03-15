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

    public class A
    {
        public void print()
        {
            Console.WriteLine("A");
        }
    }

    namespace NB
    {
        public class B
        {
            public void print()
            {
                Console.WriteLine("B");
            }
        }

        namespace NC
        {
            public class C
            {
                public void print()
                {
                    Console.WriteLine("C");
                }
            }
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
            A a = CreateInstance<A>("TEST", "A", null);
            a.print();
            NB.B b = CreateInstance<NB.B>("TEST.NB", "B", null);
            b.print();
            NB.NC.C c = CreateInstance<NB.NC.C>("TEST.NB.NC", "C", null);
            c.print();

            Console.ReadKey();
        }
    }
}
