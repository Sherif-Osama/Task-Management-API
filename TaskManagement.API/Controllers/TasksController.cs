using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Application.DTOs.Requests.Tasks;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Tasks;
using TaskManagement.Application.Interfaces.Services;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<TaskListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<TaskListResponse>>> GetAllAsync(TaskQueryParameters parameters)
        {
            var result = await _taskService.GetAllAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> GetByIdAsync(int id)
        {
            var task = await _taskService.GetByIdAsync(id);

            return Ok(task);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> UpdateAsync(int id, UpdateTaskRequest request)
        {
            var updated = await _taskService.UpdateAsync(id, request);

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _taskService.DeleteAsync(id);

            return NoContent();
        }
    }
}