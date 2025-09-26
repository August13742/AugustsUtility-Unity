#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the ItemDefinition ScriptableObject.
/// This editor provides a user-friendly interface for adding and managing
/// a polymorphic list of ItemCapability instances and previews the item's icon.
/// </summary>
///

namespace AugustsUtility.ItemSystem
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : Editor
    {
        private SerializedProperty _capabilitiesProp;
        private static List<Type> _capabilityTypes; // Cached list of all concrete capability types

        private void OnEnable()
        {
            // Find the serialized property for the capabilities list
            _capabilitiesProp = serializedObject.FindProperty("_capabilities");

            // Use reflection to find all concrete classes inheriting from ItemCapability
            if (_capabilityTypes == null)
            {
                _capabilityTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(ItemCapability)) && !type.IsAbstract)
                    .ToList();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_capabilities");

            ItemDefinition itemDef = (ItemDefinition)target;
            if (itemDef.Icon != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Icon Preview", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Rect previewAreaRect = GUILayoutUtility.GetRect(64, 64);

                if (itemDef.Icon.texture != null)
                {
                    Texture2D tex = itemDef.Icon.texture;
                    Rect texRect = itemDef.Icon.textureRect;
                    Rect finalRect = CalculateAspectRatioRect(texRect, previewAreaRect);
                    Rect uvCoords = new Rect(texRect.x / tex.width, texRect.y / tex.height, texRect.width / tex.width, texRect.height / tex.height);
                    GUI.DrawTextureWithTexCoords(finalRect, tex, uvCoords);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Capabilities", EditorStyles.boldLabel);

            for (int i = 0; i < _capabilitiesProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty elem = _capabilitiesProp.GetArrayElementAtIndex(i);
                string typeName = GetManagedRefShortType(elem); // e.g., "ConsumableCapability" or "CraftableCapability"
                GUIContent label = new GUIContent($"Element {i} ({typeName})");

                // Use the custom label instead of Unity’s default “Element i”
                EditorGUILayout.PropertyField(elem, label, /* includeChildren: */ true);

                if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    elem.managedReferenceValue = null;
                    _capabilitiesProp.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("Add Capability"))
            {
                ShowAddCapabilityMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }
        // Extracts "TypeName" from managedReferenceFullTypename which is like "AssemblyName Namespace.TypeName"
        private static string GetManagedRefShortType(SerializedProperty prop)
        {
            string full = prop.managedReferenceFullTypename; // null or "" if element is null
            if (string.IsNullOrEmpty(full))
                return "null";

            // Format is "AssemblyName Namespace.TypeName"
            int space = full.IndexOf(' ');
            string typeWithNs = space >= 0 ? full.Substring(space + 1) : full;

            int lastDot = typeWithNs.LastIndexOf('.');
            return lastDot >= 0 ? typeWithNs.Substring(lastDot + 1) : typeWithNs;
        }
        private void ShowAddCapabilityMenu()
        {
            GenericMenu menu = new GenericMenu();

            foreach (var type in _capabilityTypes)
            {
                // Add an item to the menu for each capability type
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    // This is the callback function executed when a menu item is selected
                    AddCapability(type);
                });
            }

            menu.ShowAsContext();
        }

        private void AddCapability(Type type)
        {
            // Add a new element to the array
            _capabilitiesProp.arraySize++;
            SerializedProperty newCapabilityProp = _capabilitiesProp.GetArrayElementAtIndex(_capabilitiesProp.arraySize - 1);

            // Create an instance of the selected capability type and assign it.
            // This is where [SerializeReference] is crucial.
            newCapabilityProp.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Calculates a centered rectangle within a container that maintains the aspect ratio of the content.
        /// </summary>
        /// <param name="contentRect">The pixel dimensions of the content (e.g., a sprite's textureRect).</param>
        /// <param name="containerRect">The container area to fit the content into.</param>
        /// <returns>The calculated rectangle for drawing.</returns>
        private Rect CalculateAspectRatioRect(Rect contentRect, Rect containerRect)
        {
            float contentRatio = contentRect.width / contentRect.height;
            float containerRatio = containerRect.width / containerRect.height;

            Rect resultRect = containerRect;

            if (contentRatio > containerRatio) // Content is wider than container
            {
                resultRect.height = containerRect.width / contentRatio;
                resultRect.y += (containerRect.height - resultRect.height) / 2.0f;
            }
            else // Content is taller than or has the same aspect as the container
            {
                resultRect.width = containerRect.height * contentRatio;
                resultRect.x += (containerRect.width - resultRect.width) / 2.0f;
            }

            return resultRect;
        }
    }
#endif

}
