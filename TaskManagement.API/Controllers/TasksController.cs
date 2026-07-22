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

        [HttpPost("/api/projects/{projectId:int}/tasks")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> Create(int projectId, CreateTaskRequest request)
        {
            var created = await _taskService.CreateAsync(projectId, request);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("/api/projects/{projectId:int}/tasks")]
        [ProducesResponseType(typeof(PagedResponse<TaskListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResponse<TaskListResponse>>> GetByProjectId(int projectId, [FromQuery] TaskQueryParameters parameters)
        {
            var result = await _taskService.GetByProjectIdAsync(projectId, parameters);

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<TaskListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<TaskListResponse>>> GetAll([FromQuery] TaskQueryParameters parameters)
        {
            var result = await _taskService.GetAllAsync(parameters);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);

            return Ok(task);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
        {
            var updated = await _taskService.UpdateAsync(id, request);

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _taskService.DeleteAsync(id);

            return NoContent();
        }
    }
}