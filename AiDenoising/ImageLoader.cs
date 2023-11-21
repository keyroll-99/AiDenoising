using System.Reflection;
using AiDenoisingUi.Data;

namespace AiDenoising;

public class ImageLoader
{
    public static async Task<IList<int>> GetMainImage(ImageType imageType)
    {
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        
        var file = Directory
            .EnumerateFiles(
                $"{path}{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}{Enum.GetName(imageType)}")
            .FirstOrDefault(file => file.Split("\\").Last()[0] == '0');

        return await ParseImage(file);
    }
    
    public static async Task<IList<int>> ParseImage(string file)
    {
        var image = await Image.LoadAsync<Rgba32>(file);
        var parseImageData = new List<int>();
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                foreach (ref var pixel in pixelRow)
                {
                    var color = (pixel.R + pixel.G + pixel.B) / 3;
                    parseImageData.Add(pixel.A > 0 && color < 127 ? 1 : 0);
                }
            }
        });
        return parseImageData;
    }


}