using AugustsUtility.ItemSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugustsUtility.InventorySystem
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance
        {
            get; private set;
        }

        private readonly Dictionary<int, InventoryComponent> _inventories = new();
        public InventoryComponent PlayerInventory
        {
            get; private set;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(InventoryComponent inventory)
        {
            int id = inventory.gameObject.GetInstanceID();
            if (_inventories.ContainsKey(id))
                return;

            _inventories[id] = inventory;
            if (inventory.CompareTag("Player"))
            {
                PlayerInventory = inventory;
            }
        }

        public void Unregister(InventoryComponent inventory)
        {
            int id = inventory.gameObject.GetInstanceID();
            if (!_inventories.ContainsKey(id))
                return;

            if (PlayerInventory == inventory)
            {
                PlayerInventory = null;
            }
            _inventories.Remove(id);
        }

        public IEnumerable<InventoryComponent> GetInventoriesByCategory(Category category)
        {
            return _inventories.Values.Where(inv => inv.Category == category);
        }

        public bool SwapItems(InventorySlot slotA, InventorySlot slotB)
        {
            if (slotA == null || slotB == null)
                return false;

            (slotA.ItemInstance, slotB.ItemInstance) = (slotB.ItemInstance, slotA.ItemInstance);

            slotA.Owner.NotifySlotUpdated(slotA);
            slotB.Owner.NotifySlotUpdated(slotB);

            return true;
        }

        public bool TransferItem(InventoryComponent source, InventoryComponent target, string itemID, int amount)
        {
            if (source == null || target == null || !source.HasItem(itemID, amount))
            {
                return false;
            }

            if (source.RemoveItem(itemID, amount))
            {
                int remaining = target.AddItem(itemID, amount);
                if (remaining > 0)
                {
                    // Transaction failed, target inventory was full. Roll back the change.
                    source.AddItem(itemID, remaining);
                    return false;
                }
                return true; // Success
            }
            return false; // Removal failed
        }
    }
}
