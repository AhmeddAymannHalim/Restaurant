using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Helpers;
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

    public void PrepareNew(Employee entity, IList<IFormFile> documentFiles)
    {
        var newDependents = new List<EmployeeDependent>();
        foreach (var row in entity.Dependents ?? new List<EmployeeDependent>())
        {
            if (IsBlank(row))
            {
                continue;
            }

            row.Id = 0;
            row.CreatedAt = DateTime.Now;
            newDependents.Add(row);
        }

        entity.Dependents = newDependents;

        var newAllowances = new List<EmployeeAllowance>();
        foreach (var row in entity.Allowances ?? new List<EmployeeAllowance>())
        {
            if (IsBlank(row))
            {
                continue;
            }

            row.Id = 0;
            row.CreatedAt = DateTime.Now;
            newAllowances.Add(row);
        }

        entity.Allowances = newAllowances;

        var newFixedDeductions = new List<EmployeeFixedDeduction>();
        foreach (var row in entity.FixedDeductions ?? new List<EmployeeFixedDeduction>())
        {
            if (IsBlank(row))
            {
                continue;
            }

            row.Id = 0;
            row.CreatedAt = DateTime.Now;
            newFixedDeductions.Add(row);
        }

        entity.FixedDeductions = newFixedDeductions;

        var newLeaves = new List<EmployeeLeave>();
        foreach (var row in entity.Leaves ?? new List<EmployeeLeave>())
        {
            if (IsBlank(row))
            {
                continue;
            }

            row.Id = 0;
            row.CreatedAt = DateTime.Now;
            newLeaves.Add(row);
        }

        entity.Leaves = newLeaves;

        var documents = new List<EmployeeDocument>();
        var incoming = new List<EmployeeDocument>(entity.Documents ?? new List<EmployeeDocument>());
        for (var i = 0; i < incoming.Count; i++)
        {
            var file = GetFile(documentFiles, i);
            if (IsBlank(incoming[i], file))
            {
                continue;
            }

            incoming[i].Id = 0;
            incoming[i].CreatedAt = DateTime.Now;
            if (file != null)
            {
                incoming[i].Url = DocumentSetting.UploadFile(file, DocumentsFolder);
            }

            documents.Add(incoming[i]);
        }

        entity.Documents = documents;
    }

    public void Sync(Employee existing, Employee incoming, IList<IFormFile> documentFiles)
    {
        SyncDependents(existing, incoming);
        SyncAllowances(existing, incoming);
        SyncFixedDeductions(existing, incoming);
        SyncLeaves(existing, incoming);

        var incomingDocuments = new List<EmployeeDocument>(incoming.Documents ?? new List<EmployeeDocument>());
        SyncDocuments(existing, incomingDocuments, documentFiles);
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

    private void SyncDependents(Employee existing, Employee incoming)
    {
        var rows = new List<EmployeeDependent>();
        foreach (var row in incoming.Dependents ?? new List<EmployeeDependent>())
        {
            if (!IsBlank(row))
            {
                rows.Add(row);
            }
        }

        var savedRows = new List<EmployeeDependent>(existing.Dependents);
        foreach (var saved in savedRows)
        {
            if (!ContainsId(rows, saved.Id))
            {
                _context.Remove(saved);
            }
        }

        foreach (var row in rows)
        {
            EmployeeDependent saved = null;
            if (row.Id > 0)
            {
                foreach (var candidate in existing.Dependents)
                {
                    if (candidate.Id == row.Id)
                    {
                        saved = candidate;
                    }
                }
            }

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                existing.Dependents.Add(row);
                continue;
            }

            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Relation = row.Relation;
            saved.BirthDate = row.BirthDate;
            saved.UpdatedAt = DateTime.Now;
        }
    }

    private void SyncAllowances(Employee existing, Employee incoming)
    {
        var rows = new List<EmployeeAllowance>();
        foreach (var row in incoming.Allowances ?? new List<EmployeeAllowance>())
        {
            if (!IsBlank(row))
            {
                rows.Add(row);
            }
        }

        var savedRows = new List<EmployeeAllowance>(existing.Allowances);
        foreach (var saved in savedRows)
        {
            if (!ContainsId(rows, saved.Id))
            {
                _context.Remove(saved);
            }
        }

        foreach (var row in rows)
        {
            EmployeeAllowance saved = null;
            if (row.Id > 0)
            {
                foreach (var candidate in existing.Allowances)
                {
                    if (candidate.Id == row.Id)
                    {
                        saved = candidate;
                    }
                }
            }

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                existing.Allowances.Add(row);
                continue;
            }

            saved.AllowanceTypeId = row.AllowanceTypeId;
            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Amount = row.Amount;
            saved.UpdatedAt = DateTime.Now;
        }
    }

    private void SyncFixedDeductions(Employee existing, Employee incoming)
    {
        var rows = new List<EmployeeFixedDeduction>();
        foreach (var row in incoming.FixedDeductions ?? new List<EmployeeFixedDeduction>())
        {
            if (!IsBlank(row))
            {
                rows.Add(row);
            }
        }

        var savedRows = new List<EmployeeFixedDeduction>(existing.FixedDeductions);
        foreach (var saved in savedRows)
        {
            if (!ContainsId(rows, saved.Id))
            {
                _context.Remove(saved);
            }
        }

        foreach (var row in rows)
        {
            EmployeeFixedDeduction saved = null;
            if (row.Id > 0)
            {
                foreach (var candidate in existing.FixedDeductions)
                {
                    if (candidate.Id == row.Id)
                    {
                        saved = candidate;
                    }
                }
            }

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                existing.FixedDeductions.Add(row);
                continue;
            }

            saved.DeductionTypeId = row.DeductionTypeId;
            saved.ArName = row.ArName;
            saved.EnName = row.EnName;
            saved.Amount = row.Amount;
            saved.UpdatedAt = DateTime.Now;
        }
    }

    private void SyncLeaves(Employee existing, Employee incoming)
    {
        var rows = new List<EmployeeLeave>();
        foreach (var row in incoming.Leaves ?? new List<EmployeeLeave>())
        {
            if (!IsBlank(row))
            {
                rows.Add(row);
            }
        }

        var savedRows = new List<EmployeeLeave>(existing.Leaves);
        foreach (var saved in savedRows)
        {
            if (!ContainsId(rows, saved.Id))
            {
                _context.Remove(saved);
            }
        }

        foreach (var row in rows)
        {
            EmployeeLeave saved = null;
            if (row.Id > 0)
            {
                foreach (var candidate in existing.Leaves)
                {
                    if (candidate.Id == row.Id)
                    {
                        saved = candidate;
                    }
                }
            }

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                existing.Leaves.Add(row);
                continue;
            }

            saved.LeaveTypeId = row.LeaveTypeId;
            saved.FromDate = row.FromDate;
            saved.ToDate = row.ToDate;
            saved.Days = row.Days;
            saved.Notes = row.Notes;
            saved.UpdatedAt = DateTime.Now;
        }
    }

    private void SyncDocuments(Employee existing, List<EmployeeDocument> incoming, IList<IFormFile> files)
    {
        var uploads = new List<DocumentUpload>();
        for (var i = 0; i < incoming.Count; i++)
        {
            var file = GetFile(files, i);
            if (!IsBlank(incoming[i], file))
            {
                uploads.Add(new DocumentUpload { Row = incoming[i], File = file });
            }
        }

        var savedDocuments = new List<EmployeeDocument>(existing.Documents);
        foreach (var saved in savedDocuments)
        {
            var stillThere = false;
            foreach (var upload in uploads)
            {
                if (upload.Row.Id > 0 && upload.Row.Id == saved.Id)
                {
                    stillThere = true;
                }
            }

            if (stillThere)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(saved.Url))
            {
                DocumentSetting.DeleteFile(saved.Url);
            }

            _context.Remove(saved);
        }

        foreach (var upload in uploads)
        {
            var row = upload.Row;
            var file = upload.File;

            EmployeeDocument saved = null;
            if (row.Id > 0)
            {
                foreach (var candidate in existing.Documents)
                {
                    if (candidate.Id == row.Id)
                    {
                        saved = candidate;
                    }
                }
            }

            if (saved == null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                if (file != null)
                {
                    row.Url = DocumentSetting.UploadFile(file, DocumentsFolder);
                }

                existing.Documents.Add(row);
                continue;
            }

            saved.Name = row.Name;
            saved.UpdatedAt = DateTime.Now;

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

    private bool ContainsId(IEnumerable<BaseEntity> rows, int id)
    {
        foreach (var row in rows)
        {
            if (row.Id > 0 && row.Id == id)
            {
                return true;
            }
        }

        return false;
    }

    private IFormFile GetFile(IList<IFormFile> files, int index)
    {
        if (files == null || index >= files.Count)
        {
            return null;
        }

        return files[index];
    }

    private bool IsBlank(EmployeeDependent row)
    {
        return string.IsNullOrWhiteSpace(row.ArName) && string.IsNullOrWhiteSpace(row.EnName);
    }

    private bool IsBlank(EmployeeAllowance row)
    {
        return string.IsNullOrWhiteSpace(row.ArName) && string.IsNullOrWhiteSpace(row.EnName) && row.Amount == 0;
    }

    private bool IsBlank(EmployeeFixedDeduction row)
    {
        return string.IsNullOrWhiteSpace(row.ArName) && string.IsNullOrWhiteSpace(row.EnName) && row.Amount == 0;
    }

    private bool IsBlank(EmployeeLeave row)
    {
        return row.FromDate == default && row.ToDate == default && row.Days == 0 && string.IsNullOrWhiteSpace(row.Notes);
    }

    private bool IsBlank(EmployeeDocument row, IFormFile file)
    {
        return string.IsNullOrWhiteSpace(row.Name) && file == null && string.IsNullOrWhiteSpace(row.Url);
    }
}
