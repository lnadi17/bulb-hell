using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/LevelData", order = 1)]
    public class LevelData : SerializedScriptableObject {
        [ListDrawerSettings(CustomAddFunction = "AddLevelWithNextIndex")]
        public Level[] levels;

        private Level AddLevelWithNextIndex() {
            return new Level {index = levels.Length};
        }

        [Serializable]
        public class Level {
            [FoldoutGroup("$Name", false)] public int index = 0;

            [FoldoutGroup("$Name", false)] [Range(2, 10), OnValueChanged("Initialize")]
            public int size = 5;

            [FoldoutGroup("$Name", false)] [ShowInInspector] [TableList(AlwaysExpanded = true, ShowIndexLabels = true)]
            public List<List<bool>> matrix;

            public string Name => $"Level {index + 1}";

            public Level() {
                Initialize();
            }

            public void Initialize() {
                matrix = new List<List<bool>>(size);
                for (var i = 0; i < size; i++) {
                    var row = new List<bool>(size);
                    for (var j = 0; j < size; j++) {
                        row.Add(false);
                    }

                    matrix.Add(row);
                }
            }
        }
    }
}