using System;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public class ItemInstance
    {
        [SerializeField]
        private ItemDefinition _definition;
        public ItemDefinition Definition => _definition;

        [SerializeField]
        private int _count;
        public int Count
        {
            get => _count;
            set => _count = Mathf.Clamp(value, 0, Definition?.StackSize ?? 1);
        }

        public ItemInstance(ItemDefinition definition, int count = 1)
        {
            _definition = definition;
            Count = count;
        }
    }
}
