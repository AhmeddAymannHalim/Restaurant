using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class SettingRepository : ISettingRepository
{

    private readonly ApplicationDbContext _context;
    private readonly IShiftPasswordRepository _shiftPassword;

    public SettingRepository(ApplicationDbContext context)
    {
        _context = context;
        _shiftPassword = new ShiftPasswordRepository();
    }

    public async Task<ApplicationSetting> GetApplicationSettingAsync()
    {
        return await _context.ApplicationSettings.AsNoTracking().OrderBy(a => a.Id).LastOrDefaultAsync();
    }

    public async Task<ResultMessage> UpdateApplicationSettingAsync(ApplicationSetting entity)
    {
        try
        {
            var existing = await _context.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
            entity.Password = string.IsNullOrWhiteSpace(entity.Password) ? existing?.Password : _shiftPassword.Hash(entity.Password);

            _context.ApplicationSettings.Update(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<PrinterSetting> GetPrinterSettingAsync()
    {
        return await _context.PrinterSettings.AsNoTracking().OrderBy(a => a.Id).LastOrDefaultAsync();
    }

    public async Task<ResultMessage> UpdatePrinterSettingAsync(PrinterSetting entity)
    {
        try
        {
            _context.PrinterSettings.Update(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

}
