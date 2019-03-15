using System;
using System.Collections.Generic;
using Bok.BookSql;
using Book;
using Battle.Entity;
using Battle.Room;
using Bok.DebugLog;

namespace Battle
{
    namespace Setl
    {
        public enum eEventType
        {
            Empty, TicToc, Summon, AtkStart, Hurt, AtkEnd, Death, Max
        }
        public enum eEventState
        {
            New, Triggering, Triggered, Doing, Done, Continuing, Dust
        }

        public abstract class SetlEvent //游戏事件基类
        {
            public eEventType EventType;
            public Boolean isTrigger; //是否触发效果
            public Boolean isToDo;    //是否执行事件
            public Boolean isContinue;//是否执行后续内容

            public GameRoom HappenRoom;
            public eEventState State;

            public void Trigger()
            {
                List<ABI.Ability> AbiListTemp = HappenRoom.ToBeTriggered[(uint)State];
                foreach (ABI.Ability abi in AbiListTemp)
                {
                    LogCenter.Push(9, 0, $"判断{abi.AbiInfo.Name}是否符合条件");
                    if (abi.Alive == true)
                    {
                        if (abi.Condition.Judge(this))
                        {
                            abi.Effect.Implement(this);
                        }
                    }
                }
            }
            public virtual void ToDo()
            {
            }
            public virtual void Continue()
            {
            }
            public void Run()
            {
                if (isTrigger)
                {
                    State = eEventState.Triggering;
                    Trigger();
                    State = eEventState.Triggered;
                }
                if (isToDo)
                {
                    State = eEventState.Doing;
                    ToDo();
                    State = eEventState.Done;
                }
                if (isContinue)
                {
                    State = eEventState.Continuing;
                    Continue();
                }
                State = eEventState.Dust;
            }
        } // SetlEvent

        public class TicToc : SetlEvent
        {
            public DateTime Now;

            public TicToc(GameRoom happenRoom)
            {
                EventType = eEventType.TicToc;
                isTrigger = true;
                isToDo = false;
                isContinue = false;

                HappenRoom = happenRoom;
                State = eEventState.New;

                Now = happenRoom.TimeNow();
            }
            public override void ToDo()
            {
            }
            public override void Continue()
            {
            }
        }

        public class Summon:SetlEvent
        {
            Character Chr;
            public iTheOnePage iOne;
            public mTheOne mOne;
            public XY Seat;

            public Summon(Character chr, iTheOnePage Page, XY seat)
            {
                EventType = eEventType.Summon;
                isTrigger = true;
                isToDo = true;
                isContinue = false;

                HappenRoom = ASK.Room.ParentRoom(Page);
                State = eEventState.New;

                Chr = chr;
                iOne = Page;
                Seat = seat;
            }

            public override void ToDo()
            {
                if (!ASK.Map.jSeatAccessible(ASK.Room.ParentRoom(iOne), iOne.PageType, Seat))
                {
                    mTheOne TheOne = new mTheOne(iOne);
                    TheOne.iPage = iOne;
                    TheOne.ModeId = ASK.Room.ParentRoom(TheOne).Sequence();
                    TheOne.Alive = true;
                    ASK.Room.ParentRoom(TheOne).RoomOneList.Add(TheOne.ModeId, TheOne);
                    Launch.Map.IntoMap(TheOne, Seat);

                    ASK.Room.ParentRoom(TheOne).AutoAtk.Add(TheOne);
                    LogCenter.Push(6, 0, $"<{iOne.Chr.Name}>召唤了[{iOne.iName}]");
                }
            }
        }

        public class AtkStart : SetlEvent // 以Start/End开头的结算事件，不可跳过
        {
            public mTheOne Attacker;
            public mTheOne Casualty;

            public Hurt NextEvent;

            public AtkStart(mTheOne attacker, mTheOne casualty)
            {
                LogCenter.Push(11, 0, $"创建攻击开始事件:Attacker={attacker.mName}。");
                EventType = eEventType.AtkStart;
                isTrigger = true;
                isToDo = false;
                isContinue = true;

                HappenRoom = ASK.Room.ParentRoom(attacker);
                State = eEventState.New;
                Attacker = attacker;
                Casualty = casualty;
                LogCenter.Push(11, 0, $"创建攻击开始事件:Attacker={attacker.mName}，成功。");
            }
            public override void ToDo()
            {
            }
            public override void Continue()
            {
                uint damagePoints = Attacker.mAtk;
                NextEvent = new Hurt(this, damagePoints);
                NextEvent.Run();
            }
        }

        public class Hurt : SetlEvent
        {
            public mTheOne Attacker;
            public mTheOne Casualty;
            public uint DamagePoints;

            public AtkStart LastEvent;
            public AtkEnd NextEvent;

            public Hurt(AtkStart startAtk,uint damagePoints)
            {
                LogCenter.Push(11, 0, $"创建伤害事件:Attacker={startAtk.Attacker.mName}，Casualty={startAtk.Casualty.mName}。");
                EventType = eEventType.Hurt;
                isTrigger = true;
                isToDo = true;
                isContinue = true;

                HappenRoom = ASK.Room.ParentRoom(startAtk.Attacker);
                State = eEventState.New;

                Attacker = startAtk.Attacker;
                Casualty = startAtk.Casualty;
                DamagePoints = damagePoints;

                LastEvent = startAtk;
                LogCenter.Push(11, 0, $"创建伤害事件:Attacker={startAtk.Attacker.mName}，Casualty={startAtk.Casualty.mName}，成功。");
            }

            public override void ToDo()
            {
                if (Casualty.Alive == true)
                {
                    LogCenter.Push(7, 0, $"<{Attacker.iPage.Chr.Name}>的[{Attacker.mName}]对<{Casualty.iPage.Chr.Name}>的[{Casualty.mName}]造成{DamagePoints}点伤害。");
                    LogCenter.Push(7, 0, $"<{Casualty.iPage.Chr.Name}>的[{Casualty.mName}]剩余生命值：{Casualty.mLife}。");
                    Casualty.mLife = Math.Max(0, (Casualty.mLife - DamagePoints));
                    if (Casualty.mLife <= 0)
                    {
                        Death death = new Death(Casualty);
                        death.Run();
                    }
                }
            }

            public override void Continue()
            {
                NextEvent = new AtkEnd(this);
                NextEvent.Run();
            }
        }

        public class AtkEnd : SetlEvent
        {
            public mTheOne Attacker;
            public mTheOne Casualty;
            public Hurt LastEvent;

            public AtkEnd(Hurt hurt)
            {
                LogCenter.Push(11, 0, $"创建攻击结束事件:Attacker={hurt.Attacker.mName}，Casualty={hurt.Casualty.mName}。");
                EventType = eEventType.AtkEnd;
                isTrigger = true;
                isToDo = false;
                isContinue = false;

                HappenRoom = ASK.Room.ParentRoom(hurt.Attacker);
                State = eEventState.New;

                Attacker = hurt.Attacker;
                Casualty = hurt.Casualty;
                LastEvent = hurt;
                LogCenter.Push(11, 0, $"创建攻击结束事件:Attacker={hurt.Attacker.mName}，Casualty={hurt.Casualty.mName}，成功。");
            }
        }

        public class Death : SetlEvent
        {
            public mTheOne DeadOne;

            public Death(mTheOne deadOne)
            {
                LogCenter.Push(11, 0, $"创建死亡事件:DeadOne={deadOne.mName}。");
                EventType = eEventType.Death;
                isTrigger = true;
                isToDo = true;
                isContinue = false;

                HappenRoom = ASK.Room.ParentRoom(deadOne);
                State = eEventState.New;

                DeadOne = deadOne;
            }

            public override void ToDo()
            {
                LogCenter.Push(16, 0, $"<{DeadOne.iPage.Chr.Name}>的[{DeadOne.mName}]不幸阵亡。");
                ASK.Room.ParentRoom(DeadOne).AutoAtk.Remove(DeadOne);
                DeadOne.mBuff.Clear();
                DeadOne.Alive = false;
                Launch.Map.LeaveMap(DeadOne);
            }
        }

    } // namespace Selt
    
} // namespace Battle
