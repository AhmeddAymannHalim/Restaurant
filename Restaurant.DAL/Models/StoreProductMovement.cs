using System;
using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class StoreProductMovement
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int ProductId { get; set; }

        public decimal QuantityDelta { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.Now;

        public StockMovementSource Source { get; set; }

        public string ReferenceCode { get; set; }

        public virtual Store Store { get; set; }

        public virtual Product Product { get; set; }
    }
}
