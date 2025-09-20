namespace AugustsUtility.ItemSystem
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LootTable))]
    public class LootTableEditor : Editor
    {
        private int _trials = 5000;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Analysis", EditorStyles.boldLabel);
            _trials = EditorGUILayout.IntField("Trials", Mathf.Max(100, _trials));

            if (GUILayout.Button("Analyse"))
            {
                var table = (LootTable)target;
                var sum = table.Entries
                    .Where(e => e.Item != null)
                    .ToDictionary(e => e.Item, _ => 0L);

                for (int t = 0; t < _trials; t++)
                {
                    var rolled = table.RollOnce();
                    foreach (var kv in rolled)
                        sum[kv.Key] = sum.GetValueOrDefault(kv.Key) + kv.Value;
                }

                foreach (var kv in sum.OrderBy(k => k.Key.DisplayName))
                {
                    var mean = (double)kv.Value / _trials;
                    Debug.Log($"[LootTable] {kv.Key.DisplayName} avg = {mean:0.###}");
                }
            }
        }
    }
#endif
}
