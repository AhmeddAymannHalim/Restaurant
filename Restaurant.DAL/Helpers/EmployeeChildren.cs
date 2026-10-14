using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.Helpers;

namespace Restaurant.DAL.Helpers;

public static class EmployeeChildren
{
    private const string DocumentsFolder = "Employee Documents";

    public static void PrepareNew(Employee entity, IList<IFormFile> documentFiles)
    {
        entity.Dependents = Fresh(entity.Dependents, a => !IsBlank(a));
        entity.Allowances = Fresh(entity.Allowances, a => !IsBlank(a));
        entity.FixedDeductions = Fresh(entity.FixedDeductions, a => !IsBlank(a));
        entity.Leaves = Fresh(entity.Leaves, a => !IsBlank(a));

        var documents = new List<EmployeeDocument>();
        var incoming = (entity.Documents ?? new List<EmployeeDocument>()).ToList();
        for (var i = 0; i < incoming.Count; i++)
        {
            var file = FileAt(documentFiles, i);
            if (IsBlank(incoming[i], file))
            {
                continue;
            }

            incoming[i].Id = 0;
            incoming[i].CreatedAt = DateTime.Now;
            incoming[i].Url = file is null ? incoming[i].Url : DocumentSetting.UploadFile(file, DocumentsFolder);
            documents.Add(incoming[i]);
        }

        entity.Documents = documents;
    }

    public static void Sync(ApplicationDbContext context, Employee existing, Employee incoming, IList<IFormFile> documentFiles)
    {
        SyncRows(context, existing.Dependents, incoming.Dependents, a => !IsBlank(a), (source, target) =>
        {
            target.ArName = source.ArName;
            target.EnName = source.EnName;
            target.Relation = source.Relation;
            target.BirthDate = source.BirthDate;
        });

        SyncRows(context, existing.Allowances, incoming.Allowances, a => !IsBlank(a), (source, target) =>
        {
            target.ArName = source.ArName;
            target.EnName = source.EnName;
            target.Amount = source.Amount;
        });

        SyncRows(context, existing.FixedDeductions, incoming.FixedDeductions, a => !IsBlank(a), (source, target) =>
        {
            target.ArName = source.ArName;
            target.EnName = source.EnName;
            target.Amount = source.Amount;
        });

        SyncRows(context, existing.Leaves, incoming.Leaves, a => !IsBlank(a), (source, target) =>
        {
            target.LeaveTypeId = source.LeaveTypeId;
            target.FromDate = source.FromDate;
            target.ToDate = source.ToDate;
            target.Days = source.Days;
            target.Notes = source.Notes;
        });

        SyncDocuments(context, existing, (incoming.Documents ?? new List<EmployeeDocument>()).ToList(), documentFiles);
    }

    public static void Remove(ApplicationDbContext context, Employee employee)
    {
        foreach (var document in employee.Documents)
        {
            if (!string.IsNullOrEmpty(document.Url))
            {
                DocumentSetting.DeleteFile(document.Url);
            }
        }

        context.EmployeeDependents.RemoveRange(employee.Dependents);
        context.EmployeeAllowances.RemoveRange(employee.Allowances);
        context.EmployeeFixedDeductions.RemoveRange(employee.FixedDeductions);
        context.EmployeeLeaves.RemoveRange(employee.Leaves);
        context.EmployeeDocuments.RemoveRange(employee.Documents);
    }

    private static void SyncRows<TChild>(
        ApplicationDbContext context,
        ICollection<TChild> existing,
        IEnumerable<TChild> incoming,
        Func<TChild, bool> hasData,
        Action<TChild, TChild> copy) where TChild : BaseEntity
    {
        var rows = (incoming ?? Enumerable.Empty<TChild>()).Where(hasData).ToList();
        var keptIds = rows.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

        foreach (var gone in existing.Where(a => !keptIds.Contains(a.Id)).ToList())
        {
            context.Remove(gone);
        }

        foreach (var row in rows)
        {
            var current = row.Id > 0 ? existing.FirstOrDefault(a => a.Id == row.Id) : null;
            if (current is null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                existing.Add(row);
            }
            else
            {
                copy(row, current);
                current.UpdatedAt = DateTime.Now;
            }
        }
    }

    private static void SyncDocuments(ApplicationDbContext context, Employee existing, IList<EmployeeDocument> incoming, IList<IFormFile> files)
    {
        var rows = new List<(EmployeeDocument Row, IFormFile File)>();
        for (var i = 0; i < incoming.Count; i++)
        {
            var file = FileAt(files, i);
            if (!IsBlank(incoming[i], file))
            {
                rows.Add((incoming[i], file));
            }
        }

        var keptIds = rows.Where(a => a.Row.Id > 0).Select(a => a.Row.Id).ToHashSet();
        foreach (var gone in existing.Documents.Where(a => !keptIds.Contains(a.Id)).ToList())
        {
            if (!string.IsNullOrEmpty(gone.Url))
            {
                DocumentSetting.DeleteFile(gone.Url);
            }

            context.Remove(gone);
        }

        foreach (var (row, file) in rows)
        {
            var current = row.Id > 0 ? existing.Documents.FirstOrDefault(a => a.Id == row.Id) : null;
            if (current is null)
            {
                row.Id = 0;
                row.CreatedAt = DateTime.Now;
                row.Url = file is null ? row.Url : DocumentSetting.UploadFile(file, DocumentsFolder);
                existing.Documents.Add(row);
                continue;
            }

            current.Name = row.Name;
            current.UpdatedAt = DateTime.Now;
            if (file is not null)
            {
                if (!string.IsNullOrEmpty(current.Url))
                {
                    DocumentSetting.DeleteFile(current.Url);
                }

                current.Url = DocumentSetting.UploadFile(file, DocumentsFolder);
            }
        }
    }

    private static List<TChild> Fresh<TChild>(ICollection<TChild> rows, Func<TChild, bool> hasData) where TChild : BaseEntity
    {
        return (rows ?? new List<TChild>()).Where(hasData).Select(a =>
        {
            a.Id = 0;
            a.CreatedAt = DateTime.Now;
            return a;
        }).ToList();
    }

    private static IFormFile FileAt(IList<IFormFile> files, int index)
    {
        return files is not null && index < files.Count ? files[index] : null;
    }

    private static bool IsBlank(EmployeeDependent a) => string.IsNullOrWhiteSpace(a.ArName) && string.IsNullOrWhiteSpace(a.EnName);

    private static bool IsBlank(EmployeeAllowance a) => string.IsNullOrWhiteSpace(a.ArName) && string.IsNullOrWhiteSpace(a.EnName) && a.Amount == 0;

    private static bool IsBlank(EmployeeFixedDeduction a) => string.IsNullOrWhiteSpace(a.ArName) && string.IsNullOrWhiteSpace(a.EnName) && a.Amount == 0;

    private static bool IsBlank(EmployeeLeave a) => a.FromDate == default && a.ToDate == default && a.Days == 0 && string.IsNullOrWhiteSpace(a.Notes);

    private static bool IsBlank(EmployeeDocument a, IFormFile file) => string.IsNullOrWhiteSpace(a.Name) && file is null && string.IsNullOrWhiteSpace(a.Url);
}
