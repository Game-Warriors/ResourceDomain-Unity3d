using GameWarriors.ResourceDomain.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class IntegerElement : IResourceTabElement
    {
        [SerializeField]
        private List<IntVariable> _intVariables;

        private List<int> _searchIndex;

        public int Count => IsInSearch ? _searchIndex.Count : (_intVariables?.Count ?? 0);
        public bool IsInSearch => !string.IsNullOrEmpty(SearchPattern);
        public string SearchPattern { get; private set; }
        public int CurrentIndex { get; private set; }

        public IntegerElement(IntVariable[] intVariables)
        {
            _intVariables = new List<IntVariable>();
            _intVariables.AddRange(intVariables);
        }

        public void AddNewElement()
        {
            _searchIndex?.Clear();
            _intVariables ??= new List<IntVariable>();
            _intVariables.Add(default);
        }

        public void DrawElement(int width, int height)
        {
            int index = 0;
            if (IsInSearch)
            {
                index = _searchIndex[CurrentIndex];
            }
            else
                index = CurrentIndex;
            IntVariable tmp = _intVariables[index];
            string name = EditorGUILayout.TextField(new GUIContent("Name"), tmp.Name);
            int variable = EditorGUILayout.IntField("Value", tmp.Variable);
            _intVariables[index] = new IntVariable(name, variable);
            GUILayout.Space(5);
            if (GUILayout.Button("Remove"))
            {
                _intVariables.RemoveAt(index);
                ClearSearchPatten();
            }
            else
                ++CurrentIndex;
        }

        public void SaveElement<T>(T input)
        {
            var config = input as ResourceData;
            config.SetIntVariable(_intVariables.Where((item) => item.Name != null)
                .ToArray());
        }
        public void ResetDraw()
        {
            CurrentIndex = 0;
        }

        public void ApplySearchPatten(string newPattern)
        {
            SearchPattern = newPattern;
            int count = Mathf.Max(_intVariables.Count, 5);
            _searchIndex ??= new List<int>(count);
            _searchIndex.Clear();
            int length = _intVariables.Count;
            for (int i = 0; i < length; ++i)
            {
#if UNITY_2021_1_OR_NEWER
                if (_intVariables[i].Name.Contains(newPattern, System.StringComparison.OrdinalIgnoreCase))
#else
                if (_intVariables[i].Name.Contains(newPattern))
#endif
                    _searchIndex.Add(i);
            }
        }

        public void ClearSearchPatten()
        {
            _searchIndex?.Clear();
            SearchPattern = string.Empty;
        }
    }
}
