using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Azure;

namespace CRUD_Blob.Controllers
{
    public class BlobStorageController : Controller
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        BlobServiceClient _blobClient;
        BlobContainerClient _containerClient;
        public BlobStorageController(IConfiguration iConfig)
        {
            _connectionString = iConfig.GetValue<string>("MyConfig:StorageConnection");
            _containerName = iConfig.GetValue<string>("MyConfig:ContainerName");

            _blobClient = new BlobServiceClient(_connectionString);
            _containerClient = _blobClient.GetBlobContainerClient(_containerName);
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet("GetFiles")]
        public async Task<List<string>> GetAllDocuments()
        {
            //string connectionString = _connectionString;
            //string containerName= _container;
            var container = GetContainer();

            if (!await container.ExistsAsync())
            {
                return new List<string>();
            }

            List<string> blobs = new();

            await foreach (BlobItem blobItem in container.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }
            return blobs;
        }

        [Route("UploadFile")]
        [HttpPost]
        public async Task<IActionResult> UploadDocument(List<IFormFile> files) //string containerName, string fileName, Stream fileContent)
        {
            var container = GetContainer();
            //BlobServiceClient blobServiceClient;
            //if (!await container.ExistsAsync())
            //{
            //    blobServiceClient = new(_connectionString);
            //    await blobServiceClient.CreateBlobContainerAsync(_container);
            //    container = blobServiceClient.GetBlobContainerClient(_container);
            //}

            var azureResponse = new List<Azure.Response<BlobContentInfo>>();
            foreach (var file in files)
            {
                string fileName = file.FileName;
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    var client = await _containerClient.UploadBlobAsync(fileName, memoryStream, default);
                    azureResponse.Add(client);
                }
            };

            return Ok(azureResponse); 

            //var bobclient = container.GetBlobClient(fileName);
            //if (!bobclient.Exists())
            //{
            //    fileContent.Position = 0;
            //    await container.UploadBlobAsync(fileName, fileContent);
            //}
            //else
            //{
            //    fileContent.Position = 0;
            //    await bobclient.UploadAsync(fileContent, overwrite: true);
            //}
        }

        [HttpGet("DownloadFile/{fileName}")]
        public async Task<Stream> GetDocument(string fileName)
        {
            var container = GetContainer();
            if (await container.ExistsAsync())
            {
                var blobClient = container.GetBlobClient(fileName);
                if (blobClient.Exists())
                {
                    var content = await blobClient.DownloadStreamingAsync();
                    return content.Value.Content;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

        }

        [Route("DeleteFile/{fileName}")]
        [HttpGet]
        public async Task<bool> DeleteDocument(string fileName)
        {
            var container = GetContainer();
            if (!await container.ExistsAsync())
            {
                return false;
            }

            var blobClient = container.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    
        public BlobContainerClient GetContainer()
        {
            BlobServiceClient blobServiceClient = new(_connectionString);
            return blobServiceClient.GetBlobContainerClient(_containerName);
        }
    }
}
