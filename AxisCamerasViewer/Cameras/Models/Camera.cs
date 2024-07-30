using AxisCamerasViewer.Cameras.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AxisCamerasViewer.Models;

public class Camera
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient;
    private readonly IHubContext<CameraHub> _hubContext;
    private readonly IMjpegVideoService _videoService;
    private readonly Guid _id;
    private readonly string _url;
    
    private HttpContent _content;

    public Camera(HttpClient client, 
           IHubContext<CameraHub> hubContext, 
           IMjpegVideoService videoService,
           Guid id, 
           string url)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _httpClient = client;
        _hubContext = hubContext;
        _videoService = videoService;
        _id = id;
        _url = url;
    }

    public async Task<HttpContent> FetchContent()
    {
        var response = await _httpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        _content = response.Content;

        return _content;
    }

    public async Task StartWatching()
    {
        if (_content == null)
        {
            throw new InvalidOperationException("Camera content is not fetched");
        }

        try
        {
            await using (var responseStream = await _content.ReadAsStreamAsync(_cancellationTokenSource.Token))
            {
                await _videoService.ProcessMjpegVideoAsync(responseStream, onImage: image =>
                {
                    _hubContext.Clients.All.SendAsync("SendImage", _id, image);
                }, _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException ex)
        {}
    }
    
    public void StopWatching()
    {
        _cancellationTokenSource.Cancel();
    } 
}