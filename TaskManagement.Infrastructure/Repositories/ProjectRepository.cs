using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories.Common;

namespace TaskManagement.Infrastructure.Repositories
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {

        public ProjectRepository(AppDbContext context) : base(context) { }

        public Task<Project> CreateAsync(Project project) => AddEntityAsync(project);

        public Task UpdateAsync(Project project) => UpdateEntityAsync(project);

        public Task DeleteAsync(Project project) => DeleteEntityAsync(project);

        public Task<Project?> GetByIdAsync(int id) => GetEntityByIdAsync(p => p.Id == id);

        public async Task<(IEnumerable<Project> Items, int TotalCount)> GetAllAsync(int page, int limit)
        {
            var query = _context.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await Paginate(query, page, limit).ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Projects
                .AnyAsync(p => p.Name == name);
        }
    }
}
