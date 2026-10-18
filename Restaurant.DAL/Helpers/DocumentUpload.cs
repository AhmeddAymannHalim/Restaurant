using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Helpers;

public class DocumentUpload
{
    public EmployeeDocument Row { get; set; }
    public IFormFile File { get; set; }
}
