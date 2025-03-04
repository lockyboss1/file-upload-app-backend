using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public Repository(MongoDbContext dbContext, string collectionName)
        {
            _collection = dbContext.GetType().GetProperty(collectionName)?.GetValue(dbContext) as IMongoCollection<T>
                          ?? throw new ArgumentException($"Collection '{collectionName}' not found in MongoDbContext.");
        }

        public async Task<T> GetByIdAsync(string id) =>
            await _collection.Find(e => e.Id == id).FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _collection.Find(predicate).ToListAsync();

        public async Task AddAsync(T entity) =>
            await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(T entity) =>
            await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(e => e.Id == id);
    }
}

