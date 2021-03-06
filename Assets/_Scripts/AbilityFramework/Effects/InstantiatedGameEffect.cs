using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public class InstantiatedGameEffect
    {
        public GameEffect GameEffect { get; private set; }
        public float StartDelay { get; private set; }
        public float RemainingTime { get; private set; }
        public float TotalTime { get; private set; }
        public float Period { get; private set; }
        public float TimeUntilPeriodTick { get; private set; }

        public AbilitySystemController Source;
        public AbilitySystemController Target;


        public InstantiatedGameEffect(GameEffect effect, AbilitySystemController source, AbilitySystemController target)
        {
            GameEffect = effect;
            StartDelay = effect.Delay;
            TotalTime = effect.Duration;
            RemainingTime = TotalTime;
            Period = effect.Period;
            TimeUntilPeriodTick = Period;
            Source = source;
            Target = target;
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
