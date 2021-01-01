using Api.Service.Models;
using System.Collections.Generic;

namespace Api.Service.Mongo
{
    public interface IMongoService<T> where T : MongoEntity
    {
        // WRITE
        T Create(T entity);
        bool Update(T entity);
        bool Update(T entity, string userId);
        bool Remove(string id);
        bool Remove(string id, string userId);

        // READ
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(string userId);
        T GetOne(string id);
        T GetOne(string id, string userId);
        IEnumerable<T> GetMany(HashSet<string> ids);
        IEnumerable<T> GetMany(HashSet<string> ids, string userId);
    }
}