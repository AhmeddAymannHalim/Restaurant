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

        var fromDay = startDate.Date;
        var nextDay = endDate.Date.AddDays(1);
        var scrounges = _context.Scrounges
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.Date >= fromDay && a.Date < nextDay);

        var incentives = await scrounges
            .Where(a => a.State == MoneyState.Incentive)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var discounts = await scrounges
            .Where(a => a.State == MoneyState.Discount)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var depts = await scrounges
            .Where(a => a.State == MoneyState.Dept)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var allowances = await _context.EmployeeAllowances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var fixedDeductions = await _context.EmployeeFixedDeductions
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId)
            .SumAsync(a => (decimal?)a.Amount) ?? 0;

        var mainSalary = employee.BasicSalary ?? 0;
        var totalSalary = mainSalary + allowances + incentives - fixedDeductions - discounts - depts;

        return new EmployeeExpenseViewModel
        {
            MainSalary = mainSalary,
            Allowances = allowances,
            Incentives = incentives,
            FixedDeductions = fixedDeductions,
            Discounts = discounts,
            Depts = depts,
            TotalSalary = totalSalary,
            IsSalaryStopped = employee.IsSalaryStopped,
            SalaryType = employee.SalaryType?.ToString() ?? string.Empty
        };
    }

    public async Task<ResultMessage> UpsertSalaryAsync(Salary entity, string userId)
    {
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

            Salary existing = null;
            if (entity.Id != 0)
            {
                existing = await _context.Salaries.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }
            }

            var periodAlreadyPaid = await _context.Salaries.AsNoTracking()
                .AnyAsync(a => a.Id != entity.Id
                    && a.EmployeeId == entity.EmployeeId
                    && a.FromDate <= entity.ToDate
                    && a.ToDate >= entity.FromDate);
            if (periodAlreadyPaid)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "SalaryPeriodAlreadyPaid"
                };
            }

            if (existing is null)
            {
                entity.SalaryCode = await NewSalaryCodeAsync();
                entity.UserId = userId;
                entity.SalaryDate = DateTime.Now;
                await _context.Salaries.AddAsync(entity);
            }
            else
            {
                entity.SalaryCode = existing.SalaryCode;
                entity.SalaryDate = existing.SalaryDate;
                entity.UserId = existing.UserId;
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();

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
