using Microsoft.AspNetCore.Http;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Helpers;

namespace Restaurant.DAL.Repositories.Definitions;

public class EmployeeChildrenRepository : IEmployeeChildrenRepository
{
    private const string DocumentsFolder = "Employee Documents";

    private readonly ApplicationDbContext _context;

    public EmployeeChildrenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // a new employee: keep only the filled rows, every row is a new record
    public void PrepareNew(Employee entity, IList<IFormFile> documentFiles)
    {
        var now = DateTime.Now;

        entity.Dependents = (entity.Dependents ?? new List<EmployeeDependent>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName))
            .ToList();
        foreach (var row in entity.Dependents)
        {
            row.Id = 0;
            row.CreatedAt = now;
        }

        entity.Allowances = (entity.Allowances ?? new List<EmployeeAllowance>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName) || a.Amount != 0)
            .ToList();
        foreach (var row in entity.Allowances)
        {
            row.Id = 0;
            row.CreatedAt = now;
        }

        entity.FixedDeductions = (entity.FixedDeductions ?? new List<EmployeeFixedDeduction>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName) || a.Amount != 0)
            .ToList();
        foreach (var row in entity.FixedDeductions)
        {
            row.Id = 0;
            row.CreatedAt = now;
        }

        entity.Leaves = (entity.Leaves ?? new List<EmployeeLeave>())
            .Where(a => a.FromDate != default || a.ToDate != default || a.Days != 0 || !string.IsNullOrWhiteSpace(a.Notes))
            .ToList();
        foreach (var row in entity.Leaves)
        {
            row.Id = 0;
            row.CreatedAt = now;
        }

        // the file number i belongs to the document row number i
        var documents = new List<EmployeeDocument>();
        var postedDocuments = (entity.Documents ?? new List<EmployeeDocument>()).ToList();
        for (var i = 0; i < postedDocuments.Count; i++)
        {
            var row = postedDocuments[i];
            var file = documentFiles != null && i < documentFiles.Count ? documentFiles[i] : null;
            if (string.IsNullOrWhiteSpace(row.Name) && file == null && string.IsNullOrWhiteSpace(row.Url))
            {
                continue;
            }

            row.Id = 0;
            row.CreatedAt = now;
            if (file != null)
            {
                row.Url = DocumentSetting.UploadFile(file, DocumentsFolder);
            }

            documents.Add(row);
        }

        entity.Documents = documents;
    }

    // an existing employee: delete the removed rows, update the old rows, add the new rows
    public void Sync(Employee existing, Employee incoming, IList<IFormFile> documentFiles)
    {
        var now = DateTime.Now;

        // dependents
        var dependents = (incoming.Dependents ?? new List<EmployeeDependent>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName))
            .ToList();
        foreach (var saved in existing.Dependents.ToList())
        {
            if (!dependents.Any(a => a.Id > 0 && a.Id == saved.Id))
            {
                _context.Remove(saved);
            }
        }
        foreach (var row in dependents)
        {
            var saved = existing.Dependents.FirstOrDefault(a => row.Id > 0 && a.Id == row.Id);
            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = now;
                existing.Dependents.Add(row);
                continue;
            }

            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Relation = row.Relation;
            saved.BirthDate = row.BirthDate;
            saved.UpdatedAt = now;
        }

        // allowances
        var allowances = (incoming.Allowances ?? new List<EmployeeAllowance>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName) || a.Amount != 0)
            .ToList();
        foreach (var saved in existing.Allowances.ToList())
        {
            if (!allowances.Any(a => a.Id > 0 && a.Id == saved.Id))
            {
                _context.Remove(saved);
            }
        }
        foreach (var row in allowances)
        {
            var saved = existing.Allowances.FirstOrDefault(a => row.Id > 0 && a.Id == row.Id);
            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = now;
                existing.Allowances.Add(row);
                continue;
            }

            saved.AllowanceTypeId = row.AllowanceTypeId;
            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Amount = row.Amount;
            saved.UpdatedAt = now;
        }

        // fixed deductions
        var deductions = (incoming.FixedDeductions ?? new List<EmployeeFixedDeduction>())
            .Where(a => !string.IsNullOrWhiteSpace(a.ArName) || !string.IsNullOrWhiteSpace(a.EnName) || a.Amount != 0)
            .ToList();
        foreach (var saved in existing.FixedDeductions.ToList())
        {
            if (!deductions.Any(a => a.Id > 0 && a.Id == saved.Id))
            {
                _context.Remove(saved);
            }
        }
        foreach (var row in deductions)
        {
            var saved = existing.FixedDeductions.FirstOrDefault(a => row.Id > 0 && a.Id == row.Id);
            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = now;
                existing.FixedDeductions.Add(row);
                continue;
            }

            saved.DeductionTypeId = row.DeductionTypeId;
            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Amount = row.Amount;
            saved.UpdatedAt = now;
        }

        // leaves
        var leaves = (incoming.Leaves ?? new List<EmployeeLeave>())
            .Where(a => a.FromDate != default || a.ToDate != default || a.Days != 0 || !string.IsNullOrWhiteSpace(a.Notes))
            .ToList();
        foreach (var saved in existing.Leaves.ToList())
        {
            if (!leaves.Any(a => a.Id > 0 && a.Id == saved.Id))
            {
                _context.Remove(saved);
            }
        }
        foreach (var row in leaves)
        {
            var saved = existing.Leaves.FirstOrDefault(a => row.Id > 0 && a.Id == row.Id);
            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = now;
                existing.Leaves.Add(row);
                continue;
            }

            saved.LeaveTypeId = row.LeaveTypeId;
            saved.FromDate = row.FromDate;
            saved.ToDate = row.ToDate;
            saved.Days = row.Days;
            saved.Notes = row.Notes;
            saved.UpdatedAt = now;
        }

        // documents: the file number i belongs to the document row number i
        var postedDocuments = (incoming.Documents ?? new List<EmployeeDocument>()).ToList();
        var keptDocuments = new List<EmployeeDocument>();
        var keptFiles = new List<IFormFile>();
        for (var i = 0; i < postedDocuments.Count; i++)
        {
            var file = documentFiles != null && i < documentFiles.Count ? documentFiles[i] : null;
            if (string.IsNullOrWhiteSpace(postedDocuments[i].Name) && file == null && string.IsNullOrWhiteSpace(postedDocuments[i].Url))
            {
                continue;
            }

            keptDocuments.Add(postedDocuments[i]);
            keptFiles.Add(file);
        }

        foreach (var saved in existing.Documents.ToList())
        {
            if (keptDocuments.Any(a => a.Id > 0 && a.Id == saved.Id))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(saved.Url))
            {
                DocumentSetting.DeleteFile(saved.Url);
            }

            _context.Remove(saved);
        }

        for (var i = 0; i < keptDocuments.Count; i++)
        {
            var row = keptDocuments[i];
            var file = keptFiles[i];
            var saved = existing.Documents.FirstOrDefault(a => row.Id > 0 && a.Id == row.Id);

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = now;
                if (file != null)
                {
                    row.Url = DocumentSetting.UploadFile(file, DocumentsFolder);
                }

                existing.Documents.Add(row);
                continue;
            }

            saved.Name = row.Name;
            saved.UpdatedAt = now;
            if (file != null)
            {
                if (!string.IsNullOrEmpty(saved.Url))
                {
                    DocumentSetting.DeleteFile(saved.Url);
                }

                saved.Url = DocumentSetting.UploadFile(file, DocumentsFolder);
            }
        }
    }

    public void Remove(Employee employee)
    {
        foreach (var document in employee.Documents)
        {
            if (!string.IsNullOrEmpty(document.Url))
            {
                DocumentSetting.DeleteFile(document.Url);
            }
        }

        _context.EmployeeDependents.RemoveRange(employee.Dependents);
        _context.EmployeeAllowances.RemoveRange(employee.Allowances);
        _context.EmployeeFixedDeductions.RemoveRange(employee.FixedDeductions);
        _context.EmployeeLeaves.RemoveRange(employee.Leaves);
        _context.EmployeeDocuments.RemoveRange(employee.Documents);
    }
}
