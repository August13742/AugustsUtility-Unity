using System;

namespace AugustsUtility.ItemSystem.Capabilities
{
    [Serializable]
    public class ConsumableCapability : ActionableCapability
    {
        public enum ResourceType
        {
            Health, Mana
        }
        public ResourceType TypeToRestore;
        public int Amount;
    }
}
