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
    
    public class tPage
    {
        public static Dictionary<int, tPage> List = new Dictionary<int, tPage>();

        public int tPageId;
        public string tName;
        public tBook tBookBelong;
        public ePageType PageType;
        public int tRateLevel;
        public string Introduce;
        //public int tSummonRule;//在数据库找
        public int tCost;
        public List<ABI.AbiInfo> tAbilityList;
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
            tPageId = Cmd.ReadInt("Page_ID");
            tName = Cmd.ReadString("Page_Name");
            tBookBelong = tBook.List[Cmd.ReadInt("Book_Id")];
            PageType = (ePageType)Enum.Parse(typeof(ePageType), Cmd.ReadString("Page_Type"));
            tRateLevel = Cmd.ReadInt("Rare_Level");
            tCost = Cmd.ReadInt("Cost");
            //tSummonRule = Cmd.ReadInt("Summon_Rule");
            tAbilityList = BFC.ExchangeAbility(Cmd.ReadString("Ability_List"));
            tExInfo = BFC.ExchangeExInfo(Cmd.ReadString("Ex_Info"));
            Introduce = Cmd.ReadString("Introduce");
        }

        public static void ListFromDB(BookSqlCmd Cmd)
        {
            List.Clear();
            Dictionary<string, string> list = Cmd.ReadList("All_Page_T", "Page_Id", "Page_Type");
            foreach (string BookId in list.Keys)
            {
                switch (list[BookId])
                {
                    case "TheOne":
                        tTheOnePage OnePageTemp = new tTheOnePage( Convert.ToInt32(BookId), Cmd );
                        List.Add(OnePageTemp.tPageId, OnePageTemp);
                        break;
                    case "ThePlace":
                        tThePlacePage PlacePageTemp = new tThePlacePage( Convert.ToInt32(BookId), Cmd );
                        List.Add(PlacePageTemp.tPageId, PlacePageTemp);
                        break;
                    case "TheEvent":
                        tTheEventPage TheEventPage = new tTheEventPage( Convert.ToInt32(BookId), Cmd );
                        List.Add(TheEventPage.tPageId, TheEventPage);
                        break;
                    case "TheTime":
                        tTheTimePage TheTimePage = new tTheTimePage( Convert.ToInt32(BookId), Cmd );
                        List.Add(TheTimePage.tPageId, TheTimePage);
                        break;
                }
            }
        }

    } //tPage

    public class tTheOnePage : tPage
    {
        public int tLife;
        public int tAtk;
        public int tAtkMax;

        //空构建
        public tTheOnePage() { }

        //根据PageId从数据库读取数据，进行构造
        public tTheOnePage(int PageId, BookSqlCmd Cmd)
        {
            tPageFromDB(PageId, Cmd);
            Cmd.Reading("select * from The_One_Page_V where Page_Id=" + PageId);
            tLife = Cmd.ReadInt("Life");
            tAtk = Cmd.ReadInt("Atk");
            tAtkMax = Cmd.ReadInt("Atk_Max");
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
        public String ActionObject;

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
        public String AttObject;

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
    
    public class iPage
    {
        public Character Chr;//属于哪个角色

        public int iPageId;
        public string iName;
        public ePageType PageType;
        public tPage tPage;
        public int Region;//0-墓地；1-牌堆；2-手牌；3-场上
        public int Location;//根据Region：(=0)无意义;(=1)牌堆第X位;(=2)手牌中第X位;(=3)所依附实例的id
        public List<Buff> iBuff;
        public List<ABI.Ability> AbilityList;

        public void Construct(tPage Page)
        {
            iName = Page.tName;
            tPage = Page;
            iBuff = new List<Buff>();
            AbilityList = new List<ABI.Ability>();
        }
        public void Add2Chr(Character chr)
        {
            Chr = chr;
            iPageId = BFC.ParentRoom(Chr).Sequence();
            BFC.ParentRoom(Chr).RoomPageList.Add(iPageId, this);
            foreach (ABI.AbiInfo AbiInfo in tPage.tAbilityList)
            {
                ABI.Ability AbilityThis = ABI.Func.Create(AbiInfo.AbiTypeId, null);
                ABI.Func.Endue(eOwnerType.iPage, this, AbilityThis);
            }
            //Region
            //Location
        }

        public static void Create2Chr(Character chr, tPage tPage)
        {
            iPage iPage;
            switch (tPage.PageType)
            {
                case ePageType.TheOne:
                    iPage = new iTheOnePage(tPage as tTheOnePage);
                    iPage.Add2Chr(chr);
                    break;
                case ePageType.ThePlace:
                    iPage = new iThePlacePage(tPage as tThePlacePage);
                    iPage.Add2Chr(chr);
                    break;
                case ePageType.TheEvent:
                    iPage = new iTheEventPage(tPage as tTheEventPage);
                    iPage.Add2Chr(chr);
                    break;
                case ePageType.TheTime:
                    iPage = new iTheTimePage(tPage as tTheTimePage);
                    iPage.Add2Chr(chr);
                    break;
                default:
                    throw new Exception("你Create了个啥？");
            }
        }
    }

    public class iTheOnePage : iPage
    {
        public int iAtk, iAtkMax, iLife, iCost;
        public string iDesc;
        public iTheOnePage() { }
        public iTheOnePage(tTheOnePage Page)
        {
            Construct(Page);
            PageType = ePageType.TheOne;
            iAtk = Page.tAtk;
            iAtkMax = Page.tAtkMax;
            iLife = Page.tLife;
            iCost = Page.tCost;
        }
    }

    public class iThePlacePage : iPage
    {
        public Figure iFigure;
        public iThePlacePage() { }
        public iThePlacePage(tThePlacePage Page)
        {
            Construct(Page);
            PageType = ePageType.ThePlace;
            iFigure = Page.tFigure;
        }
    }

    public class iTheEventPage : iPage
    {
        public String ActionObject;
        public iTheEventPage() { }
        public iTheEventPage(tTheEventPage Page)
        {
            Construct(Page);
            PageType = ePageType.TheEvent;
            ActionObject = Page.ActionObject;
        }
    }

    public class iTheTimePage : iPage
    {
        public String AttObject;
        public iTheTimePage() { }
        public iTheTimePage(tTheTimePage Page)
        {
            Construct(Page);
            PageType = ePageType.TheTime;
            AttObject = Page.AttObject;
        }
    }

    public enum BuffObjectType
    {
        iPage , mPage
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
    { iPage, mPage, mTheOne, mThePlace }
    public struct Owner
    {
        public eOwnerType OwnerType;
        public iPage iPage;
        public mPage mPage;
        public mTheOne mTheOne;
        public mThePlace mThePlace;
    }
    
    //namespace
}
