using System;
using Battle.Entity;
using Battle.Room;
using Battle.Setl;

namespace Battle.Launch
{
    public static class TheOne
    {
        public static void Attack(mTheOne attacker, mTheOne casualty)
        {
            Bok.DebugLog.LogCenter.Push(7, 0, $"<{attacker.iPage.Chr}>的[{attacker.mName}]对<{casualty.iPage.Chr}>的[{casualty.mName}]发起进攻");
            Setl.AtkStart AtkSelt = new AtkStart(attacker, casualty);
            AtkSelt.Run();
        }

        public static void Summon(Character chr, iTheOnePage page, XY seat)
        {
            Bok.DebugLog.LogCenter.Push(6,0,$"{chr.Name}发起召唤");
            Setl.Summon summon = new Summon(chr, page, seat);
            summon.Run();
        }
    }

    public static class Map
    {
        public static void IntoMap(mPage thing, XY seat)
        {
            ASK.Room.ParentRoom(thing).Map[seat.X, seat.Y, (int)thing.ModeType] = thing;
            thing.Position = seat;
        }

        public static void LeaveMap(mTheOne mtheOne)
        {
            ASK.Room.ParentRoom(mtheOne).RoomOneList.Remove(mtheOne.ModeId);
            ASK.Room.ParentRoom(mtheOne).Map[mtheOne.Position.X, mtheOne.Position.Y, (int)mtheOne.ModeType] = null;
            mtheOne.Position.Clean();
        }

    }
}
