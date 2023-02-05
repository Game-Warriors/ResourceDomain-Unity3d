using GameWarriors.ResourceDomain.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class StringElement : IResourceTabElement
    {
        [SerializeField]
        private List<StringVariable> _stringVariables;

        private List<int> _searchIndex;
        public int Count => IsInSearch ? _searchIndex.Count :( _stringVariables?.Count ?? 0);
        public bool IsInSearch => !string.IsNullOrEmpty(SearchPattern);
        public string SearchPattern { get; set; }
        public int CurrentIndex { get; private set; }

        public StringElement(StringVariable[] stringVariables)
        {
            _stringVariables = new List<StringVariable>();
            _stringVariables.AddRange(stringVariables);
        }

        public void AddNewElement()
        {
            _searchIndex?.Clear();
            _stringVariables ??= new List<StringVariable>();
            _stringVariables.Add(default);
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
            StringVariable tmp = _stringVariables[index];
            string name = EditorGUILayout.TextField("Name", tmp.Name);
            string variable = EditorGUILayout.TextField("Value", tmp.Variable);
            _stringVariables[index] = new StringVariable(name, variable);
            GUILayout.Space(5);
            if (GUILayout.Button("Remove"))
            {
                _stringVariables.RemoveAt(index);
                ClearSearchPatten();
            }
            else
                ++CurrentIndex;
        }

        public void SaveElement<T>(T input)
        {
            var config = input as ResourceData;
            config.SetStringVariables(_stringVariables.Where((item) => item.Name != null)
                .ToArray());
        }

        public void ResetDraw()
        {
            CurrentIndex = 0;
        }

        public void ApplySearchPatten(string newPattern)
        {
            SearchPattern = newPattern;
            int count = Mathf.Max(_stringVariables.Count, 5);
            _searchIndex ??= new List<int>(count);
            _searchIndex.Clear();
            int length = _stringVariables.Count;
            for (int i = 0; i < length; ++i)
            {
                if (_stringVariables[i].Name.Contains(newPattern, System.StringComparison.OrdinalIgnoreCase))
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
