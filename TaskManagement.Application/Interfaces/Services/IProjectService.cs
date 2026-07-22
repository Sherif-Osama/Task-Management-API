using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Projects;

namespace TaskManagement.Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateAsync(CreateProjectRequest request);
        Task<PagedResponse<ProjectResponse>> GetAllAsync(int page, int limit);
        Task<ProjectResponse> GetByIdAsync(int id);
        Task<ProjectResponse> UpdateAsync(int id, UpdateProjectRequest request);
        Task DeleteAsync(int id);
    }
}