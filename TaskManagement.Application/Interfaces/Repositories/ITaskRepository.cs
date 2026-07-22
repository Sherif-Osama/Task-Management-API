using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> CreateAsync(TaskItem task);

        Task<TaskItem?> GetByIdAsync(int id);

        Task UpdateAsync(TaskItem task);

        Task DeleteAsync(TaskItem task);

        Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetByProjectIdAsync(int projectId, TaskQueryParameters query);

        Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllAsync(TaskQueryParameters query);
    }
}