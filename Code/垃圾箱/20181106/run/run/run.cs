using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookCard;
using BookSql;
using Login;
using Rule;

namespace run
{

    class Boooook
    {
        
        static void Main(string[] args)
        {
            string path3 = Directory.GetCurrentDirectory();
            Console.WriteLine(path3);

            StreamWriter Secret_file = new StreamWriter("BookKey.TXT");
            Secret_file.WriteLine(Loginer.Key());
            Secret_file.Flush();
            Secret_file.Close();
            BookSqlCmd cmd = Loginer.DBcmd();//D784193A2865AD018DEE8C64B9C6477C
        
            tBook.ListFromDB(cmd);
            tTheOneCard.ListFromDB(cmd);
            ////////////////////////////////
            GameRoom room = new GameRoom();
            Camp C1 = new Camp(room, "红方", 0);
            Camp C2 = new Camp(room, "蓝方", 0);
            Character P1 = new Character(room, "火神", C1, true);
            Character P2 = new Character(room, "海神", C2, false);

            tBook.ListFromDB(cmd);
            tCard.ListFromDB(cmd);
            foreach (tTheOneCard card in tCard.List.Values)
            {
                iTheOneCard tempcard1 = new iTheOneCard(room, P1, card);
                iTheOneCard tempcard2 = new iTheOneCard(room, P2, card);
            }
            foreach (iTheOneCard card in room.AllCardList.Values)
            {
                Touch.aSummon(room, card.Chr, card);
            }
            //foreach (int key in room.AllInstList.Keys)
            //{
            //    Console.WriteLine(room.AllInstList[key].ModeId+"："+ room.AllInstList[key].mName);
            //}
            mTheOne A = (mTheOne)room.AllInstList[11];
            AutoComAtk at = new AutoComAtk(room, A);
            at.Begin();
            /////////////////////
            /*tTheOneCard a = new tTheOneCard(2);
            tTheOneCard b = new tTheOneCard(1);
            Console.WriteLine("A[攻" + a.tAtk + " 命" + a.tLife + "]" + " B[攻" + b.tAtk + " 命" + b.tLife + "]");
            Console.WriteLine("开始");
            Rule.Rule.AtkAI ai = new Rule.Rule.AtkAI(a, b);
            Thread Thread1 = new Thread(new ThreadStart(ai.Atking));
            Thread1.Start();*/
            /////////////////////



            Assembly dllBookCard = Assembly.LoadFrom("AbilityLib.dll");
            //实际上程序运行时已经加载过一次dll文件，若用“Assembly.Load”二次加载则失败，因为强次加载时已将其锁定，推荐“ReflectionOnlyLoadFrom”

            tAbilityBase Ability = (tAbilityBase)dllBookCard.CreateInstance("AbilityLib.bbb");
            Ability.AbilityDo();

            //Type AbilityLib = dllBookCard.GetType("BookCard.lAbility");
            //tAbilityBase abc = (tAbilityBase)AbilityLib.InvokeMember("aaa", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { });
            //abc.AbilityDo();

            //////////////////////////////////

            Console.ReadKey();
        }
    }
}
