using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Application.DTOs.Requests.Tasks;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Tasks;

namespace TaskManagement.Application.Interfaces.Services
{
    internal interface ITaskService
    {
        Task<TaskResponse> CreateAsync(int projectId, CreateTaskRequest request);

        Task<TaskResponse> GetByIdAsync(int id);

        Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request);

        Task DeleteAsync(int id);

        Task<PagedResponse<TaskListResponse>> GetByProjectIdAsync(int projectId, TaskQueryParameters parameters);

        Task<PagedResponse<TaskListResponse>> GetAllAsync(TaskQueryParameters parameters);
    }
}