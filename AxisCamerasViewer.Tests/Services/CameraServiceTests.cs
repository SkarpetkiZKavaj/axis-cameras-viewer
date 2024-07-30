using System.Collections.Concurrent;
using AxisCamerasViewer.Cameras.Hubs;
using AxisCamerasViewer.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace AxisCamerasViewer.Tests;

public class CameraServiceTests
{
    private readonly CameraService _cameraService;

    private const string CameraUrl = "http://63.142.183.154:6103/mjpg/video.mjpg";
    
    public CameraServiceTests()
    {
        var httpClient = Substitute.For<HttpClient>();
        var hubContext = Substitute.For<IHubContext<CameraHub>>();
        var videoService = Substitute.For<IMjpegVideoService>();
        
        _cameraService = new CameraService(httpClient, hubContext, videoService);
    }
    
    [Theory]
    [InlineData(CameraUrl)]
    public void CreateCamera_WhenUrlIsNotNullOrEmpty_ShouldReturnCameraId(string url)
    {
        var cameraServiceType = typeof(CameraService);
        var camerasField = cameraServiceType.GetField("_cameras", System.Reflection.BindingFlags.NonPublic | 
                                                                                System.Reflection.BindingFlags.Instance);
        var cameras = (ConcurrentDictionary<Guid, Camera>)camerasField.GetValue(_cameraService);

        var cameraId = _cameraService.CreateCamera(url);

        Assert.NotEqual(Guid.Empty, cameraId);
        Assert.True(cameras?.ContainsKey(cameraId));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateCamera_WhenUrlIsNull_ShouldThrowArgumentException(string url)
    {
        Assert.ThrowsAny<ArgumentException>(() => _cameraService.CreateCamera(url));
    }
    
    [Theory]
    [InlineData(CameraUrl)]
    public void GetCamera_WhenCameraIdIsValid_ShouldReturnCamera(string url)
    {
        var cameraId = _cameraService.CreateCamera(url);
        
        var camera = _cameraService.GetCamera(cameraId);
        
        Assert.NotNull(camera);
    }
    
    [Fact]
    public void GetCamera_WhenIdIsValid_ShouldReturnCamera()
    {
        var cameraId = _cameraService.CreateCamera(CameraUrl);

        var camera = _cameraService.GetCamera(cameraId);
        
        var cameraType = typeof(Camera);
        var urlField = cameraType.GetField("_url", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cameraUrl = (string)urlField.GetValue(camera);
        
        Assert.NotNull(camera);
        Assert.Equal(CameraUrl, cameraUrl);
    }

    [Fact]
    public void GetCamera_WhenIdIsInvalid_ShouldThrowArgumentException()
    {
        var invalidId = Guid.NewGuid();
        
        Assert.ThrowsAny<ArgumentException>(() => _cameraService.GetCamera(invalidId));
    }

    [Fact]
    public void RemoveCamera_WhenIdIsValid_ShouldReturnTrue()
    {
        var cameraId = _cameraService.CreateCamera(CameraUrl);
        
        var result = _cameraService.RemoveCamera(cameraId);
        
        var cameraServiceType = typeof(CameraService);
        var camerasField = cameraServiceType.GetField("_cameras", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cameras = (ConcurrentDictionary<Guid, Camera>)camerasField.GetValue(_cameraService);
        
        Assert.True(result);
        Assert.False(cameras.ContainsKey(cameraId));
    }

    [Fact]
    public void RemoveCamera_WhenIdIsInvalid_ShouldReturnFalse()
    {
        var invalidId = Guid.NewGuid();
        
        var result = _cameraService.RemoveCamera(invalidId);
        
        Assert.False(result);
    }
}