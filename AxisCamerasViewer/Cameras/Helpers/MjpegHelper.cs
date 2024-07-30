namespace AxisCamerasViewer.Cameras.Helpers;

public static class MjpegHelper
{
    private const int FillByte = 0xFF,
                     SoiByte = 0xD8,
                     EoiByte = 0xD9,
                     ChunkSize = 4096;
    
    public static async Task ProcessMjpegStreamAsync(Stream stream, Action<byte[]> onImage, CancellationToken cancellationToken)
    {
        using (var imageStream = new MemoryStream())
        {
            var buffer = new byte[ChunkSize];
            var isReadingImage = false;
                
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                for (var i = 0; i < bytesRead; i++)
                {
                    if (isReadingImage)
                    {
                        imageStream.WriteByte(buffer[i]);
                            
                        if (buffer[i] == EoiByte && 
                            imageStream.Length > 1 && 
                            imageStream.GetBuffer()[imageStream.Length - 2] == FillByte)
                        {
                            onImage.Invoke(imageStream.ToArray());
                            
                            isReadingImage = false;
                            imageStream.SetLength(0);
                        }
                    }
                    else if (buffer[i] == SoiByte && 
                             i > 0 && 
                             buffer[i - 1] == FillByte)
                    {
                        imageStream.Write([ FillByte, SoiByte ], 0, 2);
                        isReadingImage = true;
                    }
                }
            }
        }
    }
}