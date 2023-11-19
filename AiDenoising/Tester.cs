using System.Reflection;

namespace AiDenoising;

public class Tester
{
    private readonly IList<Perceptron> _perceptrons;
    private readonly string _imageType;
    private readonly List<IList<int>> _testCases = new();
    private readonly Rgba32 _black = new(0, 0, 0);
    private readonly Rgba32 _white = new(255, 255, 255);

    private const int ImageWidth = 50;

    public Tester(IList<Perceptron> perceptrons, string imageType)
    {
        _perceptrons = perceptrons;
        _imageType = imageType;
    }

    public async Task Test()
    {
        await LoadTestImages();
        var tasks = _testCases.Select((t, i) => Task.Run(() => Test(t, i))).ToList();

        await Task.WhenAll(tasks);
    }

    private void Test(IList<int> testCase, int index)
    {
        var correctPixel = 0;
        var originalImage = _perceptrons[0].OriginalImage;
        var resultImage = new List<int>();
        for (var i = 0; i < (50 * 50); i++)
        {
            var result = _perceptrons[i].Predict(testCase);
            if (result == originalImage[i])
            {
                correctPixel++;
            }

            resultImage.Add(result);
        }

        Console.WriteLine($"Test case {index} - Correct pixels {correctPixel} with {50 * 50}");
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);


        using var image = new Image<Rgba32>(50, 50);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    row[x] = resultImage[(ImageWidth * y) + x] == 1 ? _black : _white;
                }
            }
        });

        image.Save(
            $"{path}{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}Tests{Path.DirectorySeparatorChar}{_imageType}{Path.DirectorySeparatorChar}Results{Path.DirectorySeparatorChar}test_{index}.png");
    }

    private async Task LoadTestImages()
    {
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var files = Directory
            .EnumerateFiles(
                $"{path}{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}Tests{Path.DirectorySeparatorChar}{_imageType}");

        foreach (var file in files)
        {
            _testCases.Add(await ParseImage(file));
        }
    }

    private static async Task<IList<int>> ParseImage(string file)
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