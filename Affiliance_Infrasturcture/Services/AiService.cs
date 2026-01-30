using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Affiliance_Infrasturcture.Services
{
    public class AiService : IAiService
    {
        private readonly InferenceSession _idCardSession;
    
        public AiService()
        {
            // تحميل الموديل مرة واحدة فقط عند بدء الخدمة
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModel", "detect_id_card.onnx");

            if (File.Exists(modelPath))
            {
                _idCardSession = new InferenceSession(modelPath);
            }
        }

        public async Task<string> AnalyzeImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Image file is empty");

            if (_idCardSession == null)
                return "Model not initialized properly.";

            return await Task.Run(() =>
            {
                try
                {
                    using var stream = image.OpenReadStream();
                    using var img = Image.Load<Rgb24>(stream);

                    const int targetWidth = 640;
                    const int targetHeight = 640;

                    img.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(targetWidth, targetHeight),
                        Mode = ResizeMode.Stretch
                    }));

                    var input = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });

                    img.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                            for (int x = 0; x < accessor.Width; x++)
                            {
                                input[0, 0, y, x] = pixelSpan[x].R / 255f;
                                input[0, 1, y, x] = pixelSpan[x].G / 255f;
                                input[0, 2, y, x] = pixelSpan[x].B / 255f;
                            }
                        }
                    });

                    var inputName = _idCardSession.InputMetadata.Keys.First();
                    var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, input) };

                    using var results = _idCardSession.Run(inputs);
                    var namedOutput = results.First();
                    var outputTensor = namedOutput.AsTensor<float>();
                    var dims = outputTensor.Dimensions;

                    if (dims.Length == 3)
                    {
                        // dims: [1, channels, boxes]
                        int channels = dims[1];
                        int boxes = dims[2];

                        // YOLO-like: [x, y, w, h, objectness, class0, class1, ...]
                        const int classStartIndex = 5;
                        float maxConfidence = 0f;

                        for (int b = 0; b < boxes; b++)
                        {
                            float objectness = outputTensor[0, 4, b];
                            float bestClassProb = 0f;

                            if (channels > classStartIndex)
                            {
                                for (int c = classStartIndex; c < channels; c++)
                                {
                                    float p = outputTensor[0, c, b];
                                    if (p > bestClassProb) bestClassProb = p;
                                }
                            }
                            else
                            {
                                // No class probabilities present — treat objectness as final confidence
                                bestClassProb = 1f;
                            }

                            float conf = objectness * bestClassProb;
                            if (conf > maxConfidence) maxConfidence = conf;
                        }

                        const float threshold = 0.80f;
                        return maxConfidence > threshold ? "Success" : "Invalid_ID";
                    }
                    else
                    {
                        // Fallback: flatten-based check (previous behavior)
                        var flat = outputTensor.ToArray();
                        bool isIdDetected = flat.Any(v => v > 0.75f);
                        return isIdDetected ? "Success" : "Invalid_ID";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            });
        }
    }
}
