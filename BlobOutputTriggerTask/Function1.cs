using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace BlobOutputTriggerTask
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public Function1(ILogger<Function1> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        [Function("ProcessBlob")]
        public async Task RunAsync(
            [BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream inputBlob,
            string name)
        {
            _logger.LogInformation($"Processing blob: {name}");

            // Get a reference to the container and blob
            var containerClient = _blobServiceClient.GetBlobContainerClient("images");
            var blobClient = containerClient.GetBlobClient(name);

            // Check file extension
            string fileExtension = Path.GetExtension(name)?.ToLowerInvariant();
            if (fileExtension == ".png")
            {
                _logger.LogInformation($"Blob {name} is a PNG. Compressing to JPEG.");

                // Compress the image to JPEG
                using var image = await Image.LoadAsync(inputBlob);
                using var outputStream = new MemoryStream();
                image.SaveAsJpeg(outputStream, new JpegEncoder { Quality = 75 });
                outputStream.Position = 0;

                // Upload the compressed JPEG back to the container with a .jpg extension
                string newBlobName = Path.ChangeExtension(name, ".jpg");
                var newBlobClient = containerClient.GetBlobClient(newBlobName);
                await newBlobClient.UploadAsync(outputStream, overwrite: true);

                // Delete the original PNG file
                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation($"Blob {name} compressed to JPEG and saved as {newBlobName}. Original PNG deleted.");
            }
            else
            {
                _logger.LogInformation($"Blob {name} is not a PNG. Saving as-is.");

                // Upload the original file back to the container
                using var outputStream = new MemoryStream();
                await inputBlob.CopyToAsync(outputStream);
                outputStream.Position = 0;

                await blobClient.UploadAsync(outputStream, overwrite: true);

                _logger.LogInformation($"Blob {name} saved without modification.");
            }
        }
    }
}
