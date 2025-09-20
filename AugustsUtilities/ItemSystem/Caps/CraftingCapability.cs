using System;
using System.Collections.Generic;
using UnityEngine;
namespace AugustsUtility.ItemSystem
{
    /// <summary>
    /// A capability that defines a crafting recipe for an item.
    /// </summary>
    [Serializable]
    public class CraftingCapability : ItemCapability
    {
        [Serializable]
        public struct MaterialCost
        {
            public ItemDefinition Item;
            [Min(1)]
            public int Amount;
        }

        [Tooltip("The list of items and their required amounts to craft this item.")]
        public List<MaterialCost> RequiredMaterials;
    }
}
