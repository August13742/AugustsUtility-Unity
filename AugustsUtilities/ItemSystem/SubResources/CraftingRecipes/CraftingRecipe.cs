using System;
using System.Collections.Generic;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public class CraftingRecipe
    {
        [Serializable]
        public struct MaterialCost
        {
            public ItemDefinition Item;
            [Min(1)]
            public int Amount;
        }

        [Tooltip("A unique ID for the crafting station required (e.g., 'workbench', 'forge'). Leave blank for hand-crafting.")]
        public string StationID;

        [Tooltip("The list of items and their required amounts to craft this item.")]
        public List<MaterialCost> Ingredients;

        [Tooltip("The item(s) produced by this recipe. Defaults to the item this capability is on, but can be overridden.")]
        public List<MaterialCost> Outputs;
    }
}
