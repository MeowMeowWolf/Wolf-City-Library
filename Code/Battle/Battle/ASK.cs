using System;
using Battle.Entity;
using Battle.Room;
using Bok.DebugLog;
using Book;

namespace Battle.ASK
{
    public static class Ask
    {
        public static GameRoom ParentRoom(object Obj)
        {
            switch (Obj.GetType().Name)
            {
                case "Camp":
                    return ((Camp)Obj).ParentRoom;
                case "Character":
                    return ((Character)Obj).Camp.ParentRoom;
                case "iPage":
                    return ((iPage)Obj).Chr.Camp.ParentRoom; 
                case "mPage":
                    return ((mPage)Obj).iPage.Chr.Camp.ParentRoom;
                case "AnEntity":
                    return ParentRoom(((AnEntity)Obj).Object);
                default:
                    throw (new Exception("ParentRoom，入参Obj非预期的类型"));
            }

        }

        public static Boolean Friendly(mPage me, mPage you) //是否为友好目标
        {
            if (ASK.Ask.ParentRoom(me) != ASK.Ask.ParentRoom(you))
            { throw new Exception("都不在一个房间里，你搞毛啊！"); }

            switch (you.iPage.Chr.Camp.SP)
            {
                case 1:
                    return true;
                case -1:
                    return false;
                default:
                    if (me.iPage.Chr.Camp == you.iPage.Chr.Camp)
                    { return true; }
                    else
                    { return false; }
            }
        }


        public static Boolean jAtkable(Entity.mTheOne me, mTheOne you) //我可以攻击你吗
        {
            if (you == null) { return false; }
            if (ParentRoom(me) != ASK.Ask.ParentRoom(you))
            { throw new Exception("你是隔壁老王？"); }

            if (me.Alive == false)
            { return false; }
            if (you.Alive == false)
            { return false; }

            if (gDistance(me, you) < 0)
            { return false; }
            if (gAtkDistance(me) < gDistance(me, you))
            { return false; }

            if (Friendly(me, you))
            { return false; }
            //buff判断
            return true;
        }

        public static Boolean jAtkable(Entity.mTheOne me) //我可以发起攻击吗
        {
            if (me == null)
            { return false; }
            if (me.Alive == false)
            { return false; }
            //buff判断
            return true;
        }

        public static Boolean jSeatAccessible(GameRoom room, eModeType type, XY seat) //对象能否进入某位置
        {
            if (room.Param.MapSize.X > seat.X)
            { return false; }
            if (room.Param.MapSize.Y > seat.Y)
            { return false; }

            if (room.Map[seat.X, seat.Y, (int)type] != null)
            { return false; }

            return true;
        }

        public static Boolean jSeatAccessible(GameRoom room, ePageType PageType, XY seat) //某类卡牌能否进入某位置
        {
            if (room.Param.MapSize.X < seat.X || seat.X < 0)
            { return false; }
            if (room.Param.MapSize.Y < seat.Y || seat.Y < 0)
            { return false; }

            eModeType ThingType = (eModeType)Enum.Parse(typeof(eModeType), PageType.ToString());
            if (room.Map[seat.X, seat.Y, (int)ThingType] != null)
            { return false; }

            return true;
        }

        // [g-get-获取]
        public static float gDistance(XY me, XY you)
        {
            if (me.X < 0 || me.Y < 0) { return -1; }
            if (you.X < 0 || you.Y < 0) { return -1; }

            float distance = -1;
            distance = (float)Math.Sqrt(Math.Pow((me.X - you.X), 2) + Math.Pow((me.Y - you.Y), 2));
            return distance;
        }

        public static float gDistance(mPage me, mPage you)
        {
            if (ASK.Ask.ParentRoom(me) != ASK.Ask.ParentRoom(you))
            { throw new Exception("世界上最遥远的距离，是我在这里，你在隔壁"); }

            return gDistance(me.Position, you.Position);
        }

        public static uint gAtkDistance(mTheOne one)
        {
            //还要考虑buff，待完善
            return ASK.Ask.ParentRoom(one).Param.AtkDistance;
        }

        public static uint gAtkPoints(mTheOne one)
        {
            //还要考虑buff，待完善
            return one.mAtk;
        }

        public static uint gTimeDifMs(DateTime t1, DateTime t2)
        {
            TimeSpan t3 = t1 - t2;
            double dif = t3.TotalMilliseconds;
            return Convert.ToUInt32(dif);
        }

        // [a-action-行为]
        public static Boolean aSummon(iTheOnePage Page, XY seat)
        {
            if (!jSeatAccessible(ASK.Ask.ParentRoom(Page), Page.PageType, seat))
            { return false; }

            mTheOne TheOne = new mTheOne(Page);
            TheOne.iPage = Page;
            TheOne.ModeId = ASK.Ask.ParentRoom(TheOne).Sequence();
            TheOne.Alive = true;
            ASK.Ask.ParentRoom(TheOne).RoomOneList.Add(TheOne.ModeId, TheOne);
            aIntoMap(TheOne, seat);

            ASK.Ask.ParentRoom(TheOne).AutoAtk.Add(TheOne);
            LogCenter.Push(new DebugInfo(10, "Info", $"[{Page.Chr.Name}]召唤了<{Page.iName}>"));
            Console.WriteLine($"[{Page.Chr.Name}]召唤了<{Page.iName}>");
            return true;
        }

        public static void aIntoMap(mPage thing, XY seat)
        {
            ASK.Ask.ParentRoom(thing).Map[seat.X, seat.Y, (int)thing.ModeType] = thing;
            thing.Position = seat;
        }

        public static void aLeaveMap(mPage thing)
        {
            ASK.Ask.ParentRoom(thing).Map[thing.Position.X, thing.Position.Y, (int)thing.ModeType] = null;
            thing.Position.Clean();
        }

        public static void aMovePosition(mPage thing, XY NewSeat)
        {
            if (!jSeatAccessible(ASK.Ask.ParentRoom(thing), thing.ModeType, NewSeat))
            { throw new Exception("非法移民！遣送出境！"); }

            ASK.Ask.ParentRoom(thing).Map[thing.Position.X, thing.Position.Y, (int)thing.ModeType] = null;
            thing.Position = NewSeat;
            ASK.Ask.ParentRoom(thing).Map[NewSeat.X, NewSeat.Y, (int)thing.ModeType] = thing;
        }

        public static void aAtk(mTheOne Attacker, mTheOne Casualty)
        {
            if (Attacker.Alive == true)
            {
                Casualty.mLife = Math.Max(0, (Casualty.mLife - Attacker.mAtk));
                Console.WriteLine($"[{Attacker.iPage.Chr.Name}]的<{Attacker.mName}>对[{Casualty.iPage.Chr.Name}]的<{Casualty.mName}>造成{Attacker.mAtk}点伤害");
                Console.WriteLine($"[{Casualty.iPage.Chr.Name}]的<{Casualty.mName}>剩余血量：{Casualty.mLife}");
                if (Casualty.mLife <= 0)
                {
                    sDeath(Casualty);
                }
            }
        }

        public static void aLoseLifePoints(mTheOne Casualty, uint Points)
        {
            if (Casualty.Alive == true)
            {
                Casualty.mLife = Math.Max(0, (Casualty.mLife - Points));
                Console.WriteLine($"[{Casualty.iPage.Chr.Name}]的<{Casualty.mName}>失去{Points}点生命值，剩余{Casualty.mLife}点生命值。");
                if (Casualty.mLife <= 0)
                {
                    sDeath(Casualty);
                }
            }
        }

        // [s-settlement-结算]
        public static void sDeath(mTheOne TheOne)
        {
            ASK.Ask.ParentRoom(TheOne).AutoAtk.Remove(TheOne);
            TheOne.mBuff.Clear();
            TheOne.Alive = false;
            aLeaveMap(TheOne);
            ASK.Ask.ParentRoom(TheOne).RoomOneList.Remove(TheOne.ModeId);
            Console.WriteLine($"[{TheOne.iPage.Chr.Name}]的<{TheOne.mName}>挂了");
            // iPage 的处理
        }

    } // class Ask

}// namespace ASK