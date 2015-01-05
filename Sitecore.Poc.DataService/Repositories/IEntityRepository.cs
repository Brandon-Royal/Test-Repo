using System.Collections.Generic;
using Sitecore.Poc.DataService.Models;

namespace Sitecore.Poc.DataService.Repositories
{
    public interface IEntityRepository<T>
    {
        bool Exists(string id);

        IEnumerable<T> GetAll();

        T GetSingle(string id);

        void AddSingle(string id, T entity);

        void UpdateSingle(string id, T entity);

        void DeleteSingle(string id);
    }
}