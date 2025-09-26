using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    public static class HandlerRegistry
    {
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
                var handlerInstance = (ICapabilityHandler)Activator.CreateInstance(hType);

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

            // Check direct registration first
            if (_handlers.ContainsKey(capabilityType))
                return true;

            // Check if any base type has a handler
            var currentType = capabilityType.BaseType;
            while (currentType != null && currentType != typeof(object))
            {
                if (_handlers.ContainsKey(currentType))
                    return true;
                currentType = currentType.BaseType;
            }

            return false;
        }

        public static void Execute<T>(ItemInstance instance, T cap, object context = null) where T : ActionableCapability
        {
            if (instance == null || cap == null)
                return;

            var handler = FindHandlerForCapability(cap.GetType());
            if (handler != null)
            {
                // Cast to the base handler type and execute
                if (handler is CapabilityHandler<T> typedHandler)
                {
                    typedHandler.Execute(instance, cap, context);
                }
                else
                {
                    // Try to find a handler that can handle this capability's base type
                    ExecuteWithPolymorphicHandler(handler, instance, cap, context);
                }
            }
            else
            {
                Debug.LogError($"No handler instance found for capability type {cap.GetType().Name}");
            }
        }

        private static ICapabilityHandler FindHandlerForCapability(Type capabilityType)
        {
            // Direct lookup first
            if (_handlers.TryGetValue(capabilityType, out var handler))
                return handler;

            // Walk up the inheritance chain
            var currentType = capabilityType.BaseType;
            while (currentType != null && currentType != typeof(object))
            {
                if (_handlers.TryGetValue(currentType, out handler))
                    return handler;
                currentType = currentType.BaseType;
            }

            return null;
        }

        private static void ExecuteWithPolymorphicHandler<T>(ICapabilityHandler handler, ItemInstance instance, T cap, object context)
            where T : ActionableCapability
        {
            // Use reflection to call the handler's Execute method
            var executeMethod = handler.GetType().GetMethod("Execute");
            if (executeMethod != null)
            {
                try
                {
                    executeMethod.Invoke(handler, new object[] { instance, cap, context });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error executing handler {handler.GetType().Name} for capability {cap.GetType().Name}: {e.Message}");
                }
            }
        }
    }
}
