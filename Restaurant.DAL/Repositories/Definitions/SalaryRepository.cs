using Restaurant.DAL.IRepositories.Commons;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class SalaryRepository : ISalaryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public SalaryRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<Salary>> GetPagedSalariesAsync(PageRequest request)
    {
        var query = _context.Salaries.AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.SalaryCode.ToLower().Contains(search)
                || a.Employee.ArName.ToLower().Contains(search)
                || a.Employee.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Salary>.Create(data, total, request);
    }

    public async Task<Salary> GetSalaryByIdAsync(int id)
    {
        return await _context.Salaries.FindAsync(id);
    }

    public async Task<string> NewSalaryCodeAsync()
    {
        var codes = await _context.Salaries.AsNoTracking().Select(a => a.SalaryCode).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<EmployeeExpenseViewModel> GetEmployeeExpensesAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == employeeId);

        if (employee is null)
        {
            return null;
        }

        var scrounges = _context.Scrounges
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date);

        var model = new EmployeeExpenseViewModel
        {
            Incentives = await scrounges.Where(a => a.State == MoneyState.Incentive).SumAsync(a => (decimal?)a.Amount) ?? 0,
            Discounts = await scrounges.Where(a => a.State == MoneyState.Discount).SumAsync(a => (decimal?)a.Amount) ?? 0,
            Depts = await scrounges.Where(a => a.State == MoneyState.Dept).SumAsync(a => (decimal?)a.Amount) ?? 0,

            Allowances = await _context.EmployeeAllowances.AsNoTracking()
                .Where(a => a.EmployeeId == employeeId).SumAsync(a => (decimal?)a.Amount) ?? 0,

            FixedDeductions = await _context.EmployeeFixedDeductions.AsNoTracking()
                .Where(a => a.EmployeeId == employeeId).SumAsync(a => (decimal?)a.Amount) ?? 0,

            MainSalary = employee.BasicSalary ?? 0,
            IsSalaryStopped = employee.IsSalaryStopped,
            SalaryType = employee.PaymentMethod.ToString()
        };
        model.TotalSalary = model.MainSalary + model.Allowances + model.Incentives
            - model.FixedDeductions - model.Discounts - model.Depts;

        return model;
    }

    public async Task<ResultMessage> UpsertSalaryAsync(Salary entity, string userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // A stopped employee gets no salary saved, new or edited
            var stopped = await _context.Employees.AsNoTracking()
                .AnyAsync(a => a.Id == entity.EmployeeId && a.IsSalaryStopped);
            if (stopped)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "SalaryStopped"
                };
            }

            if (entity.Id == 0)
            {
                entity.SalaryCode = await NewSalaryCodeAsync();
                entity.UserId = userId;
                entity.SalaryDate = DateTime.Now;
                await _context.Salaries.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Salaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.SalaryCode = existing.SalaryCode;
                entity.SalaryDate = existing.SalaryDate;
                entity.UserId = existing.UserId;
                _context.Salaries.Update(entity);
                await _context.SaveChangesAsync();
            }


            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteSalaryAsync(int id)
    {
        try
        {
            var entity = await _context.Salaries.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Salary, entity.Id);
            _context.Salaries.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
