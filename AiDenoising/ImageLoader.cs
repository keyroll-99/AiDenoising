using System.Reflection;
using AiDenoisingUi.Data;

namespace AiDenoising;

public class ImageLoader
{
    public Dictionary<ImageType, List<IList<int>>> LearningData { get; private set; } = new();


    public ImageLoader()
    {
        LoadImages();
    }

    public async Task<IList<int>> GetMainImage(ImageType imageType)
    {
        if (LearningData.TryGetValue(imageType, out var learningCase))
        {
            return learningCase[0];
        }

        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        
        var file = Directory
            .EnumerateFiles(
                $"{path}{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}{Enum.GetName(imageType)}")
            .FirstOrDefault(file => file.Split("\\").Last()[0] == '0');

        return ParseImage(file);
    }

    public void LoadImages()
    {
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);


        foreach (var imageType in Enum.GetValues(typeof(ImageType)).Cast<ImageType>())
        {
            LearningData[imageType] = new List<IList<int>>();

            var files = Directory.EnumerateFiles(
                $"{path}{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}{Enum.GetName(imageType)}");

            foreach (var file in files)
            {
                var parseImageData = ParseImage(file);

                LearningData[imageType].Add(parseImageData);
            }
        }
    }

    public static IList<int> ParseImage(string file)
    {
        var image =  Image.Load<Rgba32>(file);
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