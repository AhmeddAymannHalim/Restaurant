namespace Shared.ViewModels.Definitions;

public class EmployeeDocumentViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public IFormFile File { get; set; }
}
