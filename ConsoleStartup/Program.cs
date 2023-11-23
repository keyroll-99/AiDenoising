// See https://aka.ms/new-console-template for more information

using AiDenoising;
using AiDenoisingUi.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var perceptrons = new List<Perceptron>();
for (var i = 0; i < (50 * 50); i++)
{
    var perceptron = new Perceptron(Enum.GetName(ImageType.House), i);
    perceptrons.Add(perceptron);
}

var tasks = perceptrons.Select(perceptron => Task.Run(perceptron.Train)).ToList();
await Task.WhenAll(tasks);

var tester = new Tester(perceptrons, "SmileFace");

await tester.Test();

//
// foreach (var perceptron in perceptrons)
// {
//     Console.WriteLine(
//         $"index {perceptron.PixelIndex} is {perceptron.Predict(originalImage)} should be {originalImage[perceptron.PixelIndex]}");
// }