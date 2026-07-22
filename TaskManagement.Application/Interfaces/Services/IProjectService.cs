using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.DTOs.Responses.Projects;

namespace TaskManagement.Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateAsync(CreateProjectRequest request);
        Task<IEnumerable<ProjectResponse>> GetAllAsync();
        Task<ProjectResponse> GetByIdAsync(int id);
        Task<ProjectResponse> UpdateAsync(int id, UpdateProjectRequest request);
        Task DeleteAsync(int id);
    }
}