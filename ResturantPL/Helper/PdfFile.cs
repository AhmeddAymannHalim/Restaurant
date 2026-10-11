namespace ResturantPL.Helper
{
    public static class PdfFile
    {
        // Content-Disposition headers must be ASCII, so Arabic names are percent-encoded.
        public static string Name(string fileName)
            => Uri.EscapeDataString(string.IsNullOrWhiteSpace(fileName) ? "report.pdf" : fileName);
    }
}
