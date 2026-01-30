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
                    var output = results.First().AsEnumerable<float>().ToArray();

                    bool isIdDetected = output.Any(confidence => confidence > 0.75f);

                    return isIdDetected ? "Success" : "Invalid_ID";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            });
        }
    }
}
