using AugustsUtility.ItemSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugustsUtility.InventorySystem
{
    public enum Category
    {
        Uncategorized, Player, NPC, StorageContainer, Merchant, QuestItemHolder
    }

    public class InventoryComponent : MonoBehaviour
    {
        public static event Action<InventorySlot> OnSlotUpdated;

        // Properties are now publicly gettable but privately settable.
        [field: SerializeField]
        public Category Category
        {
            get; private set;
        }
        [field: SerializeField] public int Size { get; private set; } = 28;

        public InventorySlot[] Slots
        {
            get; private set;
        }
        private ItemDatabase _database;

        public void Initialize(Category category, int size)
        {
            Category = category;
            Size = size;


            Slots = new InventorySlot[Size];
            for (int i = 0; i < Size; i++)
            {
                Slots[i] = new InventorySlot(this);
            }
        }

        internal void NotifySlotUpdated(InventorySlot slot)
        {
            OnSlotUpdated?.Invoke(slot);
        }

        private void Awake()
        {
            _database = Resources.Load<ItemDatabase>("ItemDatabase");

            Initialize(Category, Size);
        }

        private void OnEnable() => InventoryManager.Instance?.Register(this);
        private void OnDisable() => InventoryManager.Instance?.Unregister(this);

        public int AddItem(string itemID, int amount)
        {
            if (amount <= 0)
                return amount;
            var definition = _database.GetByID(itemID);
            if (definition == null)
                return amount;

            int remaining = amount;

            // First pass: fill existing stacks
            foreach (var slot in Slots.Where(s => !s.IsEmpty() && s.ItemInstance.Definition.ID == itemID))
            {
                int canAdd = definition.StackSize - slot.ItemInstance.Count;
                int toAdd = Mathf.Min(remaining, canAdd);

                slot.ItemInstance.Count += toAdd;
                remaining -= toAdd;
                NotifySlotUpdated(slot);

                if (remaining == 0)
                    return 0;
            }

            // Second pass: fill empty slots
            foreach (var slot in Slots.Where(s => s.IsEmpty()))
            {
                int toAdd = Mathf.Min(remaining, definition.StackSize);

                slot.ItemInstance = new ItemInstance(definition, toAdd);
                remaining -= toAdd;
                NotifySlotUpdated(slot);

                if (remaining == 0)
                    return 0;
            }

            return remaining;
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (amount <= 0 || !HasItem(itemID, amount))
                return false;

            int needed = amount;
            for (int i = Slots.Length - 1; i >= 0; i--)
            {
                var slot = Slots[i];
                if (!slot.IsEmpty() && slot.ItemInstance.Definition.ID == itemID)
                {
                    int toRemove = Mathf.Min(needed, slot.ItemInstance.Count);
                    slot.ItemInstance.Count -= toRemove;
                    needed -= toRemove;

                    if (slot.ItemInstance.Count <= 0)
                    {
                        slot.Clear();
                    }
                    NotifySlotUpdated(slot);

                    if (needed == 0)
                        return true;
                }
            }
            return false;
        }

        public bool HasItem(string itemID, int amount = 1) => GetItemCount(itemID) >= amount;

        public int GetItemCount(string itemID)
        {
            return Slots.Where(s => !s.IsEmpty() && s.ItemInstance.Definition.ID == itemID)
                        .Sum(s => s.ItemInstance.Count);
        }

        public ItemInstance GetFirstItemWithCapability<T>() where T : ItemCapability
        {
            var slot = Slots.FirstOrDefault(s => !s.IsEmpty() && s.ItemInstance.Definition.HasCapabilityOfType<T>());
            return slot?.ItemInstance;
        }

        public Dictionary<string, int> GetAllItemsWithCapability<T>() where T : ItemCapability
        {
            var itemsWithCap = _database.GetAllItemsWithCapability<T>().Select(def => def.ID).ToHashSet();

            return Slots.Where(s => !s.IsEmpty() && itemsWithCap.Contains(s.ItemInstance.Definition.ID))
                        .GroupBy(s => s.ItemInstance.Definition.ID)
                        .ToDictionary(g => g.Key, g => g.Sum(s => s.ItemInstance.Count));
        }
    }
}
