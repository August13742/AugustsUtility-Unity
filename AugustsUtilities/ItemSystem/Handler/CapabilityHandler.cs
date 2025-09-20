using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    public abstract class CapabilityHandler<TCap> : ICapabilityHandler where TCap : ActionableCapability
    {
        // Execute one actionable capability against some context (can define a richer context later)
        public abstract void Execute(ItemInstance instance, TCap cap, object context = null);
    }
}
