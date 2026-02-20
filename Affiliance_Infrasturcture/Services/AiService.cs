using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
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
        private readonly InferenceSession _idCardSession;

        public AiService()
        {
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModel", "detect_id_card.onnx");
            if (File.Exists(modelPath))
            {
                _idCardSession = new InferenceSession(modelPath);
            }
        }

        public async Task<string> AnalyzeImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0) return "Image is empty";
            return await Task.Run(() =>
            {
                try
                {
                    using var stream = image.OpenReadStream();
                    using var img = Image.Load<Rgb24>(stream);

                    // 1. Resize مع الحفاظ على الأبعاد (Letterbox) عشان البطاقة متتمطش
                    img.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(640, 640),
                        Mode = ResizeMode.Pad // بيضيف حواف سودة بدل ما يمط الصورة
                    }));

                    var input = new DenseTensor<float>(new[] { 1, 3, 640, 640 });
                    img.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < accessor.Height; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < accessor.Width; x++)
                            {
                                // جربنا الـ RGB، لو لسه ضعيف الموديل ده غالباً محتاج BGR
                                // هنعكس الـ R والـ B هنا ونشوف النتيجة
                                input[0, 0, y, x] = row[x].B / 255f; // Blue
                                input[0, 1, y, x] = row[x].G / 255f; // Green
                                input[0, 2, y, x] = row[x].R / 255f; // Red
                            }
                        }
                    });

                    var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_idCardSession.InputMetadata.Keys.First(), input) };
                    using var results = _idCardSession.Run(inputs);
                    var outputTensor = results.First().AsTensor<float>();

                    float maxConfidence = 0f;
                    int channels = outputTensor.Dimensions[1]; // 12
                    int boxes = outputTensor.Dimensions[2];    // 8400

                    for (int c = 4; c < channels; c++)
                    {
                        for (int b = 0; b < boxes; b++)
                        {
                            float score = outputTensor[0, c, b];
                            if (score > maxConfidence) maxConfidence = score;
                        }
                    }

                    Console.WriteLine($"\n*** NEW TEST REPORT ***");
                    Console.WriteLine($"Max Confidence: {maxConfidence * 100:0.00}%");
                    Console.WriteLine($"***********************\n");

                    // لو السكور لسه تحت الـ 40%، الموديل ده محتاج يتغير أو التدريب بتاعه فيه مشكلة
                    return maxConfidence > 0.35f ? "Success" : "Invalid_ID";
                }
                catch (Exception ex) { return $"Error: {ex.Message}"; }
            });
        }
    }
}