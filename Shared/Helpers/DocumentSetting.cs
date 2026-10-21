namespace Shared.Helpers
{
    public static class DocumentSetting
    {
        public const long MaxFileBytes = 5 * 1024 * 1024;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] DocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };

        // no file is fine; a file must be small and an image, never a page or script that could run in the browser
        public static bool IsAllowedImage(IFormFile file)
        {
            if (file is null)
            {
                return true;
            }

            var extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();

            return file.Length > 0
                && file.Length <= MaxFileBytes
                && ImageExtensions.Contains(extension)
                && (file.ContentType ?? string.Empty).StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAllowedDocument(IFormFile file)
        {
            if (file is null)
            {
                return true;
            }

            var extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();

            return file.Length > 0
                && file.Length <= MaxFileBytes
                && DocumentExtensions.Contains(extension);
        }

        public static string UploadFile(IFormFile file, string FolderName)
        {
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", FolderName);
            Directory.CreateDirectory(FolderPath);

            string FileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";

            string FilePath = Path.Combine(FolderPath, FileName);

            using (var FileStream = new FileStream(FilePath, FileMode.Create))
            {
                file.CopyTo(FileStream);
            }

            return $"/{FolderName}/{FileName}";
        }

        public static void DeleteFile(string path)
        {
            var ActualPath = $"wwwroot{path}";
            if (File.Exists(ActualPath))
            {
                File.Delete(ActualPath);
            }
        }
    }
}
