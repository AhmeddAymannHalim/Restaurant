namespace Restaurant.DAL.Models;

// the ERP Employee has no image column, so the restaurant keeps the photo here
public class EmployeeImage
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Url { get; set; }

    public virtual Employee Employee { get; set; }
}
