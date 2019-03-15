using System;
using System.Collections.Generic;
using Book;

namespace Battle.Entity
{
    public abstract class iPage
    {
        public Room.Character Chr;//属于哪个角色

        public uint iPageId;
        public string iName;
        public ePageType PageType;
        public tPage tPage;
        public uint Region;//0-墓地；1-牌堆；2-手牌；3-场上
        public uint Location;//根据Region：(=0)无意义;(=1)牌堆第X位;(=2)手牌中第X位;(=3)所依附实例的id
        public List<Buff> iBuff;
        public List<ABI.Ability> AbilityList;

        public void Init(tPage Page)
        {
            iName = Page.tName;
            tPage = Page;
            iBuff = new List<Buff>();
            AbilityList = new List<ABI.Ability>();
        }
        public void Add2Chr(Room.Character chr)
        {
            Chr = chr;
            iPageId = ASK.Room.ParentRoom(Chr).Sequence();
            ASK.Room.ParentRoom(Chr).RoomPageList.Add(iPageId, this);
            foreach (Book.tAbility abiInfo in tPage.tAbilityList)
            {
                ABI.Ability AbilityThis = ABI.Func.Create(abiInfo.AbilityId, null);
                ABI.Func.Endue( new AnEntity(eEntityType.iPage, this) , AbilityThis);
                Bok.DebugLog.LogCenter.Push(10, 0, $"对{this.iName}赋予技能:{AbilityThis.AbiInfo.Name}");
            }
        }

        public static iPage Create2Chr(Room.Character chr, tPage tPage)
        {
            iPage ipage;
            switch (tPage.PageType)
            {
                case ePageType.TheOne:
                    ipage = new iTheOnePage(tPage as tTheOnePage);
                    ipage.Add2Chr(chr);
                    break;
                case ePageType.ThePlace:
                    ipage = new iThePlacePage(tPage as tThePlacePage);
                    ipage.Add2Chr(chr);
                    break;
                case ePageType.TheEvent:
                    ipage = new iTheEventPage(tPage as tTheEventPage);
                    ipage.Add2Chr(chr);
                    break;
                case ePageType.TheTime:
                    ipage = new iTheTimePage(tPage as tTheTimePage);
                    ipage.Add2Chr(chr);
                    break;
                default:
                    throw new Exception("你Create了个啥？");
            }
            return ipage;
        }
    } // iPage

    public class iTheOnePage : iPage
    {
        public uint iAtk, iAtkMax, iLife, iCost;
        public string iDesc;
        public iTheOnePage() { }
        public iTheOnePage(tTheOnePage Page)
        {
            Init(Page);
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
            Init(Page);
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
            Init(Page);
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
            Init(Page);
            PageType = ePageType.TheTime;
            AttObject = Page.AttObject;
        }
    }

    public enum eBuffType
    {
        Disarm,Silence,
    }

    public class Buff
    {
        public eBuffType BuffType;
        public AnEntity Owner;
        public int LastTime;//单位(毫秒)；特别地，(-1)表示永久
        public DateTime BegintTime;
        public uint Level; //level高的覆盖低的
        public Dictionary<string, string> BuffInfo = new Dictionary<string, string>();
        public string Name;
        public string Desc;

        public Buff(eBuffType buffType, AnEntity owner, int inLastTime)
        {
            BuffType = buffType;
            Owner = owner;
            LastTime = inLastTime;
            BegintTime = (ASK.Room.ParentRoom(owner)).TimeNow();

        }
    }


    public abstract class mPage
    {
        public iPage iPage;

        public uint ModeId;
        public eModeType ModeType;
        public string mName;
        public Boolean WithPage, Alive;
        public Room.XY Position;
        public Dictionary<int, Buff> mBuff = new Dictionary<int, Buff>();
        public List<ABI.Ability> AbilityList = new List<ABI.Ability>();
    }

    public class mTheOne : mPage //后续加入角色支持
    {
        public uint mAtk, mAtkMax, mLife, mLifeMax;
        public mTheOne() { }
        public mTheOne(iTheOnePage Page)
        {
            ModeType = eModeType.TheOne;
            WithPage = true;
            iPage = Page;
            mName = Page.iName;
            mAtk = Page.iAtk;
            mAtkMax = Page.iAtkMax;
            mLife = Page.iLife;
            mLifeMax = Page.iLife;
        }
    }

    public class mThePlace : mPage
    {
        public Figure mFigure;
        public mThePlace() { }
        public mThePlace(iThePlacePage Page)
        {
            ModeType = eModeType.ThePlace;
            WithPage = true;
            iPage = Page;
            mName = Page.iName;
            mFigure = Page.iFigure;
        }
    }

    public enum eModeType
    {
        TheOne, ThePlace, EnumCount//由于枚举是从0开始，所以最后一个枚举可以代表该枚举类型的枚举数
    }

    public enum eEntityType
    {
        iPage, mPage, mTheOne, mThePlace
    }
    public class AnEntity
    {
        public eEntityType EntityType;
        public object Object;
        public AnEntity(eEntityType entityType,iPage ipage)
        {
            EntityType = entityType;
            Object = ipage;
        }
        public AnEntity(eEntityType entityType, mPage mpage)
        {
            EntityType = entityType;
            Object = mpage;
        }
    }

}// namespace Entity
