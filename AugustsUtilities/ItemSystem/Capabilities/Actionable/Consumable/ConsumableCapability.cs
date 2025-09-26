using System;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    /// <summary>
    /// Base class for consumable item capabilities.
    /// Each specific consumable effect should inherit from this.
    /// </summary>
    [Serializable]
    public abstract class ConsumableCapability : ActionableCapability
    {
        [Tooltip("Time in seconds before this item can be used again.")]
        public float Cooldown = 0f;

        /// <summary>
        /// Execute the consumable effect.
        /// </summary>
        /// <param name="user">The GameObject using the item</param>
        /// <param name="target">The GameObject being targeted (can be the same as user)</param>
        public abstract void Apply(GameObject user, GameObject target);
    }
}
