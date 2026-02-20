using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Affiliance_Infrasturcture.Services
{
    public class AiService : IAiService
    {
        private readonly object? _idCardSession;
        private readonly bool _isAvailable;

        public AiService()
        {
            try
            {
                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModel", "detect_id_card.onnx");
                if (File.Exists(modelPath))
                {
                    _idCardSession = CreateSession(modelPath);
                    _isAvailable = _idCardSession != null;
                }
            }
            catch (Exception)
            {
                _isAvailable = false;
                _idCardSession = null;
            }
        }

        private static object? CreateSession(string modelPath)
        {
            try
            {
                return new Microsoft.ML.OnnxRuntime.InferenceSession(modelPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> AnalyzeImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0) return "Image is empty";

            if (!_isAvailable || _idCardSession == null)
                return "AI service is not available on this server. ID verification is skipped.";

            return await Task.Run(() =>
            {
                try
                {
                    var session = (Microsoft.ML.OnnxRuntime.InferenceSession)_idCardSession;

                    using var stream = image.OpenReadStream();
                    using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgb24>(stream);

                    img.Mutate(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(640, 640),
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Pad
                    }));

                    var input = new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(new[] { 1, 3, 640, 640 });
                    img.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < accessor.Width; x++)
                            {
                                input[0, 0, y, x] = row[x].R / 255f;
                                input[0, 1, y, x] = row[x].G / 255f;
                                input[0, 2, y, x] = row[x].B / 255f;
                            }
                        }
                    });

                    var inputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
                    {
                        Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor(session.InputMetadata.Keys.First(), input)
                    };
                    using var results = session.Run(inputs);
                    var outputTensor = results.First().AsTensor<float>();

                    float maxConfidence = 0f;
                    int channels = outputTensor.Dimensions[1];
                    int boxes = outputTensor.Dimensions[2];

                    for (int c = 4; c < channels; c++)
                    {
                        for (int b = 0; b < boxes; b++)
                        {
                            float score = outputTensor[0, c, b];
                            if (score > maxConfidence) maxConfidence = score;
                        }
                    }

                    return maxConfidence > 0.5f ? "Success" : "Invalid_ID";
                }
                catch (Exception ex) { return $"Error: {ex.Message}"; }
            });
        }
    }
}