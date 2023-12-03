using System.Net.Mime;
using System.Reflection;
using AiDenoisingUi.Data;

namespace AiDenoising;

public class Perceptron
{
    private const double LearningConst = 0.1;
    private readonly Random _random;
    private IList<double> _weights = new List<double>();

    private double _biasWeight;
    private readonly double _threshold;
    private readonly int _pixelIndex;
    private ImageLoader _imageLoader;


    public Perceptron(int pixelIndex, ImageLoader imageLoader, int imageWidth = 50, int imageHeight = 50)
    {
        _pixelIndex = pixelIndex;
        _random = new Random();
        _biasWeight = _random.NextDouble() * 2 - 1;
        for (var i = 0; i < (imageHeight * imageWidth); i++)
        {
            var randomWeight = _random.NextDouble() * 2 - 1;
            _weights.Add(randomWeight);
        }

        _imageLoader = imageLoader;
        _threshold = 0;
    }


    public async Task Train()
    {
        Console.WriteLine($"Start learn pixel {_pixelIndex}");
        
        var i = 0L;
        var maxLifeTime = 0;
        var currentLifeTime = 0;
        var weights = new List<double>();
        var biasFromMaxLifeTime = 0D;
        while (i <= 10_000)
        {
            var values = Enum.GetValues(typeof(ImageType));
            var randomImageType = (ImageType)values.GetValue(_random.Next(values.Length));
            var learningIndex = RandomNextIndexOfLearningData(randomImageType);
            var learningCase = _imageLoader.LearningData[randomImageType][learningIndex];

            var target = (await _imageLoader.GetMainImage(randomImageType))[_pixelIndex];
            var error = GetError(learningCase, target);


            if (error == 0)
            {
                currentLifeTime++;
            }
            else
            {
                if (currentLifeTime > maxLifeTime)
                {
                    maxLifeTime = currentLifeTime;
                    weights = _weights.Select(x => x).ToList();
                    biasFromMaxLifeTime = _biasWeight;
                }


                for (var index = 0; index < _weights.Count; index++)
                {
                    _weights[index] += error * LearningConst * learningCase[index];
                }

                _biasWeight += LearningConst * error;
                currentLifeTime = 0;
            }


            i++;
        }

        if (currentLifeTime > maxLifeTime)
        {
            maxLifeTime = currentLifeTime;
            weights = _weights.Select(x => x).ToList();
            biasFromMaxLifeTime = _biasWeight;
        }

        _weights = weights.Select(x => x).ToList();
        _biasWeight = biasFromMaxLifeTime;

        Console.WriteLine($"Max lifetime for {_pixelIndex} is {maxLifeTime}");
        Console.WriteLine("end");
    }

    public int Predict(IList<int> data)
    {
        if (data.Count != _weights.Count)
        {
            throw new ArgumentException();
        }

        var sum = data.Select((t, i) => t * _weights[i]).Sum();

        sum += _biasWeight;

        return sum >= _threshold ? 1 : 0;
    }

    private int GetError(IList<int> input, int target)
    {
        var guess = Predict(input);
        return target - guess;
    }


    private static IList<int> ParseImage(string file)
    {
        var image = Image.Load<Rgba32>(file);
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


    private int RandomNextIndexOfLearningData(ImageType imageType)
    {
        return _random.Next(0, _imageLoader.LearningData[imageType].Count);
    }
}