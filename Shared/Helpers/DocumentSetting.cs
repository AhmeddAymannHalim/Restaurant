namespace Shared.Helpers
{
    public static class DocumentSetting
    {
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
