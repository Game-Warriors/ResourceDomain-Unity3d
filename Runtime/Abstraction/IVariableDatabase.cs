using System;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstration to access variable data which exist in resource system.
    /// </summary>
    public interface IVariableDatabase
    {
        /// <summary>
        /// This method could retrive all Object asset which added to Unity Object section in system editor before
        /// </summary>
        /// <typeparam name="T">the type of request object</typeparam>
        /// <param name="key">the key is name of object which present in editor</param>
        /// <returns></returns>
        T GetDataObject<T>(string key) where T : UnityEngine.Object;
        /// <summary>
        /// This method could retrieve all values which is added in String, Float, Integer tab in system editor.
        /// </summary>
        /// <typeparam name="T">type of the variable should be just String, Float, Integer </typeparam>
        /// <param name="variableKey">the key value is specific name of the value which is input in editor for each item</param>
        /// <returns></returns>
        T GetVariable<T>(string variableKey) where T : IConvertible;
    }
}
