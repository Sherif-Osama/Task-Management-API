using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<Project> CreateAsync(Project project);
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Project project);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByIdAsync(int projectId);
    }
}