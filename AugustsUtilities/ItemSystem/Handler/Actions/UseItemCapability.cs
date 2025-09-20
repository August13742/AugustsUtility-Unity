using System;

namespace AugustsUtility.ItemSystem
{
    [Serializable]
    public sealed class UseItemCapability : ActionableCapability
    {
        public string UseVerb = "Use";
        public float Cooldown = 0f;
    }
}
