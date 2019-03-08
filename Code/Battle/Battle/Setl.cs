using System;
using System.Collections.Generic;
using Bok.BookSql;
using Book;
using Battle.Entity;
using Battle.Room;

namespace Battle
{
    namespace Setl
    {
        public enum eEventType
        {
            TicToc, Empty, AtkStart, Hurt, AtkEnd, Max
        }
        public enum eEventState
        {
            New, Triggering, Triggered, Doing, Done, Continuing, Dust
        }

        public abstract class SetlEvent //游戏事件基类
        {
            public GameRoom HappenRoom;
            public eEventType EventType;

            public eEventState State;
            public Boolean isTrigger; //是否触发效果
            public Boolean isToDo;    //是否执行事件
            public Boolean isContinue;//是否执行后续内容

            public void Trigger()
            {
                List<ABI.Ability> AbiListTemp = HappenRoom.ToBeTriggered[(uint)State];
                foreach (ABI.Ability abi in AbiListTemp)
                {
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
                HappenRoom = happenRoom;
                EventType = eEventType.TicToc;

                State = eEventState.New;
                Now = happenRoom.TimeNow();
                isTrigger = true;
                isToDo = false;
                isContinue = false;
            }
            public override void ToDo()
            {
            }
            public override void Continue()
            {
            }
        }

        public class AtkStart : SetlEvent // 以Start/End开头的结算事件，不可跳过
        {
            public mTheOne Attacker;
            public mTheOne Casualty;

            public Hurt NextEvent;

            public AtkStart(mTheOne attacker, mTheOne casualty)
            {
                EventType = eEventType.AtkStart;
                isTrigger = true;
                isToDo = false;
                isContinue = true;
                Attacker = attacker;
                Casualty = casualty;
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
                EventType = eEventType.Hurt;
                isTrigger = true;
                isToDo = true;
                isContinue = true;

                Attacker = startAtk.Attacker;
                Casualty = startAtk.Casualty;
                DamagePoints = damagePoints;

                LastEvent = startAtk;
            }

            public override void ToDo()
            {
                ASK.Ask.aLoseLifePoints(Casualty,DamagePoints);
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
                EventType = eEventType.AtkEnd;
                isTrigger = true;
                isToDo = false;
                isContinue = false;

                Attacker = hurt.Attacker;
                Casualty = hurt.Casualty;
                LastEvent = hurt;
            }
        }
        
    } // namespace Selt
    
} // namespace Battle
