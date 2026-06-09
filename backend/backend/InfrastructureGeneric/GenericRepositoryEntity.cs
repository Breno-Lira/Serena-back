using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace InfrastructureGeneric
{
    public class GenericRepositoryEntity<T, Tcontext> : IGenericRepository<T>
        where T : class
        where Tcontext : DbContext
    {
        private readonly Tcontext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepositoryEntity(Tcontext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        
        public async Task<IEnumerable<T>> FindAllByForeignKeyAsync(
            string foreignKeyName,
            object foreignKeyValue)
        {
            var entityType = typeof(T);

            var propertyInfo = entityType.GetProperty(
                foreignKeyName,
                BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
                throw new ArgumentException(
                    $"A propriedade '{foreignKeyName}' não existe em '{entityType.Name}'.");

            var parameter = Expression.Parameter(entityType, "e");
            var propertyAccess = Expression.Property(parameter, propertyInfo);

            // TRATAMENTO DE Nullable<T>
            var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                               ?? propertyInfo.PropertyType;

            var convertedValue = Convert.ChangeType(foreignKeyValue, propertyType);

            var constant = Expression.Constant(
                convertedValue,
                propertyInfo.PropertyType);

            var equality = Expression.Equal(propertyAccess, constant);

            var predicate = Expression.Lambda<Func<T, bool>>(equality, parameter);

            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            return Task.CompletedTask;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entidade com id {id} não encontrada.");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetByConditionAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<T?> GetByIdWithIncludesAsync(
            int id,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(
                e => EF.Property<int>(e, "Id") == id);
        }
    }
}

