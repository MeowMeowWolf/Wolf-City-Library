using System;
using System.Collections.Generic;
using System.Linq;

namespace Mix
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

        public void LoadPersonalBook(BookSqlCmd Cmd, int PersonalBookId)
        {
            Cmd.Reading($"select * from Personal_Book_T where Personal_Book_Id={PersonalBookId}", false);
            while (Cmd.Reading())
            {
                int tCardId = Cmd.ReadInt("Card_Id");
                iCard.Create2Chr(this, tCard.List[tCardId]);
            }
        }
    }

    public enum eModeType
    {
        TheOne, ThePlace, EnumCount//由于枚举是从0开始，所以最后一个枚举可以代表该枚举类型的枚举数
    }


    public struct XY
    {
        public int X, Y;
        public XY(int x, int y)
        { X = x; Y = y; }
        public void Clean()
        {
            X = -1; Y = -1;
        }
    }

    public class mCard
    {
        public iCard iCard;

        public int ModeId;
        public eModeType ModeType;
        public string mName;
        public Boolean WithCard, Alive;
        public XY Position;
        public Dictionary<int, Buff> mBuff = new Dictionary<int, Buff>();
        public List<ABI.Ability> AbilityList = new List<ABI.Ability>();
    }

    public class mTheOne : mCard //后续加入角色支持
    {
        public int mAtk, mAtkMax, mLife, mLifeMax;
        public mTheOne() { }
        public mTheOne(iTheOneCard Card)
        {
            ModeType = eModeType.TheOne;
            WithCard = true;
            iCard = Card;
            mName = Card.iName;
            mAtk = Card.iAtk;
            mAtkMax = Card.iAtkMax;
            mLife = Card.iLife;
            mLifeMax = Card.iLife;
        }
    }

    public class mThePlace : mCard
    {
        public Figure mFigure;
        public mThePlace() { }
        public mThePlace(iThePlaceCard Card)
        {
            ModeType = eModeType.ThePlace;
            WithCard = true;
            iCard = Card;
            mName = Card.iName;
            mFigure = Card.iFigure;
        }
    }


    public class RoomParam
    {
        public XY MapSize; //场地大小（单位距离）

        public int UnitTime; //时间线程多久进行一次结算（毫秒）
        public int AtkInterval; //攻击间隔（毫秒）
        public int AtkDistance; //攻击单位距离

        public RoomParam()
        {
            MapSize.X = 50;
            MapSize.Y = 50;
            UnitTime = 40;//单位时间为40毫秒，每秒钟25个单位时间
            AtkInterval = 3000;
            AtkDistance = 5;
        }
    }

    public class GameRoom
    {
        public RoomParam Param;//参数
        public mCard[,,] Map;// X,Y,mCard

        public DateTime BeginTime;
        public DateTime TimeNow() { return System.DateTime.Now; }//先用这样的，以后再搞独立的计时器
        public System.Timers.Timer Timer;
        public AutoAtkTimer AutoAtk;

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

        public Dictionary<int, Camp> RoomCampList;//全阵营列表
        public Dictionary<int, Character> RoomChrList;//全角色列表
        public Dictionary<int, iCard> RoomCardList;//全卡牌列表
        public Dictionary<int, mTheOne> RoomOneList;//全生灵列表
        public Dictionary<int, mThePlace> RoomPlaceList;//全环境列表
        //应该不太需要吧 public Dictionary<int, Buff> RoomBuffList;//全buff列表
        public List<ABI.Ability> Pend2Response; // 在注册的待响应效果列表

        public GameRoom()
        {
            Param = new RoomParam();
            sequence = 1;

            BeginTime = DateTime.Now;
            Timer = new System.Timers.Timer(Param.UnitTime);
            AutoAtk = new AutoAtkTimer(this);
            Timer.Elapsed += new System.Timers.ElapsedEventHandler(AutoAtk.Doing);
            Timer.AutoReset = true;
            Timer.Start();


            Map = new mCard[Param.MapSize.X, Param.MapSize.Y, (int)eModeType.EnumCount];

            RoomCampList = new Dictionary<int, Camp>();
            RoomChrList = new Dictionary<int, Character>();
            RoomCardList = new Dictionary<int, iCard>();
            RoomOneList = new Dictionary<int, mTheOne>();
            RoomPlaceList = new Dictionary<int, mThePlace>();
            // RoomBuffList = new Dictionary<int, Buff>();
            Pend2Response = new List<ABI.Ability>();
        }
    }
    
    public class AutoAtkTimer
    {
        GameRoom ParentRoom;
        public Boolean WorkState;
        public Dictionary<int, AutoOne> AutoOneList;
        public void Doing(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (WorkState == false)
            {
                WorkState = true;
                List<AutoOne> AutoOneListTemp = AutoOneList.Values.ToList();
                foreach (AutoOne One in AutoOneListTemp)
                {
                    if (Touch.gTimeDifMs(ParentRoom.TimeNow(), One.LastAtkTime) >= ParentRoom.Param.AtkInterval)
                    {
                        One.TryAtk();
                        One.LastAtkTime = ParentRoom.TimeNow();
                    }
                }
                WorkState = false;
            }
        }

        public AutoAtkTimer(GameRoom room)
        {
            ParentRoom = room;
            WorkState = false;
            AutoOneList = new Dictionary<int, AutoOne>();
        }

        public void Add(mTheOne One)
        {
            if (ParentRoom != BFC.ParentRoom(One))
            { throw new Exception("这里不是你该来的地方"); }

            AutoOneList.Add(One.ModeId, new AutoOne(One));
        }
        public void Remove(mTheOne One)
        {
            AutoOneList[One.ModeId].Continue = false;
            AutoOneList.Remove(One.ModeId);
        }

        public class AutoOne
        {
            public mTheOne Me;
            public mTheOne Target;
            public DateTime LastAtkTime;
            public Boolean Continue;

            void FindTarget()
            {
                List<mTheOne> ValuesListTemp = BFC.ParentRoom(Me).RoomOneList.Values.ToList();
                foreach (mTheOne you in ValuesListTemp)
                {
                    if (Touch.jAtkable(Me, you))
                    {
                        Target = you;
                    }
                }
            }

            public void TryAtk()
            {
                if (Me.Alive && Continue)
                {
                    if (Touch.jAtkable(Me))
                    {
                        if (Target == null)
                        { FindTarget(); }
                        if (!Touch.jAtkable(Me, Target))
                        { FindTarget(); }
                        if (Touch.jAtkable(Me, Target))
                        {
                            //Touch.aAtk(Me, Target);
                            Selt.StartAtk(Me, Target);
                        }
                    }
                }
            }

            public AutoOne(mTheOne one)
            {
                Me = one;
                LastAtkTime = BFC.ParentRoom(Me).TimeNow();
                Continue = true;
            }

        }

    }

}
