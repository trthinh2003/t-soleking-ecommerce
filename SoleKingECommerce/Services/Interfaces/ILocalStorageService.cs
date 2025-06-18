namespace SoleKingECommerce.Services.Interfaces
{
    public interface ILocalStorageService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folderName);
        bool DeleteFile(string relativePath);
    }
}
