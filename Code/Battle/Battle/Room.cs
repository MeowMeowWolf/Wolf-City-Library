﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bok.BookSql;
using Book;
using Battle;

namespace Battle.Room
{
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

    public struct RoomParam
    {
        public XY MapSize; //场地大小（单位距离）

        public uint UnitTime; //时间线程多久进行一次结算（毫秒）
        public uint AtkInterval; //攻击间隔（毫秒）
        public uint AtkDistance; //攻击单位距离

        public RoomParam(uint zero)
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
        public Entity.mPage[,,] Map;// X,Y,mPage

        public DateTime BeginTime;
        public DateTime TimeNow() { return System.DateTime.Now; }//先用这样的，以后再搞独立的计时器
        public System.Timers.Timer Timer;
        public AutoAtkTimer AutoAtk;

        private uint sequence;
        public uint Sequence()
        {
            if (sequence == int.MaxValue)
            {
                throw new Exception("序列满了哦");
            }
            sequence++;
            return sequence - 1;
        }

        public Dictionary<uint, Camp> RoomCampList;//全阵营列表
        public Dictionary<uint, Character> RoomChrList;//全角色列表
        public Dictionary<uint, Entity.iPage> RoomPageList;//全卡牌列表
        public Dictionary<uint, Entity.mTheOne> RoomOneList;//全生灵列表
        public Dictionary<uint, Entity.mThePlace> RoomPlaceList;//全环境列表
                                                                //应该不太需要吧 public Dictionary<int, Buff> RoomBuffList;//全buff列表
        public Dictionary<uint, List<ABI.Ability>> ToBeTriggered;
        //public List<ABI.Ability> Pend2Response; // 在注册的待响应效果列表 (作废，有空再删除)

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

            Map = new Entity.mPage[Param.MapSize.X, Param.MapSize.Y, (int)Entity.eModeType.EnumCount];

            RoomCampList = new Dictionary<uint, Camp>();
            RoomChrList = new Dictionary<uint, Character>();
            RoomPageList = new Dictionary<uint, Entity.iPage>();
            RoomOneList = new Dictionary<uint, Entity.mTheOne>();
            RoomPlaceList = new Dictionary<uint, Entity.mThePlace>();
            ToBeTriggered = new Dictionary<uint, List<ABI.Ability>>();
            for (uint i = 0; i < (uint)Setl.eEventType.Max; i++)
            {
                ToBeTriggered.Add(i,new List<ABI.Ability>());
            }
        }
    }

    public class AutoAtkTimer
    {
        GameRoom ParentRoom;
        public Boolean WorkState;
        public Dictionary<uint, AutoOne> AutoOneList;
        public void Doing(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (WorkState == false)
            {
                WorkState = true;
                List<AutoOne> AutoOneListTemp = AutoOneList.Values.ToList();
                foreach (AutoOne One in AutoOneListTemp)
                {
                    if (ASK.Ask.gTimeDifMs(ParentRoom.TimeNow(), One.LastAtkTime) >= ParentRoom.Param.AtkInterval)
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
            AutoOneList = new Dictionary<uint, AutoOne>();
        }

        public void Add(Entity.mTheOne One)
        {
            if (ParentRoom != ASK.Ask.ParentRoom(One))
            { throw new Exception("这里不是你该来的地方"); }

            AutoOneList.Add(One.ModeId, new AutoOne(One));
        }
        public void Remove(Entity.mTheOne One)
        {
            AutoOneList[One.ModeId].Continue = false;
            AutoOneList.Remove(One.ModeId);
        }

        public class AutoOne
        {
            public Entity.mTheOne Me;
            public Entity.mTheOne Target;
            public DateTime LastAtkTime;
            public Boolean Continue;

            void FindTarget()
            {
                List<Entity.mTheOne> ValuesListTemp = ASK.Ask.ParentRoom(Me).RoomOneList.Values.ToList();
                foreach (Entity.mTheOne you in ValuesListTemp)
                {
                    if (ASK.Ask.jAtkable(Me, you))
                    {
                        Target = you;
                    }
                }
            }

            public void TryAtk()
            {
                if (Me.Alive && Continue)
                {
                    if (ASK.Ask.jAtkable(Me))
                    {
                        if (Target == null)
                        { FindTarget(); }
                        if (!ASK.Ask.jAtkable(Me, Target))
                        { FindTarget(); }
                        if (ASK.Ask.jAtkable(Me, Target))
                        {
                            Launch.TheOne.LaunchAttack(Me, Target);
                        }
                    }
                }
            }

            public AutoOne(Entity.mTheOne one)
            {
                Me = one;
                LastAtkTime = ASK.Ask.ParentRoom(Me).TimeNow();
                Continue = true;
            }

        }

    } // AutoAtk

    public class Camp
    {
        public GameRoom ParentRoom;

        public uint Id;
        public string Name;
        public int SP;//(1)绝对友好 (-1)绝对仇恨 (其他)无特殊变化
        public Camp() { }
        public Camp(uint id, string name, int sp)
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

        public uint Id;
        public string Name;
        public Boolean Playable;//可操作性
        public Character(Camp camp, string name, Boolean playable)
        {
            Camp = camp;
            Id = ASK.Ask.ParentRoom(Camp).Sequence();
            Name = name;
            Playable = playable;
            ASK.Ask.ParentRoom(Camp).RoomChrList.Add(Id, this);
        }

        public void LoadMyBook(BookSqlCmd Cmd, uint MyBook)
        {
            Cmd.Reading($"select * from My_Book_T where My_Book={MyBook} Order By Page_Id", false);
            while (Cmd.Reading())
            {
                uint tPageId = Cmd.ReadUInt("Page_Id");
                Entity.iPage.Create2Chr(this, tPage.List[tPageId]);
            }
        }
    }
}