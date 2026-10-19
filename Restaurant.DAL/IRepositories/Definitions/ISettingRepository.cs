using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISettingRepository
{
    Task<ApplicationSetting> GetApplicationSettingAsync();
    Task<ResultMessage> UpdateApplicationSettingAsync(ApplicationSetting entity);


    Task<PrinterSetting> GetPrinterSettingAsync();
    Task<ResultMessage> UpdatePrinterSettingAsync(PrinterSetting entity);
}
