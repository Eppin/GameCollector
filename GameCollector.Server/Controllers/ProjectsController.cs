namespace GameCollector.Server.Controllers;

using System.Net.Mime;
using Microsoft.Extensions.Caching.Memory;
using Services;
using Utils;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(ILogger<ProjectsController> logger, ProjectService projectService, IMemoryCache cache) : ControllerBase
{
    [HttpGet, Produces(MediaTypeNames.Application.Json)]
    public IActionResult Get()
    {
        try
        {
            var projects = projectService.Projects
                .Select(p => p.SaveType)
                .OrderBy(p => p);

            return new OkObjectResult(projects);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve projects");
            return new BadRequestResult();
        }
    }

    [HttpGet("Items/{projectId:int}"), Produces(MediaTypeNames.Application.Json)]
    public IActionResult Project(int projectId)
    {
        try
        {
            var project = projectService.Projects.FirstOrDefault(p => p.SaveType == (SaveType)projectId);
            if (project != null)
                return new OkObjectResult(project.Groups);

            logger.LogWarning("Unable to retrieve project [{ID}]", projectId);
            return new BadRequestResult();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve projects");
            return new BadRequestResult();
        }
    }

    [HttpGet("Single/{projectId:int}"), Produces(MediaTypeNames.Application.Json)]
    public IActionResult Single(int projectId)
    {
        try
        {
            if (!Enum.IsDefined(typeof(SaveType), projectId))
                return new BadRequestResult();

            var key = $"{(SaveType)projectId}_game";
            if (!cache.TryGetValue<Game>(key, out var data))
                return new NotFoundResult();

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve for project {ProjectId}", projectId);
            return new BadRequestResult();
        }
    }

    [HttpGet("Content/{projectId:int}"), Produces(MediaTypeNames.Application.Json)]
    public IActionResult Content(int projectId)
    {
        try
        {
            if (!Enum.IsDefined(typeof(SaveType), projectId))
                return new BadRequestResult();

            var key = $"{(SaveType)projectId}_data";
            if (!cache.TryGetValue<List<Data>>(key, out var data))
                return new NotFoundResult();

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve content for project {ProjectId}", projectId);
            return new BadRequestResult();
        }
    }
}
