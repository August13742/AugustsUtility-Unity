using System.Collections.Generic;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [CreateAssetMenu(fileName = "Loot Table", menuName = "Items/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [Header("Weighted Rolls after guaranteed")]
        [Min(0)] public int WeightedRolls = 0;

        public List<LootDrop> Entries = new();

        public Dictionary<ItemDefinition, int> RollOnce(System.Random sysRand = null)
        {
            var result = new Dictionary<ItemDefinition, int>();
            // Guaranteed
            foreach (var e in Entries)
            {
                if (e.Item == null)
                    continue;
                if (e.DropType == LootDrop.Type.Guaranteed)
                    Add(result, e.Item, e.RollAmount());
            }

            // Weighted
            var weighted = Entries.FindAll(e => e.DropType == LootDrop.Type.Weighted && e.Item != null && e.Weight > 0f);
            if (weighted.Count > 0 && WeightedRolls > 0)
            {
                float total = 0f;
                foreach (var e in weighted)
                    total += e.Weight;
                for (int i = 0; i < WeightedRolls; i++)
                {
                    float r = (sysRand != null ? (float)sysRand.NextDouble() : UnityEngine.Random.value) * total;
                    float acc = 0f;
                    foreach (var e in weighted)
                    {
                        acc += e.Weight;
                        if (r <= acc)
                        {
                            Add(result, e.Item, e.RollAmount());
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private static void Add(Dictionary<ItemDefinition, int> dict, ItemDefinition k, int amt)
        {
            if (!dict.TryGetValue(k, out var v))
                v = 0;
            dict[k] = v + Mathf.Max(amt, 0);
        }
    }
}
