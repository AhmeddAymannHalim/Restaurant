using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class BaseEntity
{
    public int Id { get; set; }
    public State? State { get; set; } = Shared.Enums.Others.State.Active;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public string DeletedById { get; set; }
}
