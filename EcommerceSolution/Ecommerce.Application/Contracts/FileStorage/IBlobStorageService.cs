using Ecommerce.Application.Models.ImageManagement;

namespace Ecommerce.Application.Contracts.FileStorage
{
    public interface IBlobStorageService
    {
        Task<ImageResponse> UploadImageAzureAsync(ImageData imageStream);
        Task<ImageResponse> UpdateImageAzureAsync(string publicIdViejo, ImageData nuevaImagen);
        Task<bool> DeleteImageAzureAsync(string publicId);
    }
}
