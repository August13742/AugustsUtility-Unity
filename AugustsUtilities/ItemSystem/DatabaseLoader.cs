using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AugustsUtility.ItemSystem
{
    public static class DatabaseLoader
    {
        private const string DATABASE_PATH = "ItemDatabase";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeDatabase()
        {
            var database = Resources.Load<ItemDatabase>(DATABASE_PATH);

            if (database == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[DatabaseLoader] No '{DATABASE_PATH}.asset' found in a Resources folder. Creating a new one.");

                // Create a new database instance
                database = ScriptableObject.CreateInstance<ItemDatabase>();

                // Ensure the Resources folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                // Create the asset and save the project
                AssetDatabase.CreateAsset(database, $"Assets/Resources/{DATABASE_PATH}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#else
                Debug.LogError($"[DatabaseLoader] CRITICAL: Failed to find the '{DATABASE_PATH}' ScriptableObject in any Resources folder. The Item System cannot initialize.");
                return; // In a build, we cannot continue.
#endif
            }

            database.Initialize();

            // Optional: assign the database to a global static variable here for easy access
            // e.g., GameManager.ItemDB = database;
        }
    }
}
