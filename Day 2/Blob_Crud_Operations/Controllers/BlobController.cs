using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Blob_Crud_Operations.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "myfiles";  //Your Blob container name
        private readonly BlobContainerClient _containerClient;
        //private readonly ILogger _logger;
        private readonly TelemetryClient _telemetry;

        public BlobController(BlobServiceClient blobServiceClient, TelemetryClient telemetry) //ILogger logger)
        {
            _blobServiceClient = blobServiceClient;
            _containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            _containerClient.CreateIfNotExists();
            _telemetry = telemetry;
        }

        [HttpGet("listblobs")]
        public async Task<IActionResult> ListBlobs()
        {
            _telemetry.TrackEvent("Entering in ListBlobs ");
            _telemetry.TrackTrace("Trace message from ListBlobs");

            List<string> blobNames = new List<string>();
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

                await foreach (BlobItem blob in containerClient.GetBlobsAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blob.Name);
                    //blobNames.Add("Blob Name: " + blob.Name + "Blob Url: " + $"{ containerClient.Uri}/{blob.Name}");
                    blobNames.Add("Blob Url: " + $"{blobClient.Uri.ToString()}");
                }
                throw new Exception("Service is down");
            }
            catch(Exception ex)
            {
                //_logger.LogError("Exception: " + ex.Message);
                _telemetry.TrackException(ex);
            }
            return Ok(blobNames);
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file selected.");
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
                var blobClient = containerClient.GetBlobClient(file.FileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }
            }
            catch (Exception ex)
            {

            }
            return Ok($"File {file.FileName} uploaded successfully.");
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteBlob(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound("File not found.");
            }

            await blobClient.DeleteAsync();
            return RedirectToAction("ListBlobs");
        }

        /*
        [HttpGet]
        public async Task<IActionResult> DownloadBlob(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound("File not found.");
            }

            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            return File(stream, "application/octet-stream", fileName);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateBlob(string oldFileName, IFormFile newFile)
        {
            if (newFile == null || newFile.Length == 0)
            {
                return BadRequest("No new file provided.");
            }

            // Delete the old file
            await DeleteBlob(oldFileName);

            // Upload the new file
            await UploadBlob(newFile);

            return RedirectToAction("ListBlobs");
        }

        */
    }


}

