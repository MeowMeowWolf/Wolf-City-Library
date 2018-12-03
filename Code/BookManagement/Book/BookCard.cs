using System;
using System.Collections.Generic;

namespace Mix
{

    public class tBook
    {//书
        public static Dictionary<int, tBook> List = new Dictionary<int, tBook>();
        public int tBookId;
        public string tName;
        public string Introduce;
        
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Cmd.Reading("select * from Book_T", false);
            while (Cmd.Reading())
            {
                tBook BookTemp = new tBook();
                BookTemp.tBookId = Cmd.ReadInt("Book_Id");
                BookTemp.tName = Cmd.ReadString("Book_Name");
                BookTemp.Introduce = Cmd.ReadString("Introduce");
                List.Add(BookTemp.tBookId, BookTemp);
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
        public Figure(string shape, string Size)
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
        //public int tSummonRule;//在数据库找
        public int tCost;
        public List<int> tAbilityList;
        public Dictionary<string, string> tExInfo = new Dictionary<string, string>();
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
            tBookBelong = tBook.List[Cmd.ReadInt("Book_Id")];
            CardType = (eCardType)Enum.Parse(typeof(eCardType), Cmd.ReadString("Card_Type"));
            tRateLevel = Cmd.ReadInt("Rare_Level");
            tCost = Cmd.ReadInt("Cost");
            //tSummonRule = Cmd.ReadInt("Summon_Rule");
            tAbilityList = BFC.ExchangeAbility(Cmd.ReadString("Ability_List"));
            tExInfo = BFC.ExchangeExInfo(Cmd.ReadString("Ex_Info"));
            tDescLong = Cmd.ReadString("Desc_Long");
            tDescShort = Cmd.ReadString("Desc_Short");
            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Dictionary<string, string> list = Cmd.ReadList("All_Card_T", "Card_Id", "Card_Type");
            foreach (string BookId in list.Keys)
            {
                switch (list[BookId])
                {
                    case "TheOne":
                        tTheOneCard OneCardTemp = new tTheOneCard( Convert.ToInt32(BookId), Cmd );
                        List.Add(OneCardTemp.tCardId, OneCardTemp);
                        break;
                    case "ThePlace":
                        tThePlaceCard PlaceCardTemp = new tThePlaceCard( Convert.ToInt32(BookId), Cmd );
                        List.Add(PlaceCardTemp.tCardId, PlaceCardTemp);
                        break;
                    case "TheEvent":
                        tTheEventCard TheEventCard = new tTheEventCard( Convert.ToInt32(BookId), Cmd );
                        List.Add(TheEventCard.tCardId, TheEventCard);
                        break;
                    case "TheTime":
                        tTheTimeCard TheTimeCard = new tTheTimeCard( Convert.ToInt32(BookId), Cmd );
                        List.Add(TheTimeCard.tCardId, TheTimeCard);
                        break;
                }
            }
        }

    } //tCard

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
    
    public class iCard
    {
        public Character Chr;//属于哪个角色

        public int iCardId;
        public string iName;
        public eCardType CardType;
        public tCard tCard;
        public int Region;//0-墓地；1-牌堆；2-手牌；3-场上
        public int Location;//根据Region：(=0)无意义;(=1)牌堆第X位;(=2)手牌中第X位;(=3)所依附实例的id
        public List<Buff> iBuff;
        public List<ABI.Ability> AbilityList;

        public void Construct(tCard card)
        {
            iName = card.tName;
            tCard = card;
            iBuff = new List<Buff>();
            AbilityList = new List<ABI.Ability>();
        }
        public void Add2Chr(Character chr)
        {
            Chr = chr;
            iCardId = BFC.ParentRoom(Chr).Sequence();
            BFC.ParentRoom(Chr).RoomCardList.Add(iCardId, this);
            foreach (int AbiId in tCard.tAbilityList)
            {
                ABI.Ability AbilityThis = ABI.Func.Create(AbiId, null);
                ABI.Func.Endue(eOwnerType.iCard, this, AbilityThis);
            }
            //Region
            //Location
        }

        public static void Create2Chr(Character chr, tCard tcard)
        {
            iCard icard;
            switch (tcard.CardType)
            {
                case eCardType.TheOne:
                    icard = new iTheOneCard(tcard as tTheOneCard);
                    icard.Add2Chr(chr);
                    break;
                case eCardType.ThePlace:
                    icard = new iThePlaceCard(tcard as tThePlaceCard);
                    icard.Add2Chr(chr);
                    break;
                case eCardType.TheEvent:
                    icard = new iTheEventCard(tcard as tTheEventCard);
                    icard.Add2Chr(chr);
                    break;
                case eCardType.TheTime:
                    icard = new iTheTimeCard(tcard as tTheTimeCard);
                    icard.Add2Chr(chr);
                    break;
                default:
                    throw new Exception("你Create了个啥？");
            }
        }
    }

    public class iTheOneCard : iCard
    {
        public int iAtk, iAtkMax, iLife, iCost;
        public string iDesc;
        public iTheOneCard() { }
        public iTheOneCard(tTheOneCard card)
        {
            Construct(card);
            CardType = eCardType.TheOne;
            iAtk = card.tAtk;
            iAtkMax = card.tAtkMax;
            iLife = card.tLife;
            iCost = card.tCost;
        }
    }

    public class iThePlaceCard : iCard
    {
        public Figure iFigure;
        public iThePlaceCard() { }
        public iThePlaceCard(tThePlaceCard card)
        {
            Construct(card);
            CardType = eCardType.ThePlace;
            iFigure = card.tFigure;
        }
    }

    public class iTheEventCard : iCard
    {
        public String ActionObject;
        public iTheEventCard() { }
        public iTheEventCard(tTheEventCard card)
        {
            Construct(card);
            CardType = eCardType.TheEvent;
            ActionObject = card.ActionObject;
        }
    }

    public class iTheTimeCard : iCard
    {
        public String AttObject;
        public iTheTimeCard() { }
        public iTheTimeCard(tTheTimeCard card)
        {
            Construct(card);
            CardType = eCardType.TheTime;
            AttObject = card.AttObject;
        }
    }

    public enum BuffObjectType
    {
        iCard , mCard
    }

    public class Buff
    {
        public Owner Owner;

        public tBuff tBuffType;
        public int Level; //level高的覆盖低的
        public DateTime BegintTime;
        public int LastTime;//单位(毫秒)；特别地，=(-1)时表示永久
        public Dictionary<string, string> BuffInfo = new Dictionary<string, string>();

        public Buff(tBuff inBuffType, int inLastTime)
        {
            tBuffType = inBuffType; LastTime = inLastTime;
        }
       
    }

    public class tBuff
    {
        public int tBuffId;
        public string Name;
        public string DescLong;
        public string DescShort;

        public static Dictionary<int, tBuff> List = new Dictionary<int, tBuff>();
        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Cmd.Reading("select * from Buff_Type_T", false);
            while (Cmd.Reading())
            {
                tBuff BuffTemp = new tBuff();
                BuffTemp.tBuffId = Cmd.ReadInt("Buff_Type_Id");
                BuffTemp.Name = Cmd.ReadString("Buff_Name");
                BuffTemp.DescLong = Cmd.ReadString("Desc_Long");
                BuffTemp.DescShort = Cmd.ReadString("Desc_Short");
                List.Add(BuffTemp.tBuffId, BuffTemp);
            }
        }
        //根据tBuffId在List里找
        public static tBuff Find(int tBuffId)
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
                throw new Exception("Can't find BookId in List!");
            }
        }

    }


    public enum eOwnerType
    { iCard, mCard, mTheOne, mThePlace }
    public struct Owner
    {
        public eOwnerType OwnerType;
        public iCard iCard;
        public mCard mCard;
        public mTheOne mTheOne;
        public mThePlace mThePlace;
    }
    
    //namespace
}
