using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ecommerce.Application.Contracts.FileStorage;
using Ecommerce.Application.Models.ImageManagement;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services.FileStorage
{
    public class BlobStorageService : IBlobStorageService
    {
        public AzureBlobStorageSettings _azureSettings { get; }

        public BlobStorageService(
            IOptions<AzureBlobStorageSettings> azureSettings)
        {

            _azureSettings = azureSettings.Value;
        }

        public async Task<ImageResponse> UploadImageAzureAsync(ImageData imageData)
        {

            // 1. Inicializar el cliente del contenedor
            var blobServiceClient = new BlobServiceClient(_azureSettings.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_azureSettings.ContainerName);

            // Crear el contenedor si no existe (opcional, dependiendo de tu arquitectura)
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 2. Generar un nombre único para evitar colisiones
            var fileName = $"{Guid.NewGuid()}_{imageData.Nombre}";
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            // 3. Subir el flujo de la imagen
            if (imageData.ImageStream!.CanSeek) imageData.ImageStream.Position = 0;

            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/jpeg" };

            await blobClient.UploadAsync(imageData.ImageStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeader
            });

            // 4. Retornar la respuesta con la estructura estándar
            return new ImageResponse
            {
                PublicId = fileName,
                Url = blobClient.Uri.ToString()
            };
        }
        public async Task<ImageResponse> UpdateImageAzureAsync(string publicIdViejo, ImageData nuevaImagen)
        {
            // 1. Eliminamos la imagen anterior
            await DeleteImageAzureAsync(publicIdViejo);

            // 2. Subimos la nueva utilizando el método que creamos antes
            return await UploadImageAzureAsync(nuevaImagen);
        }
        public async Task<bool> DeleteImageAzureAsync(string publicId)
        {
            var blobServiceClient = new BlobServiceClient(_azureSettings.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_azureSettings.ContainerName);
            var blobClient = blobContainerClient.GetBlobClient(publicId);

            // Elimina el blob si existe
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}
