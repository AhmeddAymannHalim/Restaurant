using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResturantPL.Helper;
using Rotativa.AspNetCore;
using Shared.ViewModels.Commons;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Staff")]
    public class TableExportController : Controller
    {
        private const int MaxColumns = 40;
        private const int MaxRows = 5000;
        private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        [HttpPost]
        public IActionResult Excel([FromBody] TableExportRequest request)
        {
            if (!IsValid(request))
            {
                return BadRequest();
            }

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(SheetName(request.Title));
            sheet.RightToLeft = request.IsRightToLeft;

            for (var column = 0; column < request.Headings.Count; column++)
            {
                var cell = sheet.Cell(1, column + 1);
                cell.Value = request.Headings[column];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#112233");
                cell.Style.Font.FontColor = XLColor.White;
            }

            for (var row = 0; row < request.Rows.Count; row++)
            {
                for (var column = 0; column < request.Headings.Count && column < request.Rows[row].Count; column++)
                {
                    WriteCell(sheet.Cell(row + 2, column + 1), request.Rows[row][column]);
                }
            }

            sheet.Columns().AdjustToContents();
            sheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(), ExcelType, FileName(request, ".xlsx"));
        }

        [HttpPost]
        public IActionResult Pdf([FromBody] TableExportRequest request)
        {
            if (!IsValid(request))
            {
                return BadRequest();
            }

            return new ViewAsPdf("Pdf", request)
            {
                FileName = PdfFile.Name(FileName(request, ".pdf")),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        private static bool IsValid(TableExportRequest request)
        {
            return request is not null
                && request.Headings.Count is > 0 and <= MaxColumns
                && request.Rows.Count <= MaxRows;
        }

        private static void WriteCell(IXLCell cell, string text)
        {
            var plain = (text ?? string.Empty).Replace(",", string.Empty).Trim();
            var isNumber = decimal.TryParse(plain, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                && !Regex.IsMatch(plain, @"^0\d");

            if (isNumber)
            {
                cell.Value = number;
            }
            else
            {
                cell.Value = text ?? string.Empty;
            }
        }

        private static string SheetName(string title)
        {
            var name = new string((title ?? "Sheet").Where(c => !"[]:*?/\\".Contains(c)).ToArray());

            return string.IsNullOrWhiteSpace(name) ? "Sheet" : name[..Math.Min(name.Length, 31)];
        }

        private static string FileName(TableExportRequest request, string extension)
        {
            var name = string.IsNullOrWhiteSpace(request.FileName) ? request.Title : request.FileName;

            return $"{name}-{DateTime.Now:yyyyMMdd-HHmm}{extension}";
        }
    }
}
