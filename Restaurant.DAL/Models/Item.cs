using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Item : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Remarks { get; set; }
    public double? MinQty { get; set; }
    public bool HasProduction { get; set; }
    public bool HasExpiry { get; set; }
    public bool HasSerialInput { get; set; }
    public bool HasSerialOutput { get; set; }
    public bool HasVat { get; set; }
    public bool StopTrading { get; set; }
    public int WeightInGrams { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal DiscountPercentage { get; set; }

    public decimal WeightedAverageQuantityOnHand { get; set; }
    public bool HasWeightedAverageHistory { get; set; }
    public bool RequiresCostingRebuild { get; set; }
    public bool RequiresAverageRecalculation { get; set; }

    #region ETA
    public string EtaTaxTypeCode { get; set; }
    public string EtaTaxSubTypeCode { get; set; }
    public string EtaItemCode { get; set; }
    public EtaItemTypeEnum EtaItemType { get; set; }
    #endregion

    public int? SupplierAccountId { get; set; }
    public int? GroupId { get; set; }
    public int? BrandId { get; set; }

    public virtual Account SupplierAccount { get; set; }
    public virtual Group Group { get; set; }
    public virtual Brand Brand { get; set; }

    public virtual ICollection<ItemUnit> ItemUnits { get; set; } = new HashSet<ItemUnit>();
    public virtual ICollection<StoreItemUnit> StoreItems { get; set; } = new HashSet<StoreItemUnit>();
    public virtual ICollection<ItemLocation> ItemLocations { get; set; } = new HashSet<ItemLocation>();
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new HashSet<BillDetail>();
    public virtual ICollection<ItemBarcode> ItemBarcodes { get; set; } = new HashSet<ItemBarcode>();
    public virtual ICollection<ItemDiscountWithQuantity> ItemDiscountWithQuantities { get; set; } = new HashSet<ItemDiscountWithQuantity>();
    public virtual ICollection<ItemCostingError> CostingErrors { get; set; } = new HashSet<ItemCostingError>();
}
