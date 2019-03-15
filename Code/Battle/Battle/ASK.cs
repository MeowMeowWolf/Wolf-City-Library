using System;
using Battle.Entity;
using Battle.Room;
using Bok.DebugLog;
using Book;

namespace Battle.ASK
{
    public static class Room
    {
        public static GameRoom ParentRoom(Camp Obj)
        {
            return Obj.ParentRoom;
        }
        public static GameRoom ParentRoom(Character Obj)
        {
            return Obj.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(iPage Obj)
        {
            return Obj.Chr.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(mPage Obj)
        {
            return Obj.iPage.Chr.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(AnEntity Obj)
        {
            switch (Obj.EntityType)
            {
                case eEntityType.iPage:
                    return ParentRoom(Obj.Object as iPage);
                case eEntityType.mPage:
                    return ParentRoom(Obj.Object as mPage);
                case eEntityType.mTheOne:
                    return ParentRoom(Obj.Object as mTheOne);
                case eEntityType.mThePlace:
                    return ParentRoom(Obj.Object as mThePlace);
                default:
                    throw (new Exception("惹，还有其他类型的吗？"));
            }
        }

        public static GameRoom ParentRoom(object Obj)
        {
            switch (Obj.GetType().Name)
            {

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

    }// Room

    public static class Map
    {
        //对象能否进入某位置
        public static Boolean jSeatAccessible(GameRoom room, eModeType type, XY seat)
        {
            if (room.Param.MapSize.X > seat.X)
            { return false; }
            if (room.Param.MapSize.Y > seat.Y)
            { return false; }

            if (room.Map[seat.X, seat.Y, (int)type] != null)
            { return false; }

            return true;
        }

        //某类卡牌能否进入某位置
        public static Boolean jSeatAccessible(GameRoom room, ePageType pageType, XY seat)
        {
            eModeType ThingType = (eModeType)Enum.Parse(typeof(eModeType), pageType.ToString());
            return jSeatAccessible(room, ThingType, seat);
        }
    }

    public static class Relation
    {
        public static Boolean Friendly(mPage me, mPage you) //是否为友好目标
        {
            if (Room.ParentRoom(me) != Room.ParentRoom(you))
            { throw new Exception("都不在一个房间里，搞毛啊！"); }

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

    }// Relation

    public static class Other
    {
        public static Boolean jAtkable(Entity.mTheOne me, mTheOne you) //我可以攻击你吗
        {
            if (you == null) { return false; }
            if (Room.ParentRoom(me) != Room.ParentRoom(you))
            { throw new Exception("你是隔壁老王？"); }

            if (me.Alive == false)
            { return false; }
            if (you.Alive == false)
            { return false; }

            if (gDistance(me, you) < 0)
            { return false; }
            if (gAtkDistance(me) < gDistance(me, you))
            { return false; }

            if (Relation.Friendly(me, you))
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
            if (Room.ParentRoom(me) != Room.ParentRoom(you))
            { throw new Exception("世界上最遥远的距离，是我在这里，你在隔壁"); }

            return gDistance(me.Position, you.Position);
        }

        public static uint gAtkDistance(mTheOne one)
        {
            //还要考虑buff，待完善
            return Room.ParentRoom(one).Param.AtkDistance;
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
        


        public static void aMovePosition(mPage thing, XY NewSeat)
        {
            if (!ASK.Map.jSeatAccessible(Room.ParentRoom(thing), thing.ModeType, NewSeat))
            { throw new Exception("非法移民！遣送出境！"); }

            Room.ParentRoom(thing).Map[thing.Position.X, thing.Position.Y, (int)thing.ModeType] = null;
            thing.Position = NewSeat;
            Room.ParentRoom(thing).Map[NewSeat.X, NewSeat.Y, (int)thing.ModeType] = thing;
        }

        

    } // Other

}// namespace ASK