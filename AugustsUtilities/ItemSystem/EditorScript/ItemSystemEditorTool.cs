#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AugustsUtility.ItemSystem
{
    public static class ItemSystemEditorTools
    {
        private const string DATABASE_SEARCH_STRING = "t:ItemDatabase";
        private const string DEFAULT_ITEM_PATH = "Assets/Items";

        [MenuItem("Tools/Item System/Repopulate Database")]
        public static void RepopulateDatabaseFromMenu()
        {
            string[] guids = AssetDatabase.FindAssets(DATABASE_SEARCH_STRING);
            if (guids.Length == 0)
            {
                Debug.LogError($"[ItemSystem] Could not find an ItemDatabase asset in the project. Please create one via Create > Items > Item Database.");
                return;
            }
            if (guids.Length > 1)
            {
                Debug.LogWarning("[ItemSystem] Multiple ItemDatabase assets found. Repopulating the first one. Consider having only one.");
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);

            if (database != null)
            {
                // Call the public, static populator method, assuming the default item path.
                ItemDatabaseEditor.PopulateDatabase(database, DEFAULT_ITEM_PATH);
                Debug.Log($"[ItemSystem] Successfully repopulated '{path}' using items from '{DEFAULT_ITEM_PATH}'.");
            }
        }
    }
}
#endif
