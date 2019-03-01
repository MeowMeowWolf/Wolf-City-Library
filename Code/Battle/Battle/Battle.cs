using System;
using Bok;
using Book;

namespace Battle
{
    namespace Compon
    {
        public class Camp
        {
            public GameRoom ParentRoom;

            public int Id;
            public string Name;
            public int SP;//(1)绝对友好 (-1)绝对仇恨 (其他)无特殊变化
            public Camp() { }
            public Camp(int id, string name, int sp)
            { Id = id; Name = name; SP = sp; }
            public Camp(GameRoom room, string name, int sp)
            {
                ParentRoom = room;
                Id = ParentRoom.Sequence(); Name = name; SP = sp;
                ParentRoom.RoomCampList.Add(Id, this);
            }
            public static void Create2Room(GameRoom room, string name, int sp)
            {
                Camp NewCamp = new Camp();
                NewCamp.ParentRoom = room;
                NewCamp.Id = NewCamp.ParentRoom.Sequence(); NewCamp.Name = name; NewCamp.SP = sp;
                NewCamp.ParentRoom.RoomCampList.Add(NewCamp.Id, NewCamp);
            }
        }

        public class Character
        {
            public Camp Camp;//所属阵营

            public int Id;
            public string Name;
            public Boolean Playable;//可操作性
            public Character(Camp camp, string name, Boolean playable)
            {
                Camp = camp;
                Id = BFC.ParentRoom(Camp).Sequence();
                Name = name;
                Playable = playable;
                BFC.ParentRoom(Camp).RoomChrList.Add(Id, this);
            }

            public void LoadMyBook(BookSqlCmd Cmd, int MyBook)
            {
                Cmd.Reading($"select * from My_Book_T where My_Book={MyBook} Order By Page_Id", false);
                while (Cmd.Reading())
                {
                    int tPageId = Cmd.ReadInt("Page_Id");
                    iPage.Create2Chr(this, tPage.List[tPageId]);
                }
            }
        }
    }

    namespace Entity
    {
        public class iPage
        {
            public Compon.Character Chr;//属于哪个角色

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
        } // iPage

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
            iPage, mPage
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

    } // namespace Entity
    

}
