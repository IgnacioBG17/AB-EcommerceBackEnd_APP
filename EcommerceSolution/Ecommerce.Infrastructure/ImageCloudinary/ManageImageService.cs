using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.ImageManagement;
using Microsoft.Extensions.Options;
using System.Net;
using Uploadcare;
using Uploadcare.Upload;

namespace Ecommerce.Infrastructure.ImageCloudinary
{
    public class ManageImageService : IManageImageService
    {
        public CloudinarySettings _cloudinarySettings { get; }
        public UploadcareSettings _uploadcareSettings { get; }

        private readonly UploadcareClient _uploadcareClient;

        public ManageImageService(
                                IOptions<CloudinarySettings> cloudinarySettings,
                                IOptions<UploadcareSettings> uploadcareSettings)
        {
            _cloudinarySettings = cloudinarySettings.Value;
            _uploadcareSettings = uploadcareSettings.Value;

                _uploadcareClient = new UploadcareClient(
                uploadcareSettings.Value.PublicKey,
                uploadcareSettings.Value.SecretKey,
                UploadcareAuthType.Simple 
            );
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

        public async Task<ImageResponse> UploadImageAsync(ImageData imageData)
        {   
            using var memoryStream = new MemoryStream();
            await imageData.ImageStream!.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            
            var uploader = new FileUploader(_uploadcareClient);

            // Subir archivo a Uploadcare
            var uploadedFile = await uploader.Upload(fileBytes, imageData.Nombre, store: true);

            if (uploadedFile == null || string.IsNullOrWhiteSpace(uploadedFile.Uuid))
                throw new Exception("Error al subir imagen a Uploadcare.");

            // Asegurar que el archivo quede almacenado permanentemente
            await _uploadcareClient.Files.StoreAsync(uploadedFile.Uuid);
            
            var cdnUrl = uploadedFile.CdnPath().ScaleCropCenter(834, 551).Build();

            return new ImageResponse
            {
                PublicId = uploadedFile.Uuid,
                Url = cdnUrl
            };
        }
    }
}
