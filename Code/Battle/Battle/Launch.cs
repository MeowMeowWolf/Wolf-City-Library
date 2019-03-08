using System;
using Battle.Entity;
using Battle.Setl;

namespace Battle.Launch
{
    public static class TheOne
    {
        public static void LaunchAttack(mTheOne attacker, mTheOne casualty)
        {
            AtkStart AtkSelt = new AtkStart(attacker, casualty);
            AtkSelt.Run();
        }
    }


}
