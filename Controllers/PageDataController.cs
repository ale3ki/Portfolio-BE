using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using nkport_api;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;

//This is a work in progress.
//TODO:
// 1. Add error handling.  Client side error handling is complete but we still need error handling here. 
// 2. Move the SAS generator to a new singleton class and implement rate limits within the class.  
// 3. Add a daily counter to limit SAS token for rate limits, reset counter daily. 
// 4. Add a saftey protocol to regenerate a new blob key in case of malicious pull requests. 

namespace nkport_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PageDataController : ControllerBase
    {
        private readonly ILogger<PageDataController> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Container _container;
        private readonly IConfiguration _configuration;
        private const string _databaseId = "NKPageData";
        private const string _containerId = "PageData";

        public PageDataController(ILogger<PageDataController> logger, CosmosClient cosmosClient, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_databaseId, _containerId);
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        //Pulls individual pages out of the database. 
        //Not used currently but is fully functional.  
        [HttpGet("{pageId}")]
        public async Task<ActionResult<PageData>> GetPageDataByPageId(int pageId){

            QueryDefinition queryDef = new QueryDefinition("SELECT * FROM c WHERE c.pageID = @pageId")
                .WithParameter("@pageId", pageId);
            FeedIterator<PageData> resultSet = _container.GetItemQueryIterator<PageData>(queryDef);
            FeedResponse<PageData> response = await resultSet.ReadNextAsync();

            if (response.Any()){
                PageData pageData = response.First();
                _logger.LogInformation($"Fetched data: {JsonConvert.SerializeObject(pageData)}");
                return Ok(pageData);
            }
            else
            {
                // This will return a HTTP 404 error.
                // Returning null instead of 'NotFound' will produce a HTTP 204 error which has different semantics.
                return NotFound();
            }
        }

        [HttpGet("All")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetPageDataAll(){
            //Pulling all the data out so we can cache it client side and parse it as needed for our components.  
            List<PageData> pagesList = new List<PageData>();
            QueryDefinition myQuery = new QueryDefinition("SELECT * FROM c");
            FeedIterator<PageData> resultSet = _container.GetItemQueryIterator<PageData>(myQuery);
            
            while (resultSet.HasMoreResults){
                FeedResponse<PageData> response = await resultSet.ReadNextAsync();

                foreach(PageData page in response){
                    //_logger.LogInformation($"Fetched data: {JsonConvert.SerializeObject(page)}");
                    string sasToken = GenerateSasTokenForContainer(page.BlobContainer!); 
                    page.BlobAppendSAS = sasToken;
                    pagesList.Add(page);
                }
            }

            if(pagesList.Count != 0){
                return Ok(pagesList);
            }
            return NotFound();
        }

        //https://learn.microsoft.com/en-us/azure/storage/common/storage-sas-overview
        //https://learn.microsoft.com/en-us/rest/api/storageservices/create-account-sas
        //https://learn.microsoft.com/en-us/rest/api/storageservices/create-service-sas

        //The data that we have stored is not sensitive as its for a portfolio website.  Over engineered for fun.
        //We do not set a time frame for the SAS so it defaults to the start time of year 0001
        //Time can skew up to 15 minutes for the start time and end time so the end time is adjusted to 25 minutes. (we might lower this)
        //We are caching our json client side for 15 minutes, after which we fetch new data if the user is still on the website navigating around and return a new SAS token. 

        //This whole system will get changed on the next update.  
        //TODO: Add an api call to refresh the SAS token and expire the previous one (right before it expires).
        //TODO: Implement Azure AD instead of SAS perhaps.  This will mitigate the security issue of the client having access to the SAS token.  
        //This will also increase our backend load times as we will have to pull the image from the backend and then send that to the front end.  
        //We could implement a CDN that helps with limiting pulls from the database.  We do have caching happening but there is the risk of the SAS token being on the client side. 
        private string GenerateSasTokenForContainer(string containerName){

            string accountName = _configuration["BLOB_NAME"];
            string accountKey = _configuration["BLOB_KEY"];
       
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            _logger.LogInformation("CONTAINER NAME: " + containerName);
            _logger.LogInformation("ACCOUNT NAME: " + accountName);

            // Create a BlobSasBuilder object to build a SAS token for the blob
            BlobSasBuilder sasBuilder = new BlobSasBuilder(){
                BlobContainerName = containerClient.Name,
                Resource = "c",  // 'c' for container
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(20)
               
            };
            _logger.LogInformation("STARTS ON: " + sasBuilder.StartsOn);
            _logger.LogInformation("EXPIRES ON: " + sasBuilder.ExpiresOn);

            // Specify read permissions for the SAS token
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // Use the key to get the SAS token
            BlobSasQueryParameters sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey));
            _logger.LogInformation("SAS STRING: " + sasToken);
            return sasToken.ToString();
        }
    }
}