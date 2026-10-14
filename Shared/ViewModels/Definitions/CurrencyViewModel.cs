namespace Shared.ViewModels.Definitions;

public class CurrencyViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string SubArName { get; set; }
    public string SubEnName { get; set; }
    public double? CurrencyRate { get; set; }
    public short? SubToCurrencyTrans { get; set; }
    public double? CurrencyFixedRate { get; set; }
}
