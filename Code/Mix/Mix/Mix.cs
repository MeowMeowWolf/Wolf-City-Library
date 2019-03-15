using System;
using System.Threading;
using System.Reflection;
using Bok.DebugLog;
using Bok.BookSql;
using Book;
using Battle;
using Battle.Room;
using Battle.Entity;
using Battle.ABI;

namespace Mix
{
    
    public class OneVsOne
    {
        public GameRoom room ;

        public OneVsOne(BookSqlCmd cmd)
        {
            tAbility.ListFromDB(cmd);
            tBook.ListFromDB(cmd);
            tPage.ListFromDB(cmd);

            GameRoom room = new GameRoom();
            Camp C1 = new Camp(room, "黑暗势力", 0);
            Camp C2 = new Camp(room, "更黑暗势力", 0);
            Character P1 = new Character(C1, "大魔王", true);
            Bok.DebugLog.LogCenter.Push(4, 0, $"玩家<{P1.Name}>，入场。");
            Character P2 = new Character(C2, "大邪神", false);
            Bok.DebugLog.LogCenter.Push(4, 0, $"玩家<{P2.Name}>，入场。");

            P1.LoadMyBook(cmd, 1);
            Bok.DebugLog.LogCenter.Push(4, 0, $"<木之本樱>加载书册。");
            P2.LoadMyBook(cmd, 2);
            Bok.DebugLog.LogCenter.Push(4, 0, $"<武藤游戏>加载书册。");

            int count = 0;
            foreach (iPage Page in room.RoomPageList.Values)
            {
                if (Page.PageType == ePageType.TheOne)
                {
                    Thread.Sleep(600);
                    Battle.Launch.TheOne.Summon(Page.Chr, Page as iTheOnePage, new XY(count, count));
                    count++;
                }
            }

        }// 构建完毕

    }// OneVsOne

    class run
    {
        static void Main(string[] args)
        {
            System.IO.File.Delete(@"E:\GitHub\WolfCityLibrary\Code\Run\Mix.Dlog");
            LogCenter.Init();
            LogCenter.Param(@"E:\GitHub\WolfCityLibrary\Code\Run\", "Mix",".Dlog",10000,0,0);
            LogCenter.paramMaxLevel = 12;

            BookSqlCmd cmd = new BookSqlCmd(@"E:\GitHub\boooook.db");
            OneVsOne ovo = new OneVsOne(cmd);

            Console.ReadKey();
        }
    }
}
