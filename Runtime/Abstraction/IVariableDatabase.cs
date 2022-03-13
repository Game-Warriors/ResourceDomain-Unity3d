using System;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    public interface IVariableDatabase
    {
        T GetDataObject<T>(string key) where T : ScriptableObject;
        T GetVariable<T>(string variableKey) where T : IConvertible;
        T RequestConfigData<T>(string assetName) where T : ScriptableObject;
        AsyncOperation RequestConfigDataAsync(string assetName);
        void UnloadConfigData(string assetName);
    }
}
