using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Services.Implementations
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LocalStorageService> _logger;

        public LocalStorageService(IWebHostEnvironment env, ILogger<LocalStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File is null or empty");
                return null;
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning($"File extension {extension} is not allowed");
                return null;
            }

            // Kiểm tra kích thước file (ví dụ: max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                _logger.LogWarning($"File size {file.Length} exceeds limit");
                return null;
            }

            var uniqueName = Guid.NewGuid().ToString() + extension;
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", folderName);
            var filePath = Path.Combine(folderPath, uniqueName);

            _logger.LogInformation($"Attempting to save file: {file.FileName} -> {filePath}");

            try
            {
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    _logger.LogInformation($"Created directory: {folderPath}");
                }

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Kiểm tra file đã được tạo thành công
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    _logger.LogInformation($"File saved successfully: {filePath}, Size: {fileInfo.Length} bytes");

                    // Trả về đường dẫn tương đối
                    var relativePath = $"/uploads/{folderName}/{uniqueName}";
                    return relativePath;
                }
                else
                {
                    _logger.LogError($"File was not created: {filePath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file: {file.FileName} to {filePath}");
                return null;
            }
        }

        public bool DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;

            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Đã xóa file: {Path}", relativePath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("File không tồn tại: {Path}", relativePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa file: {Path}", relativePath);
                return false;
            }
        }
    }
}
