namespace AugustsUtility.ItemSystem
{

    [HandlesCapability(typeof(UseItemCapability))]
    public sealed class UseItemHandler : CapabilityHandler<UseItemCapability>
    {
        public override void Execute(ItemInstance instance, UseItemCapability cap, object context = null)
        {
            UnityEngine.Debug.Log($"[{instance.Definition?.ID}] {cap.UseVerb} executed.");
            // do the thing…
        }
    }
}
