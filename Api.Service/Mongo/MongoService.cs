using Api.Service.Exceptions;
using Api.Service.Models;
using Api.Service.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Api.Service.Mongo
{
    public abstract class MongoService<T> : IMongoService<T> where T : MongoEntity
    {
        public abstract string CollectionName { get; }
        protected readonly IMongoCollection<T> _collection;

        protected MongoDBSettings _settings;
        protected MongoClient _client;
        protected IMongoDatabase _db;

        protected MongoService(MongoDBSettings mongoSettings)
        {
            _settings = mongoSettings;
            _client = new MongoClient($"mongodb://{_settings.Username}:{_settings.Password}@{_settings.Endpoint}");
            _db = _client.GetDatabase(_settings.Database);
            _collection = _db.GetCollection<T>(CollectionName);
        }

        #region CRUD

        public T Create(T entity)
        {
            entity.CreationDate = DateTime.UtcNow;
            entity.UpdateDate = DateTime.UtcNow;
            _collection.InsertOne(entity);

            return entity;
        }

        public bool Update(T entity)
        {
            var existing = GetOne(entity.Id);
            if (existing == null)
            {
                throw new NotFoundException($"Cannot find entity {entity.Id} to update");
            }
            entity.UpdateDate = DateTime.UtcNow;
            var result = _collection.ReplaceOne(filter => filter.Id == existing.Id, entity);

            return result.IsAcknowledged;
        }

        public bool Update(T entity, string userId)
        {
            var existing = GetOne(entity.Id, userId);
            if (existing == null)
            {
                throw new NotFoundException($"Cannot find entity {entity.Id} to update (user={userId})");
            }
            entity.UpdateDate = DateTime.UtcNow;
            var result = _collection.ReplaceOne(filter => filter.Id == existing.Id, entity);

            return result.IsAcknowledged;
        }

        public bool Remove(string id)
        {
            var existing = GetOne(id);
            if (existing == null)
            {
                throw new NotFoundException($"Cannot find entity {id} to delete");
            }
            var result = _collection.DeleteOne(filter => filter.Id == id);

            return result.DeletedCount == 1;
        }

        public bool Remove(string id, string userId)
        {
            var existing = GetOne(id, userId);
            if (existing == null)
            {
                throw new NotFoundException($"Cannot find entity {id} to delete (user={userId})");
            }
            var result = _collection.DeleteOne(filter => filter.Id == id);

            return result.DeletedCount == 1;
        }

        public IEnumerable<T> GetAll()
        {
            return _collection
                    .Find(_ => true)
                    .ToEnumerable();
        }

        public IEnumerable<T> GetAll(string userId)
        {
            return _collection
                    .Find(r => r.OwnerId == userId)
                    .ToEnumerable();
        }

        public T GetOne(string id)
        {
            return _collection
                        .Find(r => r.Id == id)
                        .FirstOrDefault();
        }

        public T GetOne(string id, string userId)
        {
            return _collection
                        .Find(r => r.Id == id && r.OwnerId == userId)
                        .FirstOrDefault();
        }

        public IEnumerable<T> GetMany(HashSet<string> ids)
        {
            return _collection
                        .Find(r => ids.Contains(r.Id))
                        .ToEnumerable();
        }

        public IEnumerable<T> GetMany(HashSet<string> ids, string userId)
        {
            return _collection
                        .Find(r => ids.Contains(r.Id) && r.OwnerId == userId)
                        .ToEnumerable();
        }

        #endregion
    }
}