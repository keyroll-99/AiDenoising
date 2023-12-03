using System.Collections;
using AiDenoising;

namespace AiDenoisingUi.Data;

public class DenoisingService
{
    private readonly IList<Perceptron> _perceptrons = new List<Perceptron>();
    private readonly ImageLoader _imageLoader;

    public DenoisingService()
    {
        _imageLoader = new ImageLoader();
        for (var i = 0; i < 50 * 50; i++)
        {
            _perceptrons.Add(new Perceptron(i, _imageLoader));
        }
    }

    public async Task<IList<int>> LoadImage(ImageType imageType)
    {
        return (await _imageLoader.GetMainImage(imageType)).Select(x => x).ToList();
    }

    public async Task Learn()
    {
        var tasks = _perceptrons.Select(perceptron => Task.Run(perceptron.Train)).ToList();

        await Task.WhenAll(tasks);
    }

    public async Task<IList<int>> Denoise(IList<int> data)
    {
        return _perceptrons.Select(perceptron => perceptron.Predict(data)).ToList();
    }
}