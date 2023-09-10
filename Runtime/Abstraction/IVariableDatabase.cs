using System;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstration to access variable data which exist in resource system.
    /// </summary>
    public interface IVariableDatabase
    {
        T GetDataObject<T>(string key) where T : UnityEngine.Object;
        T GetVariable<T>(string variableKey) where T : IConvertible;
    }
}
