using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    // Register this handler for the base ConsumableCapability type
    [HandlesCapability(typeof(ConsumableCapability))]
    public sealed class ConsumableHandler : CapabilityHandler<ConsumableCapability>
    {
        public override void Execute(ItemInstance instance, ConsumableCapability cap, object context = null)
        {
            if (context is not GameObject target)
            {
                Debug.LogError($"[ConsumableHandler] Execute failed: Context provided was not a GameObject.");
                return;
            }

            var user = target;

            Debug.Log($"Using {instance.Definition?.DisplayName} ({cap.GetType().Name}) on {target.name}");

            // The polymorphic magic happens here - cap.Apply() will call the correct override
            cap.Apply(user, target);

            // Handle cooldown logic
            if (cap.Cooldown > 0)
            {
                Debug.Log($"Item has {cap.Cooldown} second cooldown");
                // Start cooldown timer logic here
            }

            Debug.Log($"Finished using {cap.GetType().Name}");
        }
    }
}
