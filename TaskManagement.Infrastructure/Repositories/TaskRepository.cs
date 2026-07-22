using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> CreateAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);

            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();
        }
    }
}