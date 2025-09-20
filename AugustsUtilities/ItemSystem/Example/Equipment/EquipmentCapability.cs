using System;

namespace AugustsUtility.ItemSystem
{
    // A passive capability that does not inherit from ActionableCapability.
    // This means it has no handler and cannot be "executed".
    [Serializable]
    public class EquipmentCapability : ItemCapability
    {
        public int Damage = 5;
        public float AttackSpeed = 1.2f;
    }
}
