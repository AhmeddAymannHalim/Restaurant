using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IHttpContextAccessor _httpContext;
    private readonly IEmployeeChildrenRepository _employeeChildren;
    private readonly IAccountProvisionRepository _accountProvision;
    private readonly ILocationRepository _location;

    public EmployeeRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
        _employeeChildren = new EmployeeChildrenRepository(context);
        _accountProvision = new AccountProvisionRepository(context);
        _location = new LocationRepository(context);
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

        var pagedIds = data.Select(a => a.Id).ToList();
        var pagedImages = await _context.EmployeeImages.AsNoTracking()
            .Where(a => pagedIds.Contains(a.EmployeeId))
            .ToDictionaryAsync(a => a.EmployeeId, a => a.Url);

        foreach (var employee in data)
        {
            employee.Image = pagedImages.GetValueOrDefault(employee.Id);
        }

        return PaginatedResult<Employee>.Create(data, total, request);
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
            var image = await _context.EmployeeImages.AsNoTracking()
                .Where(a => a.EmployeeId == employee.Id)
                .Select(a => a.Url)
                .FirstOrDefaultAsync();
            employee.Image = image;
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

            var location = await _location.ResolveAsync(entity.TownId);
            entity.GovId = location.GovId;
            entity.CountryId = location.CountryId;

            var weeksPerMonth = 52m / 12m;
            var workDays = entity.WorkDays ?? 0;
            var dailyHours = entity.DailyHours ?? 0;
            var salary = entity.BasicSalary ?? 0;

            entity.WeeklyHours = Math.Round(workDays * dailyHours, 2);
            entity.MonthlyHours = Math.Round(entity.WeeklyHours.Value * weeksPerMonth, 2);

            var monthlyDays = workDays * weeksPerMonth;
            entity.DailyRate = monthlyDays > 0 ? Math.Round(salary / monthlyDays, 2) : null;
            entity.HourlyRate = entity.DailyRate.HasValue && dailyHours > 0 ? Math.Round(entity.DailyRate.Value / dailyHours, 2) : null;
            entity.MinuteRate = entity.HourlyRate.HasValue ? Math.Round(entity.HourlyRate.Value / 60m, 4) : null;

            var insuranceWage = (entity.InsuranceBasicSalary ?? 0) + (entity.InsuranceAllowancesAmount ?? 0);
            entity.EmployeeInsuranceAmount = entity.EmployeeInsurancePercent.HasValue
                ? Math.Round(insuranceWage * entity.EmployeeInsurancePercent.Value / 100m, 2)
                : null;
            entity.CompanyInsuranceAmount = entity.CompanyInsurancePercent.HasValue
                ? Math.Round(insuranceWage * entity.CompanyInsurancePercent.Value / 100m, 2)
                : null;

            foreach (var leave in entity.Leaves ?? new List<EmployeeLeave>())
            {
                if (leave.FromDate != default && leave.ToDate >= leave.FromDate)
                {
                    leave.Days = (leave.ToDate.Date - leave.FromDate.Date).Days + 1;
                }
            }

            if (entity.Id == 0)
            {
                // Allowance and deduction rows are never typed by hand: the chosen type decides the names and the amount.
                var newAllowanceTypes = await _context.AllowanceTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);
                var newDeductionTypes = await _context.DeductionTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);

                foreach (var row in entity.Allowances ?? new List<EmployeeAllowance>())
                {
                    if (row.AllowanceTypeId.HasValue && newAllowanceTypes.TryGetValue(row.AllowanceTypeId.Value, out var type))
                    {
                        row.ArName = type.ArName;
                        row.EnName = type.EnName;
                        row.Amount = type.Amount ?? 0;
                    }
                    else
                    {
                        row.AllowanceTypeId = null;
                        row.ArName = null;
                        row.EnName = null;
                        row.Amount = 0;
                    }
                }

                foreach (var row in entity.FixedDeductions ?? new List<EmployeeFixedDeduction>())
                {
                    if (row.DeductionTypeId.HasValue && newDeductionTypes.TryGetValue(row.DeductionTypeId.Value, out var type))
                    {
                        row.ArName = type.ArName;
                        row.EnName = type.EnName;
                        row.Amount = type.Amount ?? 0;
                    }
                    else
                    {
                        row.DeductionTypeId = null;
                        row.ArName = null;
                        row.EnName = null;
                        row.Amount = 0;
                    }
                }

                entity.Code = await NewEmployeeCodeAsync();
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";

                var newNumbers = new List<string>();
                if (!string.IsNullOrWhiteSpace(entity.PhoneNumber))
                {
                    newNumbers.Add(entity.PhoneNumber.Trim());
                }

                newNumbers.AddRange((entity.Phones ?? new List<EmployeePhone>())
                    .Select(a => a.PhoneNumber?.Trim())
                    .Where(a => !string.IsNullOrWhiteSpace(a)));

                entity.Phones = newNumbers.Distinct().Select(a => new EmployeePhone { PhoneNumber = a, CreatedAt = DateTime.Now }).ToList();

                _employeeChildren.PrepareNew(entity, documentFiles);

                if (imageFile != null)
                {
                    entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                entity.Account = await _accountProvision.CreateAsync(entity.ArName,
                                                                     entity.EnName,
                                                                     AccountType.Employee);

                await _context.Employees.AddAsync(entity);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(entity.Image))
                {
                    var newImageRow = await _context.EmployeeImages.FirstOrDefaultAsync(a => a.EmployeeId == entity.Id);
                    if (newImageRow is null)
                    {
                        await _context.EmployeeImages.AddAsync(new EmployeeImage { EmployeeId = entity.Id, Url = entity.Image });
                    }
                    else
                    {
                        newImageRow.Url = entity.Image;
                    }

                    await _context.SaveChangesAsync();
                }

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
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";

            if (imageFile != null)
            {
                entity.Image = DocumentSetting.UploadFile(imageFile, ImagesFolder);
            }

            if (existing.Account is null)
            {
                entity.Account = await _accountProvision.CreateAsync(entity.ArName,
                    entity.EnName,
                    AccountType.Employee);
            }
            else
            {
                existing.Account.ArName = entity.ArName;
                existing.Account.EnName = entity.EnName;
            }

            var allowanceTypes = await _context.AllowanceTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);
            var deductionTypes = await _context.DeductionTypes.AsNoTracking().ToDictionaryAsync(a => a.Id);

            foreach (var row in entity.Allowances ?? new List<EmployeeAllowance>())
            {
                var oldAllowance = row.Id == 0 ? null : existing.Allowances.FirstOrDefault(a => a.Id == row.Id);

                if (oldAllowance != null && oldAllowance.AllowanceTypeId == row.AllowanceTypeId)
                {
                    row.ArName = oldAllowance.ArName;
                    row.EnName = oldAllowance.EnName;
                    row.Amount = oldAllowance.Amount;
                }
                else if (row.AllowanceTypeId.HasValue && allowanceTypes.TryGetValue(row.AllowanceTypeId.Value, out var allowanceType))
                {
                    row.ArName = allowanceType.ArName;
                    row.EnName = allowanceType.EnName;
                    row.Amount = allowanceType.Amount ?? 0;
                }
                else
                {
                    row.AllowanceTypeId = oldAllowance?.AllowanceTypeId;
                    row.ArName = oldAllowance?.ArName;
                    row.EnName = oldAllowance?.EnName;
                    row.Amount = oldAllowance?.Amount ?? 0;
                }
            }

            foreach (var row in entity.FixedDeductions ?? new List<EmployeeFixedDeduction>())
            {
                var oldDeduction = row.Id == 0 ? null : existing.FixedDeductions.FirstOrDefault(a => a.Id == row.Id);

                if (oldDeduction != null && oldDeduction.DeductionTypeId == row.DeductionTypeId)
                {
                    row.ArName = oldDeduction.ArName;
                    row.EnName = oldDeduction.EnName;
                    row.Amount = oldDeduction.Amount;
                }
                else if (row.DeductionTypeId.HasValue && deductionTypes.TryGetValue(row.DeductionTypeId.Value, out var deductionType))
                {
                    row.ArName = deductionType.ArName;
                    row.EnName = deductionType.EnName;
                    row.Amount = deductionType.Amount ?? 0;
                }
                else
                {
                    row.DeductionTypeId = oldDeduction?.DeductionTypeId;
                    row.ArName = oldDeduction?.ArName;
                    row.EnName = oldDeduction?.EnName;
                    row.Amount = oldDeduction?.Amount ?? 0;
                }
            }

            var updateNumbers = new List<string>();
            if (!string.IsNullOrWhiteSpace(entity.PhoneNumber))
            {
                updateNumbers.Add(entity.PhoneNumber.Trim());
            }

            updateNumbers.AddRange((entity.Phones ?? new List<EmployeePhone>())
                .Select(a => a.PhoneNumber?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a)));

            var phones = updateNumbers.Distinct().Select(a => new EmployeePhone { PhoneNumber = a, CreatedAt = DateTime.Now }).ToList();

            entity.Phones = null;
            _context.Entry(existing).CurrentValues.SetValues(entity);

            var wantedPhones = phones.Select(a => a.PhoneNumber).ToHashSet();

            foreach (var phone in existing.Phones.Where(a => !wantedPhones.Contains(a.PhoneNumber)).ToList())
            {
                _context.EmployeePhones.Remove(phone);
            }

            var keptPhones = existing.Phones.Select(a => a.PhoneNumber).ToHashSet();
            foreach (var phone in phones.Where(a => !keptPhones.Contains(a.PhoneNumber)))
            {
                existing.Phones.Add(phone);
            }

            _employeeChildren.Sync(existing, entity, documentFiles);
            if (existing.Account is null && entity.Account is not null)
            {
                existing.Account = entity.Account;
            }

            var loginUser = await _context.Users.FirstOrDefaultAsync(a => a.Code == entity.Code);
            if (loginUser is not null)
            {
                var oldRoleName = loginUser.UserType;

                loginUser.FirstName = entity.ArName;
                loginUser.LastName = string.Empty;
                loginUser.TownId = entity.TownId;
                loginUser.Street = entity.Address;
                loginUser.PhoneNumber = entity.PhoneNumber;
                loginUser.UserType = job.ArName;

                if (oldRoleName != job.ArName)
                {
                    var oldRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == oldRoleName);
                    if (oldRole != null)
                    {
                        var oldLinks = await _context.UserRoles
                            .Where(a => a.UserId == loginUser.Id && a.RoleId == oldRole.Id)
                            .ToListAsync();
                        _context.UserRoles.RemoveRange(oldLinks);
                    }

                    var newRole = await _context.Roles.FirstOrDefaultAsync(a => a.Name == job.ArName);
                    if (newRole is null)
                    {
                        newRole = new IdentityRole
                        {
                            Name = job.ArName,
                            NormalizedName = job.ArName.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        };
                        await _context.Roles.AddAsync(newRole);
                    }

                    var alreadyInRole = await _context.UserRoles.AnyAsync(a => a.UserId == loginUser.Id && a.RoleId == newRole.Id);
                    if (!alreadyInRole)
                    {
                        await _context.UserRoles.AddAsync(new IdentityUserRole<string>
                        {
                            UserId = loginUser.Id,
                            RoleId = newRole.Id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(entity.Image))
            {
                var existingImageRow = await _context.EmployeeImages.FirstOrDefaultAsync(a => a.EmployeeId == existing.Id);
                if (existingImageRow is null)
                {
                    await _context.EmployeeImages.AddAsync(new EmployeeImage { EmployeeId = existing.Id, Url = entity.Image });
                }
                else
                {
                    existingImageRow.Url = entity.Image;
                }

                await _context.SaveChangesAsync();
            }

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
            _employeeChildren.Remove(employee);
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
