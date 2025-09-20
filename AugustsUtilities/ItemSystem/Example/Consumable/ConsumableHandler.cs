using AugustsUtility.ItemSystem.Capabilities;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    public class ConsumableHandler : CapabilityHandler<ConsumableCapability>
    {
        public override void Execute(ItemInstance instance, ConsumableCapability cap, object context = null)
        {
            if (context is not GameObject user)
                return;

            switch (cap.TypeToRestore)
            {
                case ConsumableCapability.ResourceType.Health:
                    if (user.TryGetComponent<HealthComponent>(out var health))
                    {
                        health.Restore(cap.Amount);
                        Debug.Log($"{user.name} restored {cap.Amount} Health.");
                    }
                    break;

                case ConsumableCapability.ResourceType.Mana:
                    if (user.TryGetComponent<ManaComponent>(out var mana))
                    {
                        mana.Restore(cap.Amount);
                        Debug.Log($"{user.name} restored {cap.Amount} Mana.");
                    }
                    break;
            }

            // remove item from inventory
        }
    }
}
