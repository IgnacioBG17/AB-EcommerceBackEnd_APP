﻿using Azure.Storage.Blobs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.ImageManagement;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ecommerce.Infrastructure.ImageCloudinary
{
    public class ManageImageService : IManageImageService
    {
        public CloudinarySettings _cloudinarySettings { get; }
        public ManageImageService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings.Value;
        }
        public async Task<ImageResponse> UploadImage(ImageData imageStream)
        {
            var account = new Account(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);
            var uploadImage = new ImageUploadParams()
            {
                File = new FileDescription(imageStream.Nombre, imageStream.ImageStream)
            };

            var uploadResult = await cloudinary.UploadAsync(uploadImage);

            if (uploadResult.StatusCode == HttpStatusCode.OK)
            {
                return new ImageResponse
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.ToString()
                };
            }

            throw new Exception("No se pudo guardar la imagen");
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            string connectionString = "<TU_CONNECTION_STRING_AZURE>";
            string containerName = "imagenes";

            var blobClient = new BlobContainerClient(connectionString, containerName);
            await blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient(fileName);
            await blob.UploadAsync(fileStream, overwrite: true);

            return blob.Uri.ToString(); // URL pública de la imagen
        }
    }
}
