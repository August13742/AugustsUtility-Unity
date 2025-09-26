using System;
using System.Collections.Generic;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public class CraftingCapability : ItemCapability
    {
        [Tooltip("A list of all possible ways to craft this item.")]
        public List<CraftingRecipe> Recipes = new();
    }
}
