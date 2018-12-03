using BookCard;
using BookSql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rule
{
    public class Camp
    {
        public int Id;
        public string Name;
        public int SP;//(1)绝对友好 (-1)绝对仇恨 (其他)无特殊变化
        public Camp(GameRoom room, string name, int sp)
        { Id = room.Sequence(); Name = name; SP = sp; room.AllCampList.Add(Id,this); }
    }

    public class Character
    {
        public int Id;
        public string Name;
        public Camp Camp;//所属阵营
        public Boolean Playable;//可操作性
        public Character(GameRoom room, string name, Camp camp, Boolean playable)
        {
            Id = room.Sequence(); Name = name; Camp = camp; Playable = playable; room.AllChrList.Add(Id,this);
        }
    }
    
    public class RoomParam
    {
        public int AtkInterval;//攻击间隔（毫秒）
        public XY MapSize;//场地大小（单位距离）
        public int AtkDistance;

        public RoomParam()
        {
            AtkInterval = 1000;
            MapSize.X = 50;
            MapSize.Y = 50;
            AtkDistance = 5;
        }
    }

    public class GameRoom
    {
        public RoomParam Param;//参数
        //时间
        public DateTime BeginTime;
        public mThing[,,] Map;// X,Y,mThing

        int sequence;
        public int Sequence()
        {
            if (sequence == 0)
            {
                throw new Exception("序列满了哦");
            }

            int result = sequence;
            if (sequence == int.MaxValue)
            {
                sequence = int.MinValue;
            }
            else
            {
                sequence++;
            }
            return result;
        }

        public Dictionary<int, Camp> AllCampList ;//全阵营列表
        public Dictionary<int, Character> AllChrList ;//全角色列表
        public Dictionary<int, iCard> AllCardList ;//全卡牌列表
        public Dictionary<int, mThing> AllInstList ;//全实例列表
        //应该不太需要吧 public Dictionary<int, Buff> AllBuffList;//全buff列表
        public Dictionary<int, AutoThread> AllThreadList ;//全线程列表

        public GameRoom()
        {
            Param = new RoomParam();
            BeginTime = DateTime.Now;
            sequence = 1;
            Map = new mThing[Param.MapSize.X, Param.MapSize.Y, (int)eThingType.EnumCount];

            AllCampList = new Dictionary<int, Camp>();
            AllChrList = new Dictionary<int, Character>();
            AllCardList = new Dictionary<int, iCard>();
            AllInstList = new Dictionary<int, mThing>();
            // AllBuffList = new Dictionary<int, Buff>();
            AllThreadList = new Dictionary<int, AutoThread>();
        }
    }

    public class Touch
    {
        // [j-judge-判断]
        public static Boolean jInThisRoom(GameRoom room, Camp camp)
        { return room.AllCampList.ContainsValue(camp); }
        public static Boolean jInThisRoom(GameRoom room, Character chr)
        { return room.AllChrList.ContainsValue(chr); }
        public static Boolean jInThisRoom(GameRoom room, iCard card)
        { return room.AllCardList.ContainsValue(card); }
        public static Boolean jInThisRoom(GameRoom room , mThing thing)
        { return room.AllInstList.ContainsValue(thing);}

        public static Boolean jFriendly(GameRoom room, mThing me, mThing you) //是否为友好目标
        {
            if (!jInThisRoom(room, me))
            { return false; }
            if (!jInThisRoom(room, you))
            { return false; }

            switch (you.Chr.Camp.SP)
            {
                case 1:
                    return true;
                case -1:
                    return false;
                default:
                    if (me.Chr.Camp.Equals(you.Chr.Camp))
                    { return true; }
                    else
                    { return false; }
            }
        }

        public static Boolean jAtkable(GameRoom room, mTheOne me, mTheOne you) //我可以攻击你吗
        {
            if (me == null)
            { return false; }
            if (you == null)
            { return false; }
            if (!jInThisRoom(room,me)) 
            { return false; }
            if (!jInThisRoom(room, you))
            { return false; }

            if (me.Alive == false)
            { return false; }
            if(you.Alive == false)
            { return false; }

            if (gDistance(room, me, you) < 0)
            { return false; }
            if (gAtkDistance(room, me) < gDistance(room, me, you))
            { return false; }

            if (jFriendly(room, me, you))
            { return false; }
            //buff判断
            return true;
        }

        public static Boolean jAtkable(GameRoom room, mTheOne me) //我可以发起攻击吗
        {
            if (me==null)
            { return false; }
            if (!jInThisRoom(room, me))
            { return false; }
            if (me.Alive == false)
            { return false; }
            //buff判断
            return true;
        }

        public static Boolean jSeatAccessible(GameRoom room, eThingType ThingType, XY seat) //对象能否进入某位置
        {
            if (room.Param.MapSize.X > seat.X)
            { return false; }
            if (room.Param.MapSize.Y > seat.Y)
            { return false; }

            if (room.Map[seat.X, seat.Y, (int)ThingType] != null)
            { return false; }

            return true;
        }

        public static Boolean jSeatAccessible(GameRoom room, eCardType CardType, XY seat) //某类卡牌能否进入某位置
        {
            if (room.Param.MapSize.X > seat.X)
            { return false; }
            if (room.Param.MapSize.Y > seat.Y)
            { return false; }

            eThingType ThingType = (eThingType)Enum.Parse(typeof(eThingType), CardType.ToString());
            if (room.Map[seat.X, seat.Y, (int)ThingType] != null)
            { return false; }

            return true;
        }

        // [g-get-获取]
        public static float gDistance(XY me, XY you)
        {
            if (me.X < 0 || me.Y<0) { return -1; }
            if (you.X < 0 || you.Y < 0) { return -1; }

            float distance = -1;
            distance = (float)Math.Sqrt(Math.Pow((me.X - you.X), 2) + Math.Pow((me.Y - you.Y), 2));
            return distance;
        }

        public static float gDistance(GameRoom room, mThing me, mThing you)
        {
            if (!jInThisRoom(room, me))
            { return -1; }
            if (!jInThisRoom(room, you))
            { return -1; }

            return gDistance(me.Position, you.Position);
        }

        public static int gAtkDistance(GameRoom room, mTheOne one)
        {
            return room.Param.AtkDistance;
        }

        // [a-action-行为]
        public static Boolean aSummon(GameRoom room, Character chr, iTheOneCard card , XY seat)
        {
            if (!jInThisRoom(room, chr))
            { return false; }
            if (!jInThisRoom(room, card))
            { return false; }
            if (!jSeatAccessible(room, card.CardType, seat))
            { return false; }

            mTheOne TheOne = new mTheOne(card);
            TheOne.ModeId = room.Sequence();
            TheOne.Chr = chr;
            TheOne.Alive = true;
            TheOne.Intelligent = true;
            room.AllInstList.Add(TheOne.ModeId, TheOne);
            aIntoMap(room, TheOne, seat);
            return true;
        }

        public static Boolean aSummon(GameRoom room, Character chr, tTheOneCard card, XY seat)
        {
            if (!jSeatAccessible(room, card.CardType, seat))
            { return false; }

            mTheOne TheOne = new mTheOne(card);
            TheOne.ModeId = room.Sequence();
            TheOne.Chr = chr;
            TheOne.Alive = true;
            TheOne.Intelligent = true;
            room.AllInstList.Add(TheOne.ModeId, TheOne);
            aIntoMap(room, TheOne, seat);
            return true;
        }

        public static void aIntoMap(GameRoom room, mThing thing, XY seat)
        {
            room.Map[seat.X, seat.Y, (int)thing.ThingType] = thing;
            thing.Position = seat;
        }

        public static void aLeaveMap(GameRoom room, mThing thing)
        {
            room.Map[thing.Position.X, thing.Position.Y, (int)thing.ThingType] = null;
            thing.Position.Clean();
        }

        public static void aMovePosition(GameRoom room, mThing thing, XY newseat)
        {
            room.Map[thing.Position.X, thing.Position.Y, (int)thing.ThingType] = null;
            room.Map[newseat.X, newseat.Y, (int)thing.ThingType] = thing;
            thing.Position = newseat;
        }

        public static void aAtk(GameRoom room, mTheOne Attacker, mTheOne Casualty)
        {
            if (Attacker.Alive == true)
            {
                Casualty.mLife = Math.Max(0, (Casualty.mLife - Attacker.mAtk));
                Console.WriteLine($"[{Attacker.Chr.Name}]的<{Attacker.mName}>对[{Casualty.Chr.Name}]的<{Casualty.mName}>造成{Attacker.mAtk}点伤害");
                Console.WriteLine($"[{Casualty.Chr.Name}]的<{Casualty.mName}>当前剩余血量为：{Casualty.mLife}");
                if (Casualty.mLife <= 0)
                {
                    sDeath(room, Casualty);
                    Console.WriteLine($"[{Attacker.Chr.Name}] 的<{Casualty.mName}>挂了");
                }
            }
        }

        // [s-settlement-结算]
        public static void sDeath(GameRoom room, mThing thing)
        {
            thing.mBuff.Clear();
            thing.Alive = false;
            room.Map[thing.Position.X, thing.Position.Y, (int)thing.ThingType] = null;
            thing.Position.Clean();
            room.AllInstList.Remove(thing.ModeId);
            switch (thing.SourceType)
            {
                case 1:
                    thing.iCardSource.Region = 0;
                    break;
                case 2:
                    break;
            }

        }

    }

    public class iCard
    {
        public int iCardId;
        public string iName;
        public eCardType CardType;
        public tCard tCard;
        public Character Chr;//属于哪个角色
        public int Region;//0-墓地；1-牌堆；2-手牌；3-场上
        public int Location;//根据Region：(=0)无意义;(=1)牌堆第X位;(=2)手牌中第X位;(=3)所依附实例的id
        public Dictionary<int,Buff> iBuff = new Dictionary<int, Buff>();

        public void IAA2R(GameRoom room, Character chr, tCard card)
        {
            iCardId = room.Sequence();
            iName = card.tName;
            tCard = card;
            Chr = chr;
            room.AllCardList.Add(iCardId, this);
        }
    }

    public class iTheOneCard : iCard
    {
        public int iAtk, iAtkMax, iLife, iCost;
        public string iDesc;
        //public pic;
        public iTheOneCard() { }
        public iTheOneCard(GameRoom room, Character chr, tTheOneCard card)
        {
            CardType = eCardType.TheOne;
            iAtk = card.tAtk;
            iAtkMax = card.tAtkMax;
            iLife = card.tLife;
            iCost = card.tCost;
            IAA2R(room, chr, card);
        }
    }

    public class iThePlaceCard : iCard
    {
        public Figure iFigure;
        //public pic;
        public iThePlaceCard() { }
        public iThePlaceCard(GameRoom room, Character chr, tThePlaceCard card)
        {
            CardType = eCardType.ThePlace;
            iFigure = card.tFigure;
            IAA2R(room, chr, card);
        }
    }

    public class iTheEventCard : iCard
    {
        public String ActionObject;
        //public pic;
        public iTheEventCard() { }
        public iTheEventCard(GameRoom room, Character chr, tTheEventCard card)
        {
            CardType = eCardType.TheEvent;
            ActionObject = card.ActionObject;
            IAA2R(room, chr, card);
        }
    }

    public class iTheTimeCard : iCard
    {
        public String AttObject;
        //public pic;
        public iTheTimeCard() { }
        public iTheTimeCard(GameRoom room, Character chr, tTheTimeCard card)
        {
            CardType = eCardType.TheTime;
            AttObject = card.AttObject;
            IAA2R(room, chr, card);
        }
    }

    public enum eThingType
    {
        TheOne, ThePlace, EnumCount//由于枚举是从0开始，所以最后一个枚举可以代表该枚举类型的枚举数
    }

    public class mThing
    {
        public int ModeId;
        public eThingType ThingType;//1-TheOne
        public string mName;
        public Boolean WithCard, Alive, Intelligent;
        public Character Chr;
        public XY Position;
        public Dictionary<int, Buff> mBuff = new Dictionary<int, Buff>();
        public int SourceType;//1=iCard,2=tCard,3=待定
        public iCard iCardSource;
        public tCard tCardSource;
    }

    public class mTheOne : mThing
    {
        public int mAtk, mAtkMax, mLife, mLifeMax;
        public mTheOne() { }
        public mTheOne(iTheOneCard Card)
        {
            ThingType = eThingType.TheOne;
            WithCard = true;
            iCardSource = Card;
            mName = Card.iName;
            mAtk = Card.iAtk;
            mAtkMax = Card.iAtkMax;
            mLife = Card.iLife;
            mLifeMax = Card.iLife;
        }
        public mTheOne(tTheOneCard Card)
        {
            ThingType = eThingType.TheOne;
            WithCard = false;
            tCardSource = Card;
            mName = Card.tName;
            mAtk = Card.tAtk;
            mAtkMax = Card.tAtkMax;
            mLife = Card.tLife;
            mLifeMax = Card.tLife;
        }
    }

    public class mThePlace : mThing
    {
        public Figure mFigure;
        public mThePlace() { }
        public mThePlace(iThePlaceCard Card)
        {
            ThingType = eThingType.ThePlace;
            WithCard = true;
            iCardSource = Card;
            mName = Card.iName;
            mFigure = Card.iFigure;
        }
        public mThePlace(tThePlaceCard Card)
        {
            ThingType = eThingType.TheOne;
            WithCard = false;
            tCardSource = Card;
            mName = Card.tName;
            mFigure = Card.tFigure;
        }
    }

    public class AutoThread
    {
        public int Id;
        public GameRoom room;
        public int Type;// 1-自动的普通攻击
        public Task task;
        public Boolean Continue;
        public virtual void Doit() { }
    }

    public class AutoComAtk : AutoThread
    {
        mTheOne me;
        mTheOne target;
        public AutoComAtk(GameRoom rom, mTheOne one)
        {
            room = rom; me = one;
            Id = room.Sequence();
            Type = 1;
            Continue = true;
        }
        public void Begin()
        {
            if (task != null)
            {
                //task.Status = TaskStatus.Running
                task.Wait();
            }
            task = new Task(Doit);
            task.Start();
            room.AllThreadList.Add(Id, this);
        }

        void FindTarget()
        {
            foreach (mTheOne you in room.AllInstList.Values)
            {
                if (Touch.jAtkable(room, me, you))
                {
                    target = you;
                }
            }
        }

        public override void Doit()
        {
            while (me.Alive && Continue)//只有死亡才会停止攻击！
            {
                Thread.Sleep(room.Param.AtkInterval);
                if (Touch.jAtkable(room, me) && Continue)
                {
                    if (!Touch.jAtkable(room, me, target))
                    { FindTarget(); }
                    while (Touch.jAtkable(room, me, target) && Continue)
                    {
                        Touch.aAtk(room, me, target);
                        Thread.Sleep(room.Param.AtkInterval);
                    }
                }
            }
        }
    }

    public class Buff
    {
        public int BuffId;
        public int BuffType;//其实没啥用，bufftype已经在列表的key部分表示了
        public int LastTime;//单位(秒)；特别地，=(-1)时表示永久
        public Dictionary <string, string> BuffInfo = new Dictionary <string, string>();
        public Buff(int inBuffId, int inBuffType, int inLastTime)
        {
            BuffId = inBuffId; BuffType = inBuffType; LastTime = inLastTime;
        }

        public enum eBuffType
        {
            Powerless, Dumbness, Ward, Bleeding, Transfiguration
        }
        public static string[] BuffTypeName = { "Powerless", "Dumbness", "Ward", "Bleeding", "Transfiguration" };
        public static string[] BuffTypeDesc = { "无力", "沉默", "守护", "流血", "变形" };
    }

    

    public struct XY
    {
        public int X, Y;
        public XY(int x, int y)
        { X = x;Y = y; }
        public void Clean()
        {
            X = -1;Y = -1;
        }
    }



    public class OneVsOne
    {
        GameRoom room = new GameRoom();

        OneVsOne(BookSqlCmd cmd)
        {
            Camp C1 = new Camp(room, "红方", 0);
            Camp C2 = new Camp(room, "蓝方", 0);
            Character P1 = new Character(room, "火神", C1, true);
            Character P2 = new Character(room, "海神", C2, false);

            tBook.ListFromDB(cmd);
            tTheOneCard.ListFromDB(cmd);
            foreach (tTheOneCard card in tTheOneCard.List.Values)
            {
                iTheOneCard tempcard1 = new iTheOneCard(room, P1, card);
                iTheOneCard tempcard2 = new iTheOneCard(room, P2, card);
            }
            
        }
    }
}
