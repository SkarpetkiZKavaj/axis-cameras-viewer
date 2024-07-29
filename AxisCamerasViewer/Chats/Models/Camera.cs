namespace AxisCamerasViewer.Models;

public class Camera
{
    private readonly string _url;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient;
    private HttpContent _content;

    public Camera(HttpClient client, string url)
    {
        _url = url;
        
        _httpClient = client;
        _cancellationTokenSource = new CancellationTokenSource();
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

    public async Task StartWatching(Stream receiver)
    {
        if (_content == null)
        {
            throw new InvalidOperationException("Camera content is not fetched");
        }

        try
        {
            await using (var responseStream = await _content.ReadAsStreamAsync(_cancellationTokenSource.Token))
            {
                await responseStream.CopyToAsync(receiver, _cancellationTokenSource.Token);
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