using System.Collections.Concurrent;
using AxisCamerasViewer.Cameras.Hubs;
using AxisCamerasViewer.Models;
using Microsoft.AspNetCore.SignalR;

namespace AxisCamerasViewer;

public interface ICameraService
{
    public Guid CreateCamera(string url);
    public Camera GetCamera(Guid id);
    public bool RemoveCamera(Guid id);
}

public class CameraService : ICameraService
{
    private readonly ConcurrentDictionary<Guid, Camera> _cameras = new ();
    private readonly HttpClient _httpClient;
    private readonly IHubContext<CameraHub> _hubContext;
    private readonly IMjpegVideoService _videoService;

    public CameraService(HttpClient httpClient, 
                         IHubContext<CameraHub> hubContext,
                         IMjpegVideoService videoService)
    {
        _httpClient = httpClient;
        _hubContext = hubContext;
        _videoService = videoService;
    }
    
    public Guid CreateCamera(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        
        var id = Guid.NewGuid();
        var camera = new Camera(_httpClient, _hubContext, _videoService, id, url);
        
        _cameras.TryAdd(id, camera);
        
        return id;
    }

    public Camera GetCamera(Guid id)
    {
        if (_cameras.TryGetValue(id, out var camera))
        {
            return camera;
        }
        
        throw new ArgumentException("Invalid camera Id");
    }

    public bool RemoveCamera(Guid id)
    {
        var isRemoved = _cameras.TryRemove(id, out var camera);
        if (isRemoved)
        {
            camera?.StopWatching();
        }
        
        return isRemoved;
    }
}