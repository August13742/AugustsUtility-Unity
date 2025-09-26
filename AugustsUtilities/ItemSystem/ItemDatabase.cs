using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{

    //[CreateAssetMenu(fileName = "Item Database", menuName = "Items/Item Database")] //should be automatic
    public class ItemDatabase : ScriptableObject
    {
        // The serialized source of truth. Populated by the editor script.
        [SerializeField]
        private List<ItemDefinition> _items = new();
        public IReadOnlyList<ItemDefinition> AllItems => _items;

        // Non-serialized runtime lookup.
        private Dictionary<string, ItemDefinition> _itemsByID;
        private bool _isInitialized = false;

        // OnEnable is called when the scriptable object is loaded.
        // This ensures the dictionary is ready for use within the editor.
        private void OnEnable()
        {
            BuildIndex();
        }

        // A single, explicit initialisation point for runtime.
        public void Initialize()
        {
            if (_isInitialized)
                return;

            // The index is already built by OnEnable, but we ensure it.
            BuildIndex();

            HandlerRegistry.Build();
            ValidateHandlers();

            _isInitialized = true;
            Debug.Log($"[ItemDatabase] Initialized with {_itemsByID.Count} items.");
        }

        public void BuildIndex()
        {
            _itemsByID = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
            if (_items == null)
                return;

            foreach (var item in _items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.ID))
                {
                    Debug.LogError($"[ItemDatabase] Item '{item.name}' is missing an ID.", item);
                    continue;
                }

                if (_itemsByID.ContainsKey(item.ID))
                {
                    Debug.LogError($"[ItemDatabase] Duplicate item ID '{item.ID}'.", item);
                    continue;
                }
                _itemsByID[item.ID] = item;
            }
        }

        public ItemDefinition GetByID(string id)
        {
            if (id == null || _itemsByID == null)
                return null;
            _itemsByID.TryGetValue(id, out var item);
            return item;
        }

        /// <summary>
        /// Finds all item definitions in the database that have a specific capability.
        /// </summary>
        /// <typeparam name="T">The type of capability to search for.</typeparam>
        /// <returns>An enumerable collection of matching item definitions.</returns>
        public IEnumerable<ItemDefinition> GetAllItemsWithCapability<T>() where T : ItemCapability
        {
            if (_items == null)
                return Enumerable.Empty<ItemDefinition>();
            return _items.Where(def => def != null && def.HasCapabilityOfType<T>());
        }

        /// <summary>
        /// A specialized helper for finding all items that can be "used" via a handler.
        /// </summary>
        public IEnumerable<ItemDefinition> GetAllActionableItems()
        {
            return GetAllItemsWithCapability<ActionableCapability>();
        }

        // add more complex queries as needed. For example, finding all
        // potions that restore mana:
        // public IEnumerable<ItemDefinition> GetAllManaPotions()
        // {
        //     return _items.Where(def => def != null && def.GetCapabilities<ConsumableCapability>()
        //         .Any(cap => cap.TypeToRestore == ResourceType.Mana));
        // }

        private void ValidateHandlers()
        {
            int errorCount = 0;
            foreach (var def in _items)
            {
                if (def == null)
                    continue;

                foreach (var cap in def.GetCapabilitiesOfType<ActionableCapability>())
                {
                    var capType = cap.GetType();
                    if (!HandlerRegistry.HasHandlerFor(capType))
                    {
                        Debug.LogError($"[ItemDatabase] Missing handler for capability '{capType.Name}' on item '{def.ID}'.", def);
                        errorCount++;
                    }
                }
            }

            if (errorCount > 0)
                Debug.LogError($"[ItemDatabase] Validation failed with {errorCount} missing handler mapping(s).");
            else
                Debug.Log("[ItemDatabase] Handler validation successful.");
        }

#if UNITY_EDITOR
        // Provide a way for the editor script to manage the internal list.
        public void Editor_ClearItems() => _items.Clear();
        public void Editor_AddItem(ItemDefinition item) => _items.Add(item);
#endif
    }
}
