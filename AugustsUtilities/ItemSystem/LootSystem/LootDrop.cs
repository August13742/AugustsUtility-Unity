using System;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public sealed class LootDrop
    {
        public enum Type
        {
            Guaranteed, Weighted
        }

        public Type DropType = Type.Guaranteed;
        public ItemDefinition Item;
        [Range(1, 999)] public int MinAmount = 1;
        [Range(1, 999)] public int MaxAmount = 1;

        [Tooltip("Only used if Weighted. Higher = more likely.")]
        public float Weight = 1f;

        public int RollAmount(UnityEngine.Random.State? state = null)
            => UnityEngine.Random.Range(MinAmount, MaxAmount + 1);
    }
}
