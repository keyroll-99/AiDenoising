// See https://aka.ms/new-console-template for more information

using AiDenoising;
using AiDenoisingUi.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var perceptrons = new List<Perceptron>();
var tasks = new List<Task>();
var imageLoader = new ImageLoader();

for (var i = 0; i < (50 * 50); i++)
{
    var perceptron = new Perceptron(i, imageLoader);
    perceptrons.Add(perceptron);
    tasks.Add(perceptron.Train());
}

await Task.WhenAll(tasks);

foreach (var imageName in Enum.GetValues(typeof(ImageType)).Cast<ImageType>())
{
    var tester = new Tester(perceptrons, imageName);

    await tester.Test(imageLoader);

}


//
// foreach (var perceptron in perceptrons)
// {
//     Console.WriteLine(
//         $"index {perceptron.PixelIndex} is {perceptron.Predict(originalImage)} should be {originalImage[perceptron.PixelIndex]}");
// }