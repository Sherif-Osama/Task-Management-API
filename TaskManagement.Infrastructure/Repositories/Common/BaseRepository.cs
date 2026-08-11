using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories.Common
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        protected async Task<T> AddEntityAsync(T entity)
        {
            await _context.AddAsync(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        protected async Task UpdateEntityAsync(T entity)
        {
            _context.Update(entity);

            await _context.SaveChangesAsync();
        }

        protected async Task DeleteEntityAsync(T entity)
        {
            _context.Remove(entity);

            await _context.SaveChangesAsync();
        }

        protected async Task<T?> GetEntityByIdAsync(Expression<Func<T, bool>> expressionId, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();


            foreach (var item in includes)
            {
                query = query.Include(item);
            }

            return await query.FirstOrDefaultAsync(expressionId);
        }
        protected static IQueryable<T> Paginate(IQueryable<T> query, int page, int limit)
        {
            return query.Skip((page - 1) * limit).Take(limit);
        }
    }
}
