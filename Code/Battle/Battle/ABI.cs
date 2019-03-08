using System;
using Battle.Entity;
using Battle.Setl;
using Book;

namespace Battle.ABI
{
    public class Condition
    {
        public Ability ParentAbi;
        public virtual Boolean Judge(Setl.SetlEvent Event) { return false; }
    }
    public class Effect
    {
        public Ability ParentAbi;
        public virtual void Implement(Setl.SetlEvent Event) { }
    }
    public class Ability
    {
        public tAbility AbiInfo;
        public Setl.eEventType TriggerEventType;
        public Condition Condition;
        public Effect Effect;

        public Boolean Alive;
        public AnEntity Owner;

        public virtual void Init()
        {
            // 设置TriggerEventType
            // 配置Condition
            // 配置Effect
            Alive = true;
        }

    }

    public class Func
    {
        public static Ability Create(uint abiTypeId, object[] Parameters)
        {
            tAbility info = tAbility.List[abiTypeId];
            Ability ability = Bok.BFC.Factory.CreateInstance<Ability>("ABI.Skill", info.AbiCode, Parameters);
            if (ability == null) { throw (new Exception("能力实例化失败")); }
            ability.AbiInfo = info;
            ability.Init();
            return ability;
        }

        public static void Endue(AnEntity anEntity, Ability ability)
        {
            ability.Owner = anEntity;
            switch (ability.Owner.EntityType)
            {
                case eEntityType.iPage:
                    ((iPage)anEntity.Object).AbilityList.Add(ability);
                    break;
                case eEntityType.mPage:
                    ((mPage)anEntity.Object).AbilityList.Add(ability);
                    break;
                case eEntityType.mTheOne:
                    ((mTheOne)anEntity.Object).AbilityList.Add(ability);
                    break;
                case eEntityType.mThePlace:
                    ((mThePlace)anEntity.Object).AbilityList.Add(ability);
                    break;
            }
            ability.Alive = true;
            ASK.Ask.ParentRoom(ability.Owner).ToBeTriggered[(uint)ability.TriggerEventType].Add(ability);
        }

    }//Func



    namespace Skill
    {
        public class cGouLi : Condition
        {
            public override Boolean Judge(SetlEvent Event)
            {
                AtkStart thisEvent = Event as AtkStart;
                if (thisEvent.Casualty.iPage != (iPage)ParentAbi.Owner.Object)
                { return false; }

                if (thisEvent.Casualty.Alive != true)
                { return false; }

                return true;
            }
        }
        public class eGouLi : Effect
        {
            public override void Implement(SetlEvent Event)
            {
                SE.StartAtk thisEvent = Event as SE.StartAtk;
                Console.WriteLine($"<{thisEvent.Casualty.mName}> 预感到危险的来临，为自己增加1点生命值！({thisEvent.Casualty.mLife}->{thisEvent.Casualty.mLife + 1})");
                thisEvent.Casualty.mLife = Math.Min(thisEvent.Casualty.mLife + 1, thisEvent.Casualty.mLifeMax);

            }
        }

        public class aGouLi : Ability
        {

            public override void Init()
            {
                TriggerEventType = Setl.eEventType.StartAtk;
                Condition = new cGouLi();
                Condition.ParentAbi = this;
                Effect = new eGouLi();
                Effect.ParentAbi = this;
            }

        }
    }
}//namespace ABI
