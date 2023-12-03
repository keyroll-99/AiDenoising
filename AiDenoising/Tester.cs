using System.Reflection;
using AiDenoisingUi.Data;

namespace AiDenoising;

public class Tester
{
    private readonly IList<Perceptron> _perceptrons;
    private readonly ImageType _imageType;
    private readonly List<IList<int>> _testCases = new();
    private readonly Rgba32 _black = new(0, 0, 0);
    private readonly Rgba32 _white = new(255, 255, 255);

    private const int ImageWidth = 50;

    public Tester(IList<Perceptron> perceptrons, ImageType imageType)
    {
        _perceptrons = perceptrons;
        _imageType = imageType;
    }

    public async Task Test(ImageLoader imageLoader)
    {
        await LoadTestImages();
        var tasks = _testCases.Select((x, i) => Test(x, i, imageLoader)).ToList();


        await Task.WhenAll(tasks);
    }

    private async Task Test(IList<int> testCase, int index, ImageLoader imageLoader)
    {
        var correctPixel = 0;

        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        var originalImage = await imageLoader.GetMainImage(_imageType);
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
            _testCases.Add(ImageLoader.ParseImage(file));
        }
    }
    
}