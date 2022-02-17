using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public class InstantiatedGameEffect
    {
        public GameEffect GameEffect { get; private set; }
        public float RemainingTime { get; private set; }
        public float TotalTime { get; private set; }
        public float Period { get; private set; }
        public float TimeUntilPeriodTick { get; private set; }

        public InstantiatedGameEffect(GameEffect effect)
        {
            GameEffect = effect;
            TotalTime = effect.Duration;
            RemainingTime = TotalTime;
            Period = effect.Period;
            TimeUntilPeriodTick = Period;
        }

        public bool DoTick(float deltaTime)
        {
            RemainingTime -= deltaTime;
            TimeUntilPeriodTick -= deltaTime;
            if (TimeUntilPeriodTick <= 0)
            {
                TimeUntilPeriodTick = Period;
                if(Period > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
