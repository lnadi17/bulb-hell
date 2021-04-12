using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor.Experimental.Networking.PlayerConnection;
using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/LevelData", order = 1)]
    public class LevelData : SerializedScriptableObject {
        [ListDrawerSettings(CustomAddFunction = "AddLevelWithNextIndex")]
        public Level[] levels;

        private Level AddLevelWithNextIndex() {
            var createdLevel = new Level {index = levels.Length, size = 5};
            createdLevel.Initialize();
            return createdLevel;
        }
        
        public struct Level {
            [FoldoutGroup("$Name", false)] public int index;

            [FoldoutGroup("$Name", false)] [Range(2, 10), OnValueChanged("Initialize")]
            public int size;

            [FoldoutGroup("$Name", false)]
            [TableMatrix(HorizontalTitle = "Board", DrawElementMethod = "DrawColoredElement", SquareCells = true)]
            public bool[,] matrix;

            public string Name => $"Level {index + 1}";

            private List<List<bool>> _matrixList;

            public List<List<bool>> Matrix {
                get => _matrixList;
                set => _matrixList = value;
            }

            public void Initialize() {
                _matrixList = new List<List<bool>>(size);
                matrix = new bool[size, size];
                for (var i = 0; i < size; i++) {
                    var row = new List<bool>(size);
                    for (var j = 0; j < size; j++) {
                        row.Add(false);
                        matrix[i, j] = false;
                    }

                    _matrixList.Add(row);
                }
            }
            
            private static bool DrawColoredElement(Rect rect, bool value) {
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
                    value = !value;
                    GUI.changed = true;
                    Event.current.Use();
                }

                UnityEditor.EditorGUI.DrawRect(rect.Padding(1),
                    value ? new Color(1f, 1f, 0.28f) : new Color(0.02f, 0.01f, 0.01f, 0.82f));

                return value;
            }
        }
    }
}