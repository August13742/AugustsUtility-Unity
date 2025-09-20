using System;

namespace AugustsUtility.ItemSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class HandlesCapabilityAttribute : Attribute
    {
        public Type CapabilityType
        {
            get;
        }
        public HandlesCapabilityAttribute(Type capabilityType) => CapabilityType = capabilityType;
    }
}
