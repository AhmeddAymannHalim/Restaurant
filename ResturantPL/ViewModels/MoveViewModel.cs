namespace ResturantPL.ViewModels
{
    public class MoveViewModel
    {
        public int Id { get; set; }
        public int OrderMenuItemId { get; set; }
        public int OrderTableToId { get; set; }
    }
    
    public class MergeViewModel
    {
        public int Id { get; set; }
        public int OrderTableToId { get; set; }
    }
}
