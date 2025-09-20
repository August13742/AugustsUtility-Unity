using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AugustsUtility.ItemSystem
{


    [CreateAssetMenu(fileName = "NewDefinition", menuName = "Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("Unique identifier for this item.")]
        [SerializeField] private string _id;
        public string ID => _id;

        [Tooltip("Name displayed to the player.")]
        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;

        [Tooltip("Icon displayed in the UI.")]
        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;

        [Tooltip("Maximum number of this item that can be in a single stack.")]
        [SerializeField] private int _stackSize = 99;
        public int StackSize => _stackSize;

        [Tooltip("Description shown to the player.")]
        [SerializeField, TextArea(3, 5)] private string _description;
        public string Description => _description;

        [Tooltip("A list of capabilities that define this item's behavior.")]
        [SerializeReference] private List<ItemCapability> _capabilities = new List<ItemCapability>();

        /// <summary>
        /// Retrieves the first capability of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of capability to find.</typeparam>
        /// <returns>The capability if found, otherwise null.</returns>
        public T GetCapability<T>() where T : ItemCapability
        {
            return _capabilities.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all capabilities of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of capabilities to find.</typeparam>
        /// <returns>An enumerable collection of matching capabilities.</returns>
        public IEnumerable<T> GetCapabilities<T>() where T : ItemCapability
        {
            return _capabilities.OfType<T>();
        }
        // -----------------------

        /// <summary>
        /// Checks if the item has a capability of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of capability to check for.</typeparam>
        /// <returns>True if the capability exists, otherwise false.</returns>
        public bool HasCapability<T>() where T : ItemCapability
        {
            return _capabilities.OfType<T>().Any();
        }
    }
}
