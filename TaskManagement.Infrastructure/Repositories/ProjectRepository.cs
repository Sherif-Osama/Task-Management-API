using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            await _context.Projects.AddAsync(project);

            await _context.SaveChangesAsync();

            return project;
        }

        public async Task<(IEnumerable<Project> Items, int TotalCount)> GetAllAsync(int page, int limit)
        {
            var query = _context.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return (items, totalCount);
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Projects
                .AnyAsync(p => p.Name == name);
        }

        public async Task<bool> ExistsByIdAsync(int projectId)
        {
            return await _context.Projects
                .AnyAsync(p => p.Id == projectId);
        }
    }
}
