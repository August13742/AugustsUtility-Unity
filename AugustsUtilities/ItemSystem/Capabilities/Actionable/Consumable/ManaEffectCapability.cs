using System;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public class ManaEffectCapability : ConsumableCapability
    {
        public int Amount = 50;
        public bool IsInstant = true;

        [Tooltip("If not instant, duration in seconds")]
        public float Duration = 5f;

        public override void Apply(GameObject user, GameObject target)
        {
            Debug.Log($"Healing {target.name} for {Amount} HP");

            // logic here
            // Example: target.GetComponent<ManaComponent>()?.Heal(HealAmount);

            if (IsInstant)
            {
                // Apply immediately
            }
            else
            {
                // Start coroutine for over time
            }
        }
    }
}   
