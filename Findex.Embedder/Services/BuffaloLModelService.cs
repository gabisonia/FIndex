using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace Findex.Embedder.Services;

public class BuffaloLModelService
{
    private readonly InferenceSession _session = new("Data/webface_r50.onnx");

    public float[]? GetEmbedding(byte[] imageData)
    {
        var mat = Cv2.ImDecode(imageData, ImreadModes.Color);
        if (mat.Empty())
            return null;

        var resized = mat.Resize(new Size(112, 112));
        Cv2.CvtColor(resized, resized, ColorConversionCodes.BGR2RGB);
        resized.ConvertTo(resized, MatType.CV_32FC3, 1.0 / 255);

        var inputTensor = new DenseTensor<float>([1, 3, 112, 112]);
        for (var c = 0; c < 3; c++) // Channels first
        for (var y = 0; y < 112; y++)
        for (var x = 0; x < 112; x++)
            inputTensor[0, c, y, x] = resized.At<Vec3f>(y, x)[c];

        // https://netron.app
        var modelInputName = "input.1";
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(modelInputName, inputTensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        var norm = (float)Math.Sqrt(output.Sum(v => v * v));
        return output.Select(v => v / norm).ToArray();
    }
}