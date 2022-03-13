using GameWarriors.ResourceDomain.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class ResourceSystemMenu : ScriptableWizard
    {
        [SerializeField]
        private string _mainServerAddress;
        [SerializeField]
        private string _testServerAddress;
        [SerializeField]
        private UnityEngine.Object[] _assetObjects;
        [SerializeField]
        private StringVariable[] _stringVariables;
        [SerializeField]
        private FloatVariable[] _floatVariables;
        [SerializeField]
        private IntVariable[] _intVariables;
        [SerializeField]
        private Sprite[] _persistSprites;

        [MenuItem("Tools/Resource Configuration")]
        private static void OpenBuildConfigWindow()
        {
            if (!Directory.Exists("Assets/AssetData/Resources/"))
                Directory.CreateDirectory("Assets/AssetData/Resources/");

            ResourceSystemMenu tmp = DisplayWizard<ResourceSystemMenu>("Resource Configuration", "Save");
            tmp.Initialization();
        }

        private void Initialization()
        {
            ResourceData resourceAsset = AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.ASSET_PATH);
            if (resourceAsset != null)
            {
                _mainServerAddress = resourceAsset.MainServerAddess;
                _testServerAddress = resourceAsset.TestServerAddess;
                _assetObjects = resourceAsset.AssetObjects;
                _stringVariables = resourceAsset.StringVars;
                _floatVariables = resourceAsset.FloatVars;
                _intVariables = resourceAsset.IntVars;
                _persistSprites = resourceAsset.PersistSprites;
            }
        }

        private void OnWizardCreate()
        {
            ResourceData resourceAsset = AssetDatabase.LoadAssetAtPath<ResourceData>(ResourceData.ASSET_PATH);

            if (resourceAsset != null)
            {
                resourceAsset.SetResourceAssets(_assetObjects, _stringVariables, _floatVariables, _intVariables, _persistSprites);
                resourceAsset.SetServerAddress(_mainServerAddress, _testServerAddress);
                EditorUtility.SetDirty(resourceAsset);
            }
            else
            {
                resourceAsset = new ResourceData(_assetObjects, _stringVariables, _floatVariables, _intVariables, _persistSprites);
                resourceAsset.SetServerAddress(_mainServerAddress, _testServerAddress);
                AssetDatabase.CreateAsset(resourceAsset, ResourceData.ASSET_PATH);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
