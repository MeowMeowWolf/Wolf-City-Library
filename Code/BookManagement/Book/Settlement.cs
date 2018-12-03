using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mix
{
    public class SE
    {
        public enum eEventType
        {
            TicToc, Empty, StartAtk, Hurt, EndAtk
        }

        public class SetlEvent //注意其子类的命名规则为：se+对应的eEventType
        {
            public Boolean Happening; // null-该事件未发生，true-该事件正在发生，false-该事件已结束
            public eEventType EventType;

        }

        public static void Happen2Room(GameRoom Room, SetlEvent Event)
        {
            Event.Happening = true;

            List<ABI.Ability> AbiListTemp = Room.Pend2Response.ToList();
            foreach (ABI.Ability abi in AbiListTemp)
            {
                if (abi.TriggerEventType == Event.EventType)
                {
                    if (abi.Alive == true)
                    {
                        if (abi.Condition.Judge(Event))
                        {
                            abi.Effect.Launch(Event);
                        }
                    }
                }
            }

            Event.Happening = false;
        }

        public class Toc : SetlEvent
        {
            public static Boolean State;
            public DateTime Now;

            public Toc(DateTime time)
            {
                EventType = eEventType.TicToc;
                Now = time;
            }
        }

        public class StartAtk : SetlEvent // 以Start/End开头的结算事件，不可跳过
        {
            public Boolean Continue;

            public mTheOne Attacker;
            public mTheOne Casualty;

            public Hurt NextEvent;

            public StartAtk(mTheOne attacker, mTheOne casualty)
            {
                EventType = eEventType.StartAtk;
                Continue = true;

                Attacker = attacker;
                Casualty = casualty;
            }
        }

        public class Hurt : SetlEvent
        {
            public Boolean Continue;

            public mTheOne Attacker;
            public mTheOne Casualty;
            public int DamagePoints;

            public StartAtk LastEvent;
            public EndAtk NextEvent;

            public Hurt(StartAtk startAtk)
            {
                EventType = eEventType.Hurt;
                Continue = true;

                Attacker = startAtk.Attacker;
                Casualty = startAtk.Casualty;
                DamagePoints = Touch.gAtkPoints(Attacker);

                LastEvent = startAtk;
                startAtk.NextEvent = this;
            }
        }

        public class EndAtk : SetlEvent
        {
            public mTheOne Attacker;
            public mTheOne Casualty;
            public Hurt LastEvent;

            public EndAtk(Hurt hurt)
            {
                Attacker = hurt.Attacker;
                Casualty = hurt.Casualty;
                LastEvent = hurt;
                hurt.NextEvent = this;
            }
        }

        // SE
    }


    public static class Selt
    {
        public static void Tic(GameRoom room)
        {
            SE.Toc toc = new SE.Toc( room.TimeNow() );
            SE.Happen2Room(room, toc);
        }

        public static void StartAtk(mTheOne attacker, mTheOne casualty)
        {
            Console.WriteLine($"————————————");
            Console.WriteLine($"<{attacker.mName}>向<{casualty.mName}>发起进攻！");

            SE.StartAtk startAtk = new SE.StartAtk(attacker, casualty);
            SE.Happen2Room(BFC.ParentRoom(startAtk.Attacker), startAtk);

            if (startAtk.Continue == true)
            { Hurt(startAtk); }
        }

        public static void Hurt(SE.StartAtk startAtk)
        {
            SE.Hurt hurt = new SE.Hurt(startAtk);
            SE.Happen2Room(BFC.ParentRoom(hurt.Casualty), hurt);
            Console.WriteLine($"<{hurt.Attacker.mName}>对<{hurt.Casualty.mName}>造成{hurt.DamagePoints}点伤害");

            if (hurt.Continue == true)
            {
                Touch.aLoseLifePoints(hurt.Casualty, hurt.DamagePoints);
                EndAtk(hurt);
            }
        }

        public static void EndAtk(SE.Hurt hurt)
        {
            SE.EndAtk endAtk = new SE.EndAtk(hurt);
            SE.Happen2Room(BFC.ParentRoom(endAtk.Casualty), endAtk);
            Console.WriteLine($"————————————");
        }



        // Selt
    }

}
