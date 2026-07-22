using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Projects;
using TaskManagement.Application.Interfaces.Services;
namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request)
        {
            var created = await _projectService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<ProjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<ProjectResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _projectService.GetAllAsync(page, limit);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectResponse>> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id);

            return Ok(project);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProjectResponse>> Update(int id, UpdateProjectRequest request)
        {
            var updated = await _projectService.UpdateAsync(id, request);

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _projectService.DeleteAsync(id);

            return NoContent();
        }
    }
}