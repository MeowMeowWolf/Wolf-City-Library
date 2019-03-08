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
        //public A()
        //{ }
        public A(int abc)
        {
            Console.WriteLine("123");
        }
    }

    public class B : A
    {
        public B()
        {
            Console.WriteLine("B");
        }
    }
    public class C : A
    {
        public C(int abc) : base(abc)
        {
            Console.WriteLine("C");
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
            B b = new B();
            Console.WriteLine("————————————");
            C c = new C(123);

            Console.ReadKey();
        }
    }
}
