using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mix
{

    public static class BFC
    {
        public static T CreateInstance<T>(string nameSpace, string className, object[] parameters)
        {
            //try
            //{
                string fullName = nameSpace + "." + className;//命名空间.类型名
                object ect = Assembly.GetExecutingAssembly().CreateInstance(fullName, true, System.Reflection.BindingFlags.Default, null, parameters, null, null);//加载程序集，创建程序集里面的 命名空间.类型名 实例
                return (T)ect;//类型转换并返回
            //}
            //catch
            //{
                //发生异常，返回类型的默认值
            //    return default(T);
            //}
        }

        public static Dictionary<string, string> ExchangeExInfo(string inExInfo)
        {//转化各种如"物种=人类|性别=男|职业=乌干达"的字符串为Dictionary"
            Dictionary<string, string> outExInfo = new Dictionary<string, string>();
            string String1 = inExInfo;
            string String2 = inExInfo;
            int cut1 = 0, cut2 = 0;
            string add1 = null, add2 = null;

            while (String2.Contains("="))
            {
                if (String2.Contains("|"))
                {
                    cut1 = String2.IndexOf('|');
                    String1 = String2.Substring(0, cut1);
                    String2 = String2.Substring(cut1 + 1);

                    cut2 = String1.IndexOf('=');
                    add1 = String1.Substring(0, cut2);
                    add2 = String1.Substring(cut2 + 1);
                    outExInfo.Add(add1, add2);
                }
                else
                {
                    cut2 = String2.IndexOf('=');
                    add1 = String2.Substring(0, cut2);
                    add2 = String2.Substring(cut2 + 1);
                    outExInfo.Add(add1, add2);
                    String2 = "";
                }

                if (String1.Contains("="))
                {
                    
                }
            }

            return outExInfo;
        }

        public static List<int> ExchangeAbility(string inString)
        {//转化各种如"0,1,2,3"的字符串为List，没有附加信息时写"0"
            List<int> outList = new List<int>();
            string String1 = inString;
            string String2 = inString;
            int cut = 0;
            while (String2.Length > 0)
            {
                if (String2.Contains(","))
                {
                    cut = String2.IndexOf(',');
                    String1 = String2.Substring(0, cut);
                    String2 = String2.Substring(cut + 1);
                    outList.Add( Convert.ToInt32(String1) );
                }
                else
                {
                    outList.Add(Convert.ToInt32(String2));
                    String2 = "";
                }
            }
            return outList;
        }

        public static GameRoom ParentRoom(Camp camp)
        {
            return camp.ParentRoom;
        }
        public static GameRoom ParentRoom(Character chr)
        {
            return chr.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(iCard card)
        {
            return card.Chr.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(mCard mcard)
        {
            return mcard.iCard.Chr.Camp.ParentRoom;
        }
        public static GameRoom ParentRoom(Owner Owner)
        {
            switch (Owner.OwnerType)
            {
                case eOwnerType.iCard:
                    return ParentRoom(Owner.iCard);
                case eOwnerType.mCard:
                    return ParentRoom(Owner.mCard);
                case eOwnerType.mTheOne:
                    return ParentRoom(Owner.mTheOne);
                case eOwnerType.mThePlace:
                    return ParentRoom(Owner.mThePlace);
                default:
                    throw new Exception("非预期的类型");
            }

        }
        public static void Print(tCard card)
        {
            Console.WriteLine("——————————————————");
            Console.WriteLine($"CardId = {card.tCardId}");
            Console.WriteLine($"Card_Name = {card.tName}");
            Console.WriteLine($"Book = {card.tBookBelong.tName}");
            Console.WriteLine($"CardType = {card.CardType.ToString()}");
            Console.WriteLine($"RateLevel = {card.tRateLevel}");
            Console.WriteLine($"DescLong = {card.tDescLong}");
            Console.WriteLine($"DescShort = {card.tDescShort}");
            Console.WriteLine($"Introduce = {card.Introduce}");
            //Console.WriteLine($"SummonRule = {card.tSummonRule}");
            Console.WriteLine($"Cost = {card.tCost}");
            Console.WriteLine($"AbilityList = {card.tAbilityList}");
            foreach (string key in card.tExInfo.Keys)
            { Console.Write($"{key}={card.tExInfo[key]}|"); }
            Console.WriteLine();
            switch (card.CardType)
            {
                case eCardType.TheOne:
                    tTheOneCard oCard = card as tTheOneCard;
                    Console.WriteLine($"Life = {oCard.tLife}");
                    Console.WriteLine($"Atk = {oCard.tAtk}");
                    Console.WriteLine($"AtkMax = {oCard.tAtkMax}");
                    break;
                case eCardType.ThePlace:
                    tThePlaceCard pCard = card as tThePlaceCard;
                    Console.WriteLine($"Shape = {pCard.tFigure.Shape.ToString()}");
                    Console.WriteLine($"SizeX = {pCard.tFigure.SizeX}");
                    Console.WriteLine($"SizeY = {pCard.tFigure.SizeY}");
                    break;
                case eCardType.TheEvent:
                    tTheEventCard eCard = card as tTheEventCard;
                    Console.WriteLine($"ActionObject = {eCard.ActionObject}");
                    break;
                case eCardType.TheTime:
                    tTheTimeCard tCard = card as tTheTimeCard;
                    Console.WriteLine($"AttObject = {tCard.AttObject}");
                    break;
                default:
                    break;
            }

            Console.WriteLine("——————————————————");
        }
    }

    public class Touch
    {
        // [j-judge-判断]
        public static Boolean jFriendly(mCard me, mCard you) //是否为友好目标
        {
            if (BFC.ParentRoom(me) != BFC.ParentRoom(you))
            { throw new Exception("都不在一个房间里，你搞毛啊！"); }

            switch (you.iCard.Chr.Camp.SP)
            {
                case 1:
                    return true;
                case -1:
                    return false;
                default:
                    if (me.iCard.Chr.Camp == you.iCard.Chr.Camp)
                    { return true; }
                    else
                    { return false; }
            }
        }

        public static Boolean jAtkable(mTheOne me, mTheOne you) //我可以攻击你吗
        {
            if (BFC.ParentRoom(me) != BFC.ParentRoom(you))
            { throw new Exception("你是隔壁老王？"); }

            if (me.Alive == false)
            { return false; }
            if (you.Alive == false)
            { return false; }

            if (gDistance(me, you) < 0)
            { return false; }
            if (gAtkDistance(me) < gDistance(me, you))
            { return false; }

            if (jFriendly(me, you))
            { return false; }
            //buff判断
            return true;
        }

        public static Boolean jAtkable(mTheOne me) //我可以发起攻击吗
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

        public static Boolean jSeatAccessible(GameRoom room, eCardType CardType, XY seat) //某类卡牌能否进入某位置
        {
            if (room.Param.MapSize.X < seat.X || seat.X < 0)
            { return false; }
            if (room.Param.MapSize.Y < seat.Y || seat.Y < 0)
            { return false; }

            eModeType ThingType = (eModeType)Enum.Parse(typeof(eModeType), CardType.ToString());
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

        public static float gDistance(mCard me, mCard you)
        {
            if (BFC.ParentRoom(me) != BFC.ParentRoom(you))
            { throw new Exception("世界上最遥远的距离，是我在这里，你在隔壁"); }

            return gDistance(me.Position, you.Position);
        }

        public static int gAtkDistance(mTheOne one)
        {
            //还要考虑buff，待完善
            return BFC.ParentRoom(one).Param.AtkDistance;
        }

        public static int gAtkPoints(mTheOne one)
        {
            //还要考虑buff，待完善
            return one.mAtk;
        }

        public static int gTimeDifMs(DateTime t1, DateTime t2)
        {
            TimeSpan t3 = t1 - t2;
            double dif = t3.TotalMilliseconds;
            int Dif = Convert.ToInt32(dif);
            return Dif;
        }

        // [a-action-行为]
        public static Boolean aSummon(iTheOneCard card, XY seat)
        {
            if (!jSeatAccessible( BFC.ParentRoom(card), card.CardType, seat))
            { return false; }

            mTheOne TheOne = new mTheOne(card);
            TheOne.iCard = card;
            TheOne.ModeId = BFC.ParentRoom(TheOne).Sequence();
            TheOne.Alive = true;
            BFC.ParentRoom(TheOne).RoomOneList.Add(TheOne.ModeId, TheOne);
            aIntoMap(TheOne, seat);

            BFC.ParentRoom(TheOne).AutoAtk.Add(TheOne);
            Console.WriteLine($"[{card.Chr.Name}]召唤了<{card.iName}>");
            return true;
        }
        
        public static void aIntoMap(mCard thing, XY seat)
        {
            BFC.ParentRoom(thing).Map[seat.X, seat.Y, (int)thing.ModeType] = thing;
            thing.Position = seat;
        }

        public static void aLeaveMap(mCard thing)
        {
            BFC.ParentRoom(thing).Map[thing.Position.X, thing.Position.Y, (int)thing.ModeType] = null;
            thing.Position.Clean();
        }

        public static void aMovePosition(mCard thing, XY NewSeat)
        {
            if( !jSeatAccessible(BFC.ParentRoom(thing), thing.ModeType, NewSeat))
            { throw new Exception("非法移民！遣送出境！"); }

            BFC.ParentRoom(thing).Map[thing.Position.X, thing.Position.Y, (int)thing.ModeType] = null;
            thing.Position = NewSeat;
            BFC.ParentRoom(thing).Map[NewSeat.X, NewSeat.Y, (int)thing.ModeType] = thing;
        }

        public static void aAtk(mTheOne Attacker, mTheOne Casualty)
        {
            if (Attacker.Alive == true)
            {
                Casualty.mLife = Math.Max(0, (Casualty.mLife - Attacker.mAtk));
                Console.WriteLine($"[{Attacker.iCard.Chr.Name}]的<{Attacker.mName}>对[{Casualty.iCard.Chr.Name}]的<{Casualty.mName}>造成{Attacker.mAtk}点伤害");
                Console.WriteLine($"[{Casualty.iCard.Chr.Name}]的<{Casualty.mName}>剩余血量：{Casualty.mLife}");
                if (Casualty.mLife <= 0)
                {
                    sDeath(Casualty);
                }
            }
        }

        public static void aLoseLifePoints(mTheOne Casualty, int Points)
        {
            if (Casualty.Alive == true)
            {
                Casualty.mLife = Math.Max(0, (Casualty.mLife - Points));
                Console.WriteLine($"[{Casualty.iCard.Chr.Name}]的<{Casualty.mName}>失去{Points}点生命值，剩余{Casualty.mLife}点生命值。");
                if (Casualty.mLife <= 0)
                {
                    sDeath(Casualty);
                }
            }
        }

        // [s-settlement-结算]
        public static void sDeath(mTheOne TheOne)
        {
            BFC.ParentRoom(TheOne).AutoAtk.Remove(TheOne);
            TheOne.mBuff.Clear();
            TheOne.Alive = false;
            aLeaveMap(TheOne);
            BFC.ParentRoom(TheOne).RoomOneList.Remove(TheOne.ModeId);
            Console.WriteLine($"[{TheOne.iCard.Chr.Name}] 的<{TheOne.mName}>挂了");
            // iCard 的处理
        }


    }
}
