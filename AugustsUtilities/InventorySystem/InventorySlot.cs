using AugustsUtility.ItemSystem;
using System;

namespace AugustsUtility.InventorySystem
{
    [Serializable]
    public class InventorySlot
    {
        public ItemInstance ItemInstance;
        public InventoryComponent Owner
        {
            get; private set;
        }

        public InventorySlot(InventoryComponent owner)
        {
            Owner = owner;
        }

        public bool IsEmpty() => ItemInstance == null || ItemInstance.Count <= 0;

        public void Clear() => ItemInstance = null;
    }
}
