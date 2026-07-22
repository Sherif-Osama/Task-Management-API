using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> CreateAsync(TaskItem task);

        Task<TaskItem?> GetByIdAsync(int id);

        Task UpdateAsync(TaskItem task);

        Task DeleteAsync(TaskItem task);

        // TODO: Add task listing with pagination, filtering, sorting, and search.
    }
}
