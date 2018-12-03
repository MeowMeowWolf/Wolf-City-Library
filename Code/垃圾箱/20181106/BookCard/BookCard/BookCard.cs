using BookSql;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BookCard
{
    public class tBook
    {//书
        public static Dictionary <int,tBook> List = new Dictionary <int, tBook>();
        public int tBookId;
        public string tName;
        public string Introduce;
        object BookCoverPic;
        object CardCoverPic;

        //空构造
        public tBook(){        }
        //入参构造
        public tBook(int inBookId, string inName, string inIntroduce)
        {
            tBookId = inBookId;
            tName = inName;
            Introduce = inIntroduce;
        }
        //根据BookId从数据库读取数据，进行构造
        public tBook(int BookId, BookSqlCmd Cmd)
        {
            Cmd.Reading("select * from Book_T where Book_ID=" + BookId);
            tBookId = Cmd.ReadInt("Book_Id");
            tName = Cmd.ReadString("Book_Name");
            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            BookSqlCmd Cmd2 = Cmd.Clone();
            Cmd.Reading("select * from Book_T", false);
            while ( Cmd.Reading() )
            {
                tBook BookTemp = new tBook(Cmd.ReadInt("Book_ID"), Cmd2);
                List.Add(BookTemp.tBookId, BookTemp);
            }
        }

        //根据BookId在List里找
        public static tBook Find(int BookId)
        {
            if (0 == List.Count)
            {
                throw new Exception("List not build!");
            }

            if (List.ContainsKey(BookId))
            {
                return List[BookId];
            }
            else
            {
                throw new Exception("Can't find BookId in List!");
            }
        }

    }

    public enum eCardType
    {
        TheOne, TheTime, ThePlace, TheEvent, TheLegend
    }
    public enum eRareLevel
    {
        Unreal, Papyrus, SilkBook, JadeBook, Wordless
    }
    public enum eShape
    {
        Circular, Triangle, Rectangle, Hexagon, FivePointedStar, 
    }
    public struct Figure
    {
        public eShape Shape;
        public int SizeX;
        public int SizeY;
        public Figure(string shape,string Size)
        {
            Shape = (eShape)Enum.Parse(typeof(eShape), shape);
            int X = Size.IndexOf("×");
            SizeX = Convert.ToInt32(Size.Substring(0, X));
            SizeY = Convert.ToInt32(Size.Substring(X + 1));
        }
    }

    public static class bEnum
    {
        public static string[] ShapeName = { "Circular", "Triangle", "Rectangle", "Hexagon", "FivePointedStar" };
        public static string[] ShapeDesc = { "圆形", "三角形", "长方形", "六边形", "五角星" };
        public static string[] CardTypeName = { "TheOne", "TheTime", "ThePlace", "TheEvent", "TheLegend" };
        public static string[] CardTypeDesc = { "生灵", "时间", "环境", "事件", "传奇" };
        public static string[] RareLevelName = { "Unreal", "Papyrus", "SilkBook", "JadeBook", "Wordless" };
        public static string[] RareLevelDesc = { "言灵", "草纸", "帛书", "玉简", "无字碑" };
    }


    public class tCard
    {
        public static Dictionary<int, tCard> List = new Dictionary<int, tCard>();

        public int tCardId;
        public string tName;
        public tBook tBookBelong;
        public eCardType CardType;
        public int tRateLevel;
        public string tDescLong;
        public string tDescShort;
        public string Introduce;
        public int tSummonRule;//在数据库找
        public int tCost;
        public string tAbilityList;//在数据库找
        public Dictionary<string,string> tExInfo = new Dictionary<string, string>();
        public string getExMsg(string inKey)
        {
            if (tExInfo.ContainsKey(inKey))
            { return tExInfo[inKey]; }
            else
            { return "none"; };
        }

        public void tCardFromDB(int CardId, BookSqlCmd Cmd)
        {
            Cmd.Reading("select * from All_Card_T where Card_Id=" + CardId);
            tCardId = Cmd.ReadInt("Card_ID");
            tName = Cmd.ReadString("Card_Name");
            tBookBelong = tBook.Find(Cmd.ReadInt("Book_Id"));
            CardType = (eCardType)Enum.Parse(typeof(eCardType), Cmd.ReadString("Card_Type"));
            tRateLevel = Cmd.ReadInt("Rare_Level");
            tCost = Cmd.ReadInt("Cost");
            tSummonRule = Cmd.ReadInt("Summon_Rule");
            tAbilityList = Cmd.ReadString("Ability_List");
            tExInfo = bFunc.ExInfoExchange(Cmd.ReadString("Ex_Info"));
            tDescLong = Cmd.ReadString("Desc_Long");
            tDescShort = Cmd.ReadString("Desc_Short");
            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            BookSqlCmd Cmd2 = Cmd.Clone();
            Cmd.Reading("select * from All_Card_T", false);
            while (Cmd.Reading())
            {
                switch (Cmd.ReadString("Card_Type"))
                {
                    case "TheOne":
                        tTheOneCard OneCardTemp = new tTheOneCard(Cmd.ReadInt("Card_ID"), Cmd2);
                        List.Add(OneCardTemp.tCardId, OneCardTemp);
                        break;
                    case "ThePlace":
                        tThePlaceCard PlaceCardTemp = new tThePlaceCard(Cmd.ReadInt("Card_ID"), Cmd2);
                        List.Add(PlaceCardTemp.tCardId, PlaceCardTemp);
                        break;
                    case "TheEvent":
                        tTheEventCard TheEventCard = new tTheEventCard(Cmd.ReadInt("Card_ID"), Cmd2);
                        List.Add(TheEventCard.tCardId, TheEventCard);
                        break;
                    case "TheTime":
                        tTheTimeCard TheTimeCard = new tTheTimeCard(Cmd.ReadInt("Card_ID"), Cmd2);
                        List.Add(TheTimeCard.tCardId, TheTimeCard);
                        break;
                    default :
                        tCard Card = new tCard();
                        Card.tCardFromDB(Cmd.ReadInt("Card_ID"), Cmd2);
                        List.Add(Card.tCardId, Card);
                        break;
                }
            }
        }
        //根据CardId在List里找
        public static tCard Find(int CardId)
        {
            if (0 == List.Count)
            {
                throw new Exception("List not build!");
            }

            if (List.ContainsKey(CardId))
            {
                return List[CardId];
            }
            else
            {
                throw new Exception("Can't find CardId in List!");
            }
        }

    }

    public class tTheOneCard : tCard
    {
        public int tLife;
        public int tAtk;
        public int tAtkMax;

        //空构建
        public tTheOneCard() { }
        
        //根据CardId从数据库读取数据，进行构造
        public tTheOneCard(int CardId, BookSqlCmd Cmd)
        {
            tCardFromDB(CardId, Cmd);
            Cmd.Reading("select * from The_One_Card_V where Card_Id=" + CardId);
            tLife = Cmd.ReadInt("Life");
            tAtk = Cmd.ReadInt("Atk");
            tAtkMax = Cmd.ReadInt("Atk_Max");
        }
    }

    public class tThePlaceCard : tCard
    {
        public Figure tFigure;

        //空构建
        public tThePlaceCard() { }
        //
        //根据CardId从数据库读取数据，进行构造
        public tThePlaceCard(int CardId, BookSqlCmd Cmd)
        {
            tCardFromDB(CardId, Cmd);
            Cmd.Reading("select * from The_Place_Card_V where Card_Id=" + CardId);
            tFigure = new Figure(Cmd.ReadString("Shape"), Cmd.ReadString("Size"));
        }
    }

    public class tTheEventCard : tCard
    {
        public String ActionObject;

        //空构建
        public tTheEventCard() { }
        //
        //根据CardId从数据库读取数据，进行构造
        public tTheEventCard(int CardId, BookSqlCmd Cmd)
        {
            tCardFromDB(CardId, Cmd);
            Cmd.Reading("select * from The_Event_Card_V where Card_Id=" + CardId);
            ActionObject = Cmd.ReadString("Action_Object");
        }
    }

    public class tTheTimeCard : tCard
    {
        public String AttObject;

        //空构建
        public tTheTimeCard() { }
        //
        //根据CardId从数据库读取数据，进行构造
        public tTheTimeCard(int CardId, BookSqlCmd Cmd)
        {
            tCardFromDB(CardId, Cmd);
            Cmd.Reading("select * from The_Time_Card_V where Card_Id=" + CardId);
            AttObject = Cmd.ReadString("Att_Object");
        }
    }

    public class lSummonRule
    {
        public bool justOK()
        {
            return true;
        }

        public bool justNO()
        {
            return false;
        }
    }

    public class tAbilityBase
    {
        public string AbilityName;
        public int AbilityInsId;
        public virtual void AbilityDo() { }
    }

    public static class bFunc
    {
        public static Dictionary<string, string> ExInfoExchange(string inExInfo)
        {//转化各种如"物种=人类|性别=男|职业=乌干达|0"的字符串为Dictionary ，注意末尾以"|0"结尾，没有附加信息时写"0|0"
            Dictionary<string,string> outExInfo = new Dictionary<string, string>();
            string String1 = inExInfo;
            string String2 = inExInfo;
            int cut1 = 0, cut2 = 0;
            string add1=null, add2=null;

            while (String2.Contains("|"))
            {
                cut1 = String2.IndexOf('|');
                String1 = String2.Substring(0, cut1);
                String2 = String2.Substring(cut1 + 1);

                if (String1.Contains("="))
                {
                    cut2 = String1.IndexOf('=');
                    add1 = String1.Substring(0, cut2);
                    add2 = String1.Substring(cut2 + 1);
                }

                outExInfo.Add(add1, add2);
            }

            return outExInfo;
        }

        public static void Print(tCard card)
        {
            Console.WriteLine("——————————————————");
            Console.WriteLine($"CardId = {card.tCardId}");
            Console.WriteLine($"Card_Name = {card.tName}");
            Console.WriteLine($"Book = {card.tBookBelong.tName}");
            Console.WriteLine($"CardType = {card.CardType.ToString()}");
            Console.WriteLine($"RateLevel = {card.tRateLevel}");
            Console.WriteLine($"DescLong = {card.tDescLong}");
            Console.WriteLine($"DescShort = {card.tDescShort}");
            Console.WriteLine($"Introduce = {card.Introduce}");
            Console.WriteLine($"SummonRule = {card.tSummonRule}");
            Console.WriteLine($"Cost = {card.tCost}");
            Console.WriteLine($"AbilityList = {card.tAbilityList}");
            foreach (string key in card.tExInfo.Keys)
            { Console.Write($"{key}={card.tExInfo[key]}|");}
            Console.WriteLine();
            switch (card.CardType)
            {
                case eCardType.TheOne:
                    tTheOneCard oCard = card as tTheOneCard;
                    Console.WriteLine($"Life = {oCard.tLife}");
                    Console.WriteLine($"Atk = {oCard.tAtk}");
                    Console.WriteLine($"AtkMax = {oCard.tAtkMax}");
                    break;
                case eCardType.ThePlace:
                    tThePlaceCard pCard = card as tThePlaceCard;
                    Console.WriteLine($"Shape = {pCard.tFigure.Shape.ToString()}");
                    Console.WriteLine($"SizeX = {pCard.tFigure.SizeX}");
                    Console.WriteLine($"SizeY = {pCard.tFigure.SizeY}");
                    break;
                case eCardType.TheEvent:
                    tTheEventCard eCard = card as tTheEventCard;
                    Console.WriteLine($"ActionObject = {eCard.ActionObject}");
                    break;
                case eCardType.TheTime:
                    tTheTimeCard tCard = card as tTheTimeCard;
                    Console.WriteLine($"AttObject = {tCard.AttObject}");
                    break;
                default:
                    break;
            }

            Console.WriteLine("——————————————————");
        }
    }
}