using AxisCamerasViewer.Models;
using Microsoft.AspNetCore.Mvc;

namespace AxisCamerasViewer.Controllers;

[Route("viewer")]
public class ViewerController : Controller
{
    private readonly ICameraService _cameraService;

    public ViewerController(ICameraService cameraService)
    {
        _cameraService = cameraService;
    }
    
    [HttpPost("camera")]
    public IActionResult CreateCamera([FromBody] CreateCameraRequest request)
    {
        var id = _cameraService.CreateCamera(request.Url);
        
        return Ok(new { id });
    }

    [HttpGet("camera/{id}")]
    public async Task GetCamera(Guid id)
    {
        var camera = _cameraService.GetCamera(id);
        
        await camera.FetchContent();
        await camera.StartWatching();
    }

    [HttpDelete("camera/{id}")]
    public IActionResult RemoveCamera(Guid id)
    {
        _cameraService.RemoveCamera(id);
        
        return Ok();
    }
}