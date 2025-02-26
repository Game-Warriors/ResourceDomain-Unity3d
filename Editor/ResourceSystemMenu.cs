using GameWarriors.ResourceDomain.Data;
using System.IO;
using System.Resources;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameWarriors.ResourceDomain.Editor
{
    public class ResourceSystemMenu : EditorWindow
    {
        private const int ELEMENT_WIDTH = 500;
        private const int ELEMENT_HEIGHT = 100;
        [SerializeField]
        private string _mainServerAddress;
        [SerializeField]
        private string _testServerAddress;

        private string[] _tapContents;
        private int _tapIndex = 0;
        private Vector2 scrollPosition;
        private int _drawCount;
        private IResourceTabElement[] _resourceElements;

        private IResourceTabElement CurrentElement => _resourceElements?[_tapIndex];

        [MenuItem("Tools/Resource Configuration")]
        private static void OpenBuildConfigWindow()
        {
            if (!Directory.Exists("Assets/AssetData/Resources/"))
                Directory.CreateDirectory("Assets/AssetData/Resources/");

            ResourceSystemMenu tmp = CreateInstance<ResourceSystemMenu>();
            tmp.Initialization(ResourceData.ASSET_PATH);
            tmp.Show();
        }

        public void Initialization(string assetPath)
        {
            _tapContents = new string[] { "Unity Object", "String", "Float", "Integer", "Sprite" };
            _resourceElements = new IResourceTabElement[_tapContents.Length];

            ResourceData resourceAsset = AssetDatabase.LoadAssetAtPath<ResourceData>(assetPath);
            if (resourceAsset != null)
            {
                _mainServerAddress = resourceAsset.MainServerAddess;
                _testServerAddress = resourceAsset.TestServerAddess;
            }
            else
            {
                resourceAsset = CreateInstance<ResourceData>();
                Debug.Log("Creating new instnace of resource data");
            }
            _resourceElements[0] = new ObjectElement(resourceAsset.AssetObjects);
            _resourceElements[1] = new StringElement(resourceAsset.StringVars);
            _resourceElements[2] = new FloatElement(resourceAsset.FloatVars);
            _resourceElements[3] = new IntegerElement(resourceAsset.IntVars);
            _resourceElements[4] = new SpriteElement(resourceAsset.PersistSprites);
        }

        void OnGUI()
        {
            DrawElementView();
        }

        private void DrawElementView()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(30);
            _tapIndex = GUILayout.Toolbar(_tapIndex, _tapContents);
            if (_tapContents != null && CurrentElement != null)
            {
                CurrentElement.ResetDraw();
                DrawSearchField(CurrentElement);
                _drawCount = CurrentElement?.Count ?? 0;
                int horizontalCount = (int)(position.width / (ELEMENT_WIDTH + 15));
                horizontalCount = Mathf.Max(1, horizontalCount);
                int verticalCount = CurrentElement.Count / horizontalCount;

                ++verticalCount;
                for (int i = 0; i < verticalCount; ++i)
                {
                    DrawHorizontalElement((i * horizontalCount) + 1, _tapContents[_tapIndex], horizontalCount);
                }
                // GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                Close();
            }
        }

        private void DrawHorizontalElement(int startVerticalCount, string elementName, int elementCount)
        {
            int count = Mathf.Min(_drawCount, elementCount);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < count; ++i)
            {
                GUILayout.BeginVertical($"{startVerticalCount + i}-{elementName}", GUI.skin.box, GUILayout.Width(ELEMENT_WIDTH));
                GUILayout.Space(30);
                CurrentElement.DrawElement(ELEMENT_WIDTH, ELEMENT_HEIGHT);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                --_drawCount;
            }
            if (count < elementCount)
                DrawAddButton();
            GUILayout.EndHorizontal();
            DrawSaveButton();
        }

        private void DrawSearchField(IResourceTabElement element)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search Name : ", GUILayout.Width(90));
            string newPattern = GUILayout.TextField(element.SearchPattern);
            if (newPattern != element.SearchPattern)
            {
                element.ApplySearchPatten(newPattern);
            }
            if (!string.IsNullOrEmpty(element.SearchPattern))
            {
                int size = 20;
                if (GUILayout.Button("X", GUILayout.Width(size), GUILayout.Height(size)))
                    element.ClearSearchPatten();
            }
            GUILayout.EndHorizontal();
        }

        private ResourceData Save()
        {
            ResourceData resourceAsset = AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.ASSET_PATH);
            if (resourceAsset != null)
            {
                resourceAsset.SetServerAddress(_mainServerAddress, _testServerAddress);
                EditorUtility.SetDirty(resourceAsset);
            }
            else
            {
                resourceAsset = new ResourceData();
                resourceAsset.SetServerAddress(_mainServerAddress, _testServerAddress);
                AssetDatabase.CreateAsset(resourceAsset, ResourceData.ASSET_PATH);
            }
            return resourceAsset;
        }

        private void DrawAddButton()
        {
            if (CurrentElement.IsInSearch)
                return;

            if (GUILayout.Button("+", GUILayout.Width(40), GUILayout.Height(40)))
            {
                CurrentElement.AddNewElement();
            }
        }

        private void DrawSaveButton()
        {
            if (GUI.Button(new Rect(position.width - 105, 5, 100, 20), "Save"))
            {
                int length = _resourceElements.Length;
                for (int i = 0; i < length; i++)
                {
                    ResourceData resourceAsset = Save();
                    _resourceElements[i].SaveElement(resourceAsset);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
