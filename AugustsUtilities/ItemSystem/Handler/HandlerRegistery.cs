using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    public static class HandlerRegistry
    {
        // maps from a Type to an INSTANCE.
        private static readonly Dictionary<Type, ICapabilityHandler> _handlers = new();
        private static bool _built;

        public static void Build()
        {
            if (_built)
                return;
            _handlers.Clear();

            var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(ICapabilityHandler).IsAssignableFrom(t));

            foreach (var hType in handlerTypes)
            {
                // Create a single, persistent instance of the handler.
                var handlerInstance = (ICapabilityHandler)Activator.CreateInstance(hType);

                // Find the capability type it handles.
                var baseType = hType.BaseType;
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(CapabilityHandler<>))
                {
                    var capType = baseType.GetGenericArguments()[0];
                    if (!_handlers.ContainsKey(capType))
                    {
                        _handlers[capType] = handlerInstance;
                    }
                }
            }
            _built = true;
            Debug.Log($"[HandlerRegistry] Built with {_handlers.Count} handler instances.");
        }
        public static bool HasHandlerFor(Type capabilityType)
        {
            if (!_built)
            {
                Debug.LogWarning("[HandlerRegistry] Attempted to check for a handler before the registry was built.");
                return false;
            }
            return _handlers.ContainsKey(capabilityType);
        }

        public static void Execute<T>(ItemInstance instance, T cap, object context = null) where T : ActionableCapability
        {
            if (instance == null || cap == null)
                return;

            if (_handlers.TryGetValue(cap.GetType(), out var handler))
            {
                // Cast to the specific handler type and execute.
                (handler as CapabilityHandler<T>)?.Execute(instance, cap, context);
            }
            else
            {
                Debug.LogError($"No handler instance found for capability type {cap.GetType().Name}");
            }
        }
    }
}
