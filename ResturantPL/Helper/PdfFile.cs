namespace ResturantPL.Helper
{
    public static class PdfFile
    {
        public static string Name(string fileName)
            => Uri.EscapeDataString(string.IsNullOrWhiteSpace(fileName) ? "report.pdf" : fileName);
    }
}
