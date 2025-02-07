using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SourceAFIS;

namespace FingerPrintApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FingerPrintVerificationController : ControllerBase
    {
        public class VerificationResult
        {
            public bool IsMatch { get; set; }
            public int Score { get; set; }
            public string Message { get; set; }
        }

        public class FingerDto
        {
            public string SavedFingerUrl { get; set; }
            public string UserFingerUrl { get; set; }
        }

        private readonly HttpClient _httpClient;

        public FingerPrintVerificationController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyFingerPrint([FromBody] FingerDto fingerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(fingerDto.SavedFingerUrl) || string.IsNullOrEmpty(fingerDto.UserFingerUrl))
                {
                    return BadRequest(new VerificationResult
                    {
                        IsMatch = false,
                        Message = "Fingerprint URL cannot be empty"
                    });
                }

                // Download images from URLs
                var savedImageBytes = await DownloadImageAsync(fingerDto.SavedFingerUrl);
                var userImageBytes = await DownloadImageAsync(fingerDto.UserFingerUrl);

                if (savedImageBytes == null || userImageBytes == null)
                {
                    return BadRequest(new VerificationResult
                    {
                        IsMatch = false,
                        Message = "Failed to download fingerprint images"
                    });
                }

                // Convert images to SourceAFIS format
                var savedTemplate = ExtractTemplate(savedImageBytes);
                var userTemplate = ExtractTemplate(userImageBytes);

                if (savedTemplate == null || userTemplate == null)
                {
                    return BadRequest(new VerificationResult
                    {
                        IsMatch = false,
                        Message = "Failed to process fingerprint images"
                    });
                }

                // Compare the templates using SourceAFIS
                var matcher = new FingerprintMatcher(savedTemplate);
                var score = matcher.Match(userTemplate);

                // Define a threshold for matching
                const int threshold = 50; // Adjust this threshold based on your requirements
                bool isMatch = score >= threshold;

                return Ok(new VerificationResult
                {
                    IsMatch = isMatch,
                    Score = (int)score,
                    Message = isMatch ? "Fingerprint matched" : "Fingerprint did not match"
                });
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new VerificationResult
                {
                    IsMatch = false,
                    Message = $"An error occurred during verification: {ex.Message}"
                });
            }
        }

        private async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }

        private FingerprintTemplate ExtractTemplate(byte[] imageBytes)
        {
            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    // Convert the image to a format SourceAFIS can process
                    var image = Image.FromStream(ms);
                    var bitmap = new Bitmap(image);

                    // Create a FingerprintImage object with required parameters
                    var fingerprintImage = new FingerprintImage(bitmap.Width, bitmap.Height, GetPixels(bitmap));


                    // Create a FingerprintTemplate from the FingerprintImage
                    var template = new FingerprintTemplate(fingerprintImage);
                    return template;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error extracting template: {ex.Message}");
                return null;
            }
        }

        private byte[] GetPixels(Bitmap bitmap)
        {
            // Convert the bitmap to grayscale and extract pixel data
            var width = bitmap.Width;
            var height = bitmap.Height;
            var pixels = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    // Convert to grayscale using the luminance formula
                    byte grayscale = (byte)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                    pixels[y * width + x] = grayscale;
                }
            }

            return pixels;
        }
    }
}
