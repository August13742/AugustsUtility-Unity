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

            // Draw the default inspector for all fields except the capabilities list
            DrawPropertiesExcluding(serializedObject, "_capabilities");

            // --- Icon Preview Section ---
            ItemDefinition itemDef = (ItemDefinition)target;
            if (itemDef.Icon != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Icon Preview", EditorStyles.boldLabel);

                // Center the preview area horizontally
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                // Reserve a 128x128 rect for the preview area.
                Rect previewAreaRect = GUILayoutUtility.GetRect(64, 64);

                if (itemDef.Icon.texture != null)
                {
                    // Get sprite texture and rect in pixels
                    Texture2D tex = itemDef.Icon.texture;
                    Rect texRect = itemDef.Icon.textureRect;

                    // Calculate the final drawing rect inside the preview area, preserving aspect ratio
                    Rect finalRect = CalculateAspectRatioRect(texRect, previewAreaRect);

                    // Convert pixel rect to UV coordinates for drawing from an atlas
                    Rect uvCoords = new Rect(
                        texRect.x / tex.width,
                        texRect.y / tex.height,
                        texRect.width / tex.width,
                        texRect.height / tex.height
                    );

                    // Draw the sprite texture with the correct coordinates and aspect ratio
                    GUI.DrawTextureWithTexCoords(finalRect, tex, uvCoords);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Capabilities", EditorStyles.boldLabel);

            // Display the list of currently attached capabilities
            for (int i = 0; i < _capabilitiesProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Draw the capability itself. The 'true' argument ensures its children are drawn.
                EditorGUILayout.PropertyField(_capabilitiesProp.GetArrayElementAtIndex(i), true);

                // Add a remove button
                if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    // Important: First, null the reference. Then delete the array element.
                    _capabilitiesProp.GetArrayElementAtIndex(i).managedReferenceValue = null;
                    _capabilitiesProp.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            // The "Add Capability" button and dropdown menu
            if (GUILayout.Button("Add Capability"))
            {
                ShowAddCapabilityMenu();
            }

            serializedObject.ApplyModifiedProperties();
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
