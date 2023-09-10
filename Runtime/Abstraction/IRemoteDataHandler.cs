using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction for objects which is remote data bridge from client to server or any online source
    /// </summary>
    public interface IRemoteDataHandler
    {
        void RegisterData(IReadOnlyDictionary<string, IConvertible> dataTable);
        bool TryGetValue(string variableKey, TypeCode typeCode, out IConvertible remoteConvertible);
        Task SetupRemoteDataAsync();
    }
}