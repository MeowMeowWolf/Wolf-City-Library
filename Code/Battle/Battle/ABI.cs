using System;
using Battle.Entity;
using Battle.Setl;
using Book;
using Bok.DebugLog;

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
        public static T CreateInstance<T>(string nameSpace, string className, object[] parameters)
        {
            string fullName = nameSpace + "." + className;
            object obj = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(fullName, true, System.Reflection.BindingFlags.Default, null, parameters, null, null);//加载程序集，创建程序集里面的 命名空间.类型名 实例
            return (T)obj;
        }

        public static Ability Create(uint abiTypeId, object[] Parameters)
        {
            tAbility info = tAbility.List[abiTypeId];
            Ability ability = CreateInstance<Ability>("Battle.ABI.Skill", "a"+info.AbiCode, Parameters);
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
            ASK.Room.ParentRoom(ability.Owner).ToBeTriggered[(uint)ability.TriggerEventType].Add(ability);
        }

    }//Func



    namespace Skill
    {
        public class cGouLi : Condition
        {
            public override Boolean Judge(SetlEvent Event)
            {
                LogCenter.Push(13, 0, $"判断本次事件是否触发技能“苟利”。");
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
                AtkStart thisEvent = Event as AtkStart;
                LogCenter.Push(7, 0, $"<{thisEvent.Casualty.iPage.Chr.Name}>的[{thisEvent.Casualty.mName}]预感到危险的来临，为自己增加1点生命值！");
                thisEvent.Casualty.mLife = Math.Min(thisEvent.Casualty.mLife + 1, thisEvent.Casualty.mLifeMax);
                LogCenter.Push(7, 0, $"<{thisEvent.Casualty.iPage.Chr.Name}>的[{thisEvent.Casualty.mName}]当前生命值：{thisEvent.Casualty.mLife}");
            }
        }

        public class aGouLi : Ability
        {

            public override void Init()
            {
                TriggerEventType = eEventType.AtkStart;
                Condition = new cGouLi();
                Condition.ParentAbi = this;
                Effect = new eGouLi();
                Effect.ParentAbi = this;
            }

        }
    }
}//namespace ABI
