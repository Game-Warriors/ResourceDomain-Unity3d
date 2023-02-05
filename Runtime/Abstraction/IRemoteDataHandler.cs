using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameWarriors.ResourceDomain.Abstraction
{
    public interface IRemoteDataHandler
    {
        void RegisterData(IReadOnlyDictionary<string, IConvertible> dataTable);
        bool TryGetValue(string variableKey, TypeCode typeCode, out IConvertible remoteConvertible);
        Task SetupRemoteDataAsync();
    }
}