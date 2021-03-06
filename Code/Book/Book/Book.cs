﻿using System;
using System.Collections.Generic;
using Bok.BookSql;

// 卡牌资料
namespace Book
{
    public class tBook
    {
        public uint BookId;
        public string BookName;
        public string Introduce;

        public static Dictionary<uint, tBook> List = new Dictionary<uint, tBook>();
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Book列表，开始。");
            List.Clear();
            Cmd.Reading("select * from Book_T", false);
            while (Cmd.Reading())
            {
                tBook BookTemp = new tBook();
                BookTemp.BookId = Cmd.ReadUInt("Book_Id");
                BookTemp.BookName = Cmd.ReadString("Book_Name");
                BookTemp.Introduce = Cmd.ReadString("Introduce");
                List.Add(BookTemp.BookId, BookTemp);
                Bok.DebugLog.LogCenter.Push(12, 0, $"BookId={BookTemp.BookId}，BookName={BookTemp.BookName}。");
            }
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Book列表，完毕。");

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
        public uint SizeX;
        public uint SizeY;
        public Figure(string shape, string Size)
        {
            Shape = (eShape)Enum.Parse(typeof(eShape), shape);
            int X = Size.IndexOf("×");
            SizeX = Convert.ToUInt32(Size.Substring(0, X));
            SizeY = Convert.ToUInt32(Size.Substring(X + 1));
        }
    }

    public class tAbility
    {
        public uint AbilityId;
        public string Name;
        public string AbiCode;
        public string DescShort;
        public string DescLong;

        public static Dictionary<uint, tAbility> List = new Dictionary<uint, tAbility>();
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Ability列表，开始。");
            List.Clear();
            Cmd.Reading("select * from Ability_T", false);
            while (Cmd.Reading())
            {
                tAbility abiInfo = new tAbility();
                abiInfo.AbilityId = Cmd.ReadUInt("Ability_Id");
                abiInfo.Name = Cmd.ReadString("Ability_Name");
                abiInfo.AbiCode = Cmd.ReadString("Ability_Code");
                abiInfo.DescShort = Cmd.ReadString("Desc_Short");
                abiInfo.DescLong = Cmd.ReadString("Desc_Long");
                List.Add(abiInfo.AbilityId, abiInfo);
                Bok.DebugLog.LogCenter.Push(12, 0, $"AbilityId={abiInfo.AbilityId}，Name={abiInfo.Name}。");
            }
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Ability列表，完毕。");
        }

        public static List<tAbility> ExchangeAbility(string inString)
        {//转化各种如"0,1,2,3"的字符串为List，没有附加信息时写"0"
            List<uint> NumList = new List<uint>();

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
                    NumList.Add(Convert.ToUInt32(String1));
                }
                else
                {
                    NumList.Add(Convert.ToUInt32(String2));
                    String2 = "";
                }
            }

            List<tAbility> ListOutput = new List<tAbility>();
            foreach (uint AbiId in NumList)
            {
                ListOutput.Add(tAbility.List[AbiId]);
            }

            return ListOutput;
        }
    }

    public class tPage
    {
        public static Dictionary<uint, tPage> List = new Dictionary<uint, tPage>();

        public uint tPageId;
        public string tName;
        public tBook tBookBelong;
        public ePageType PageType;
        public uint tRateLevel;
        public string Introduce;
        //public uint tSummonRule;//在数据库找
        public uint tCost;
        public List<tAbility> tAbilityList;
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
            tPageId = Cmd.ReadUInt("Page_ID");
            tName = Cmd.ReadString("Page_Name");
            tBookBelong = tBook.List[Cmd.ReadUInt("Book_Id")];
            PageType = (ePageType)Enum.Parse(typeof(ePageType), Cmd.ReadString("Page_Type"));
            tRateLevel = Cmd.ReadUInt("Rare_Level");
            tCost = Cmd.ReadUInt("Cost");
            //tSummonRule = Cmd.ReadInt("Summon_Rule");
            tAbilityList = tAbility.ExchangeAbility(Cmd.ReadString("Ability_List"));
            tExInfo = Bok.BFC.Date.ExchangeExInfo(Cmd.ReadString("Ex_Info"));

            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Page列表，开始。");
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
                Bok.DebugLog.LogCenter.Push(12, 0, $"导入BookId=BookId");
            }
            Bok.DebugLog.LogCenter.Push(2, 0, $"从数据库加载Page列表，完毕");
        }



    } //tPage

    public class tTheOnePage : tPage
    {
        public uint tLife;
        public uint tAtk;
        public uint tAtkMax;

        //空构建
        public tTheOnePage() { }

        //根据PageId从数据库读取数据，进行构造
        public tTheOnePage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_One_Page_V where Page_Id=" + PageId);
            tLife = Cmd.ReadUInt("Life");
            tAtk = Cmd.ReadUInt("Atk");
            tAtkMax = Cmd.ReadUInt("Atk_Max");
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
        public string ActionObject;

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
        // 埋伏在什么目标上
        public String AttObject;
        // 是否可见
        public Boolean Hide;
        // 能力效果
        public tAbility Ability;

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

    /*
    public class tBuff
    {
        public uint tBuffId;
        public string Name;
        public string DescLong;
        public string DescShort;

        public static Dictionary<uint, tBuff> List = new Dictionary<uint, tBuff>();
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Cmd.Reading("select * from Buff_Type_T", false);
            while (Cmd.Reading())
            {
                tBuff BuffTemp = new tBuff();
                BuffTemp.tBuffId = Cmd.ReadUInt("Buff_Type_Id");
                BuffTemp.Name = Cmd.ReadString("Buff_Name");
                BuffTemp.DescLong = Cmd.ReadString("Desc_Long");
                BuffTemp.DescShort = Cmd.ReadString("Desc_Short");
                List.Add(BuffTemp.tBuffId, BuffTemp);
            }
        }
        //根据tBuffId在List里找
        public static tBuff Find(uint tBuffId)
        {
            if (0 == List.Count)
            {
                throw new Exception("List not build!");
            }

            if (List.ContainsKey(tBuffId))
            {
                return List[tBuffId];
            }
            else
            {
                throw new Exception("Can't find BuffId in List!");
            }
        }

    } */

} // namespace Book
