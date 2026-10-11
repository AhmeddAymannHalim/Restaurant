namespace ResturantPL.Helper
{
    public static class DocumentSetting
    {
        public static string UploadFile(IFormFile file, string FolderName)
        {
            //get folder path
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", FolderName);
            //get filename
            string FileName = Path.GetFileName(file.FileName);
            //get fie path
            string FilePath =  Path.Combine(FolderPath, FileName);//not according to view but according to file stream

            using(var FileStream =new FileStream(FilePath,FileMode.Create))
            {
                file.CopyTo(FileStream);
            }

            return Path.Combine( $"/{FolderName}",FileName); //file path according to view
        }

        public static void DeleteFile(string path)
        {
            var ActualPath = $"wwwroot{path}";//file according to current directory
            File.Delete(ActualPath);
        }
    }
}
