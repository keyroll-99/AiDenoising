using System.Collections;
using AiDenoising;

namespace AiDenoisingUi.Data;

public class DenoisingService
{
    private readonly Dictionary<ImageType, IList<Perceptron>> _perceptrons = new ();

    public DenoisingService()
    {
        _perceptrons[ImageType.SmileFace] = new List<Perceptron> ();
        for (var i = 0; i < 50 * 50; i++)
        {
            _perceptrons[ImageType.SmileFace].Add(new Perceptron(Enum.GetName(ImageType.SmileFace), i));
        }
    }

    public async Task<IList<int>> LoadImage(ImageType imageType)
    {
        return await ImageLoader.GetMainImage(imageType);
    }

    public async Task Learn(ImageType imageType)
    {
        var tasks = _perceptrons[imageType].Select(perceptron => Task.Run(perceptron.Train)).ToList();

        await Task.WhenAll(tasks);
    }

    public async Task<IList<int>> Denoising(ImageType imageType, IList<int> data)
    {
        return _perceptrons[imageType].Select(perceptron => perceptron.Predict(data)).ToList();
    }
}