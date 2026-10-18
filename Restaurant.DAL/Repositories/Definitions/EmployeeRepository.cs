using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class EmployeeRepository : IEmployeeRepository
{
    private const string ImagesFolder = "Employee Images";

    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Employee>> GetPagedEmployeesAsync(PageRequest request)
    {
        var query = _context.Employees.AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Branch)
            .Include(a => a.Town)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search)
                                  || a.EnName.ToLower().Contains(search)
                                  || a.Code.ToLower().Contains(search)
                                  || a.PhoneNumber.Contains(search)
                                  || a.Job.ArName.ToLower().Contains(search)
                                  || a.Job.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        var codes = data.Select(a => a.Code).ToList();
        var withLogin = await _context.Users
            .AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .Select(a => a.Code)
            .ToListAsync();

        foreach (var employee in data)
        {
            employee.HasLoginAccount = withLogin.Contains(employee.Code);
        }

        await LoadImagesAsync(data);

        return PaginatedResult<Employee>.Create(data, total, request);
    }

    private async Task LoadImagesAsync(IList<Employee> employees)
    {
        var ids = employees.Select(a => a.Id).ToList();
        var images = await _context.EmployeeImages.AsNoTracking()
            .Where(a => ids.Contains(a.EmployeeId))
            .ToDictionaryAsync(a => a.EmployeeId, a => a.Url);

        foreach (var employee in employees)
        {
            employee.Image = images.GetValueOrDefault(employee.Id);
        }
    }

    private async Task SaveImageAsync(int employeeId, string url)
    {
        var row = await _context.EmployeeImages.FirstOrDefaultAsync(a => a.EmployeeId == employeeId);
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        if (row is null)
        {
            await _context.EmployeeImages.AddAsync(new EmployeeImage { EmployeeId = employeeId, Url = url });
        }
        else
        {
            row.Url = url;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(a => a.Town)
            .ThenInclude(a => a.Governorate)
            .Include(a => a.Phones)
            .Include(a => a.Dependents)
            .Include(a => a.Allowances)
            .Include(a => a.FixedDeductions)
            .Include(a => a.Leaves)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (employee is not null)
        {
            await LoadImagesAsync(new List<Employee> { employee });
        }

        return employee;
    }

    public async Task<string> NewEmployeeCodeAsync()
    {
        var codes = await _context.Employees.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertEmployeeAsync(Employee entity, IFormFile imageFile, IList<IFormFile> documentFiles)
    {
        try
        {
            var job = await _context.Jobs.FindAsync(entity.JobId);
            if (job is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            (entity.GovId, entity.CountryId) = await LocationResolver.ResolveAsync(_context, entity.TownId);
            CalculatePay(entity);
            CalculateInsurance(entity);
            CalculateLeaveDays(entity);

            if (entity.Id == 0)
            {
                await ApplyTypeValuesAsync(entity, null);
                entity.Code = await NewEmployeeCodeAsync();
                entity.CreatedAt = DateTime.Now;
                entity.Phones = BuildPhones(entity);
                EmployeeChildren.PrepareNew(entity, documentFiles);

                if (imageFile != null)
                {
                    entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                entity.Account = await AccountProvisioner.CreateAsync(
                    _context,
                    entity.ArName,
                    entity.EnName,
                    AccountType.Employee,
                    AccountCodes.EmployeesPayable);

                await _context.Employees.AddAsync(entity);
                await _context.SaveChangesAsync();
                await SaveImageAsync(entity.Id, entity.Image);

                return new ResultMessage { Success = true };
            }

            var existing = await _context.Employees
                .Include(a => a.Account)
                .Include(a => a.Phones)
                .Include(a => a.Dependents)
                .Include(a => a.Allowances)
                .Include(a => a.FixedDeductions)
                .Include(a => a.Leaves)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == entity.Id);
            if (existing is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var oldImage = await _context.EmployeeImages.AsNoTracking().Where(a => a.EmployeeId == existing.Id).Select(a => a.Url).FirstOrDefaultAsync();

            entity.Code = existing.Code;
            entity.Image = oldImage;
            entity.AccountId = existing.AccountId;
            entity.UserId = existing.UserId;
            entity.State = existing.State;
            entity.CreatedAt = existing.CreatedAt;
            entity.CreatedById = existing.CreatedById;
            entity.UpdatedAt = DateTime.Now;

            if (imageFile != null)
            {
                entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
            }

            if (existing.Account is null)
            {
                entity.Account = await AccountProvisioner.CreateAsync(
                    _context,
                    entity.ArName,
                    entity.EnName,
                    AccountType.Employee,
                    AccountCodes.EmployeesPayable);
            }
            else
            {
                existing.Account.ArName = entity.ArName;
                existing.Account.EnName = entity.EnName;
            }

            await ApplyTypeValuesAsync(entity, existing);

            var phones = BuildPhones(entity);
            entity.Phones = null;
            _context.Entry(existing).CurrentValues.SetValues(entity);
            SyncPhones(existing, phones);
            EmployeeChildren.Sync(_context, existing, entity, documentFiles);
            if (existing.Account is null && entity.Account is not null)
            {
                existing.Account = entity.Account;
            }

            await SyncLoginAccountAsync(entity, job.ArName);
            await _context.SaveChangesAsync();
            await SaveImageAsync(existing.Id, entity.Image);

            if (imageFile != null && !string.IsNullOrEmpty(oldImage))
            {
                DocumentSetting.DeleteFile(oldImage);
            }

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

    /* Allowance and deduction rows are never typed by hand: the chosen type decides the names and the amount.
       A saved row keeps its stored values while its type is unchanged, so editing a type later
       does not silently change what an employee already gets. */
    private async Task ApplyTypeValuesAsync(Employee entity, Employee existing)
    {
        var allowanceTypes = await _context.AllowanceTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);
        var deductionTypes = await _context.DeductionTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);

        foreach (var row in entity.Allowances ?? new List<EmployeeAllowance>())
        {
            var old = row.Id == 0 ? null : existing?.Allowances.FirstOrDefault(a => a.Id == row.Id);

            if (old != null && old.AllowanceTypeId == row.AllowanceTypeId)
            {
                row.ArName = old.ArName;
                row.EnName = old.EnName;
                row.Amount = old.Amount;
            }
            else if (row.AllowanceTypeId.HasValue && allowanceTypes.TryGetValue(row.AllowanceTypeId.Value, out var type))
            {
                row.ArName = type.ArName;
                row.EnName = type.EnName;
                row.Amount = type.Amount ?? 0;
            }
            else
            {
                row.AllowanceTypeId = old?.AllowanceTypeId;
                row.ArName = old?.ArName;
                row.EnName = old?.EnName;
                row.Amount = old?.Amount ?? 0;
            }
        }

        foreach (var row in entity.FixedDeductions ?? new List<EmployeeFixedDeduction>())
        {
            var old = row.Id == 0 ? null : existing?.FixedDeductions.FirstOrDefault(a => a.Id == row.Id);

            if (old != null && old.DeductionTypeId == row.DeductionTypeId)
            {
                row.ArName = old.ArName;
                row.EnName = old.EnName;
                row.Amount = old.Amount;
            }
            else if (row.DeductionTypeId.HasValue && deductionTypes.TryGetValue(row.DeductionTypeId.Value, out var type))
            {
                row.ArName = type.ArName;
                row.EnName = type.EnName;
                row.Amount = type.Amount ?? 0;
            }
            else
            {
                row.DeductionTypeId = old?.DeductionTypeId;
                row.ArName = old?.ArName;
                row.EnName = old?.EnName;
                row.Amount = old?.Amount ?? 0;
            }
        }
    }

    private static void CalculatePay(Employee employee)
    {
        var weeksPerMonth = 52m / 12m;
        var workDays = employee.WorkDays ?? 0;
        var dailyHours = employee.DailyHours ?? 0;
        var salary = employee.BasicSalary ?? 0;

        employee.WeeklyHours = Math.Round(workDays * dailyHours, 2);
        employee.MonthlyHours = Math.Round(employee.WeeklyHours.Value * weeksPerMonth, 2);

        var monthlyDays = workDays * weeksPerMonth;
        employee.DailyRate = monthlyDays > 0 ? Math.Round(salary / monthlyDays, 2) : null;
        employee.HourlyRate = employee.DailyRate.HasValue && dailyHours > 0 ? Math.Round(employee.DailyRate.Value / dailyHours, 2) : null;
        employee.MinuteRate = employee.HourlyRate.HasValue ? Math.Round(employee.HourlyRate.Value / 60m, 4) : null;
    }

    private static void CalculateInsurance(Employee employee)
    {
        var wage = (employee.InsuranceBasicSalary ?? 0) + (employee.InsuranceAllowancesAmount ?? 0);

        employee.EmployeeInsuranceAmount = employee.EmployeeInsurancePercent.HasValue
            ? Math.Round(wage * employee.EmployeeInsurancePercent.Value / 100m, 2)
            : null;
        employee.CompanyInsuranceAmount = employee.CompanyInsurancePercent.HasValue
            ? Math.Round(wage * employee.CompanyInsurancePercent.Value / 100m, 2)
            : null;
    }

    private static void CalculateLeaveDays(Employee employee)
    {
        foreach (var leave in employee.Leaves ?? new List<EmployeeLeave>())
        {
            if (leave.FromDate != default && leave.ToDate >= leave.FromDate)
            {
                leave.Days = (leave.ToDate.Date - leave.FromDate.Date).Days + 1;
            }
        }
    }

    private static List<EmployeePhone> BuildPhones(Employee entity)
    {
        var numbers = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.PhoneNumber))
        {
            numbers.Add(entity.PhoneNumber.Trim());
        }

        numbers.AddRange((entity.Phones ?? new List<EmployeePhone>())
            .Select(a => a.PhoneNumber?.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a)));

        return numbers.Distinct().Select(a => new EmployeePhone { PhoneNumber = a, CreatedAt = DateTime.Now }).ToList();
    }

    private void SyncPhones(Employee existing, List<EmployeePhone> desired)
    {
        var wanted = desired.Select(a => a.PhoneNumber).ToHashSet();

        foreach (var phone in existing.Phones.Where(a => !wanted.Contains(a.PhoneNumber)).ToList())
        {
            _context.EmployeePhones.Remove(phone);
        }

        var kept = existing.Phones.Select(a => a.PhoneNumber).ToHashSet();
        foreach (var phone in desired.Where(a => !kept.Contains(a.PhoneNumber)))
        {
            existing.Phones.Add(phone);
        }
    }

    private async Task SyncLoginAccountAsync(Employee employee, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(a => a.Code == employee.Code);
        if (user is null)
        {
            return;
        }

        var oldRoleName = user.UserType;

        user.FirstName = employee.ArName;
        user.LastName = string.Empty;
        user.TownId = employee.TownId;
        user.Street = employee.Address;
        user.PhoneNumber = employee.PhoneNumber;
        user.UserType = roleName;

        if (oldRoleName == roleName)
        {
            return;
        }

        var oldRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == oldRoleName);
        if (oldRole != null)
        {
            var oldLinks = await _context.UserRoles
                .Where(a => a.UserId == user.Id && a.RoleId == oldRole.Id)
                .ToListAsync();
            _context.UserRoles.RemoveRange(oldLinks);
        }

        var newRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == roleName);
        if (newRole is null)
        {
            newRole = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            await _context.Roles.AddAsync(newRole);
        }

        var alreadyInRole = await _context.UserRoles.AnyAsync(a => a.UserId == user.Id && a.RoleId == newRole.Id);
        if (!alreadyInRole)
        {
            await _context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newRole.Id
            });
        }
    }

    public async Task<ResultMessage> DeleteEmployeeAsync(int id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(a => a.Account)
                .Include(a => a.Phones)
                .Include(a => a.Dependents)
                .Include(a => a.Allowances)
                .Include(a => a.FixedDeductions)
                .Include(a => a.Leaves)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (employee is null)
            {
                return new ResultMessage { Success = true };
            }

            var image = await _context.EmployeeImages.AsNoTracking().Where(a => a.EmployeeId == employee.Id).Select(a => a.Url).FirstOrDefaultAsync();
            _context.EmployeePhones.RemoveRange(employee.Phones);
            EmployeeChildren.Remove(_context, employee);
            _context.Employees.Remove(employee);
            if (employee.Account is not null)
            {
                _context.Accounts.Remove(employee.Account);
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(image))
            {
                DocumentSetting.DeleteFile(image);
            }

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
