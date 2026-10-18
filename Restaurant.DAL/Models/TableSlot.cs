namespace Restaurant.DAL.Models;

// A table with the answers the booking page needs for one date, time and group size. Not saved in the database.
public class TableSlot
{
    public Table Table { get; set; }
    public bool IsReserved { get; set; }
    public bool TooSmall { get; set; }
}
