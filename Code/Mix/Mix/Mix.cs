using System;
using System.Threading;
using System.Reflection;

namespace Mix
{
    
    public class OneVsOne
    {
        public GameRoom room ;

        public OneVsOne(BookSqlCmd cmd)
        {
            ABI.AbiInfo.ListFromDB(cmd);
            tBuff.ListFromDB(cmd);
            tBook.ListFromDB(cmd);
            tPage.ListFromDB(cmd);

            GameRoom room = new GameRoom();
            Camp C1 = new Camp(room, "库洛牌势力", 0);
            Camp C2 = new Camp(room, "游戏王势力", 0);
            Character P1 = new Character(C1, "木之本樱", true);
            Character P2 = new Character(C2, "武藤游戏", false);

            P1.LoadMyBook(cmd, 1);
            P2.LoadMyBook(cmd, 2);
            
            int count = 0;
            foreach (iPage Page in room.RoomPageList.Values)
            {
                if (Page.PageType == ePageType.TheOne)
                {
                    Thread.Sleep(600);
                    Touch.aSummon(Page as iTheOnePage, new XY(count, count));
                    count++;
                }
            }

        }// 构建完毕

    }// OneVsOne

    class run
    {
        static void Main(string[] args)
        {
            //BookSqlCmd cmd = Loginer.DBcmd();
            BookSqlCmd cmd = new BookSqlCmd("boooook.db");
            OneVsOne ovo = new OneVsOne(cmd);

            //string aa = "";
            //System.Collections.Generic.List<int> aaa = BFC.ExchangeAbility(aa);
            //foreach (int a in aaa)
            //{
            //    Console.WriteLine(a);
            //}

            //string bb = "";
            //System.Collections.Generic.Dictionary<string,string> bbb = BFC.ExchangeExInfo(bb);
            //foreach (string b in bbb.Keys)
            //{
            //    Console.WriteLine($"{b}={bbb[b]}");
            //}

            Console.ReadKey();
        }
    }
}
