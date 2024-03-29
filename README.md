# NK-Portfolio-BE
This is the backend for a portfolio website, built with ASP.NET Core and Azure services. It's designed to serve data to a frontend application, and includes features like data caching and secure access to Azure Blob Storage.

## Features
- Fetches data from an Azure Cosmos DB database.
- Generates Shared Access Signature (SAS) tokens for secure access to Azure Blob Storage.
- Caches data on the client side to reduce load on the server and improve performance.
- Logs important information for debugging and monitoring.

## Tech Stack
- ASP.NET Core
- Azure Cosmos DB
- Azure Blob Storage

## Code Overview
### Controllers
The `PageDataController` is the main controller in the application. It has two endpoints:
- `GET /PageData/{pageId}`: Fetches data for a specific page from the database.
- `GET /PageData/All`: Fetches all data and caches it on the client side.

### Models
The `PageData`, `ContainerData`, and `CarouselCard` classes define the structure of the data in the database.

### SAS Token Generation
The `GenerateSasTokenForContainer` method in `PageDataController` generates a SAS token for a given blob container. This token allows the client to securely access the blobs in the container.

## Future Work
This project is a work in progress. Future updates will include:
- Adding error handling.
- Moving the SAS generator to a new singleton class that can revoke old keys.
- Adding an API call to refresh the SAS token and expire the previous one.

## Note
The `launchSettings.json` file is omitted from the repository to avoid uploading sensitive data to GitHub.  You can view the sample launchSettings if you need to. 
