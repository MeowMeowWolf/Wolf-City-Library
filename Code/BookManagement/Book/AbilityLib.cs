using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mix
{
    public class ABI
    {
        public static Dictionary<int, AbiInfo> AbiInfoList = new Dictionary<int, AbiInfo>();
        public class AbiInfo
        {
            public int AbiTypeId;
            public string Name;
            public string AbiCode;
            public string DescShort;
            public string DescLong;
        }

        public class Condition
        {
            public Ability ParentAbi;
            public virtual Boolean Judge(SE.SetlEvent Event) { return false; }
        }
        public class Effect
        {
            public Ability ParentAbi;
            public virtual void Launch(SE.SetlEvent Event) { }
        }
        public class Ability
        {
            public Owner Owner;
            public AbiInfo AbiInfo;

            public Boolean Alive;
            public SE.eEventType TriggerEventType;
            public Condition Condition;
            public Effect Effect;

            public virtual void Construct()
            {
                TriggerEventType = SE.eEventType.Empty;
                Condition = new Condition();
                Condition.ParentAbi = this;
                Effect = new Effect();
                Effect.ParentAbi = this;
            }
        }

        public class Func
        {
            public static void ListFromDB(BookSqlCmd Cmd)
            {
                ABI.AbiInfoList.Clear();
                Cmd.Reading("select * from Ability_T", false);
                while (Cmd.Reading())
                {
                    AbiInfo abiInfo = new AbiInfo();
                    abiInfo.AbiTypeId = Cmd.ReadInt("Abi_Type_Id");
                    abiInfo.Name = Cmd.ReadString("Abi_Name");
                    abiInfo.AbiCode = Cmd.ReadString("Abi_Code");
                    abiInfo.DescShort = Cmd.ReadString("Desc_Short");
                    abiInfo.DescLong = Cmd.ReadString("Desc_Long");
                    ABI.AbiInfoList.Add(abiInfo.AbiTypeId, abiInfo);
                }
            }

            public static Ability Create(int abiTypeId, object[] Parameters)
            {
                AbiInfo info = ABI.AbiInfoList[abiTypeId];
                string CodeName = "ABI+Instance+a" + info.AbiCode; // 类中类在反射时用'+'而非'.'
                Ability ability = BFC.CreateInstance<Ability>("Mix", CodeName, Parameters);
                if (ability == null) { Console.WriteLine($"{CodeName}实例化失败"); }
                ability.AbiInfo = info;
                ability.Construct();
                return ability;
            }

            public static void Endue(eOwnerType ownertype, object owner, Ability ability)
            {
                ability.Owner.OwnerType = ownertype;
                switch (ability.Owner.OwnerType)
                {
                    case eOwnerType.iCard:
                        ability.Owner.iCard = (iCard)owner;
                        ability.Owner.iCard.AbilityList.Add(ability);
                        break;
                    case eOwnerType.mCard:
                        ability.Owner.mCard = (mCard)owner;
                        ability.Owner.mCard.AbilityList.Add(ability);
                        break;
                    case eOwnerType.mTheOne:
                        ability.Owner.mTheOne = (mTheOne)owner;
                        ability.Owner.mTheOne.AbilityList.Add(ability);
                        break;
                    case eOwnerType.mThePlace:
                        ability.Owner.mThePlace = (mThePlace)owner;
                        ability.Owner.mThePlace.AbilityList.Add(ability);
                        break;
                    default:
                        throw new Exception("非预期的类型");
                }
                ability.Alive = true;
                BFC.ParentRoom(ability.Owner).Pend2Response.Add(ability);
            }

        }//Func

        public class Instance
        {
            public class cGouLi : Condition
            {
                public override Boolean Judge(SE.SetlEvent Event)
                {
                    SE.StartAtk thisEvent = Event as SE.StartAtk;
                    if (thisEvent.Casualty.iCard != ParentAbi.Owner.iCard)
                    { return false; }
                    
                    if (thisEvent.Casualty.Alive != true)
                    { return false; }

                    return true;
                }
            }
            public class eGouLi : Effect
            {
                public override void Launch(SE.SetlEvent Event)
                {
                    SE.StartAtk thisEvent = Event as SE.StartAtk;
                    Console.WriteLine($"<{thisEvent.Casualty.mName}> 预感到危险的来临，为自己增加1点生命值！({thisEvent.Casualty.mLife}->{thisEvent.Casualty.mLife + 1})");
                    thisEvent.Casualty.mLife = Math.Min(thisEvent.Casualty.mLife + 1, thisEvent.Casualty.mLifeMax);

                }
            }

            public class aGouLi : Ability
            {

                public override void Construct()
                {
                    TriggerEventType = SE.eEventType.StartAtk;
                    Condition = new cGouLi();
                    Condition.ParentAbi = this;
                    Effect = new eGouLi();
                    Effect.ParentAbi = this;
                }

            }


        }//Instance

        

    }//Abi

}//namespace
