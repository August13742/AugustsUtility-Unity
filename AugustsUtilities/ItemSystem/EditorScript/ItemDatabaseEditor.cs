#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : Editor
    {
        private string _searchPath = "Assets/Items"; // Default search path for the Inspector UI

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Database Automation", EditorStyles.boldLabel);

            _searchPath = EditorGUILayout.TextField("Asset Search Path", _searchPath);

            if (GUILayout.Button("Populate Database from Path"))
            {
                PopulateDatabase((ItemDatabase)target, _searchPath);
            }
        }

        /// <summary>
        /// The core database population logic, now public and static so it can be called from other editor scripts.
        /// </summary>
        /// <param name="database">The database asset to populate.</param>
        /// <param name="searchPath">The folder path to search for ItemDefinitions.</param>
        public static void PopulateDatabase(ItemDatabase database, string searchPath)
        {
            if (database == null)
                return;

            Undo.RecordObject(database, "Populate Item Database");
            database.Editor_ClearItems();

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ItemDefinition)}", new[] { searchPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);
                if (item != null)
                {
                    database.Editor_AddItem(item);
                }
            }

            database.BuildIndex();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ItemDatabaseEditor] Population complete. Found {guids.Length} items in '{searchPath}'.");
        }
    }
}
#endif
