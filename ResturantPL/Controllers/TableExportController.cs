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
            if (request is null
                || request.Headings.Count is not (> 0 and <= MaxColumns)
                || request.Rows.Count > MaxRows)
            {
                return BadRequest();
            }

            using var workbook = new XLWorkbook();
            var sheetName = new string((request.Title ?? "Sheet").Where(c => !"[]:*?/\\".Contains(c)).ToArray());
            sheetName = string.IsNullOrWhiteSpace(sheetName) ? "Sheet" : sheetName[..Math.Min(sheetName.Length, 31)];
            var sheet = workbook.Worksheets.Add(sheetName);
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
                    var text = request.Rows[row][column];
                    var plain = (text ?? string.Empty).Replace(",", string.Empty).Trim();
                    var isNumber = decimal.TryParse(plain, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                        && !Regex.IsMatch(plain, @"^0\d");

                    // numbers stay numbers in Excel, except codes with a leading zero
                    if (isNumber)
                    {
                        sheet.Cell(row + 2, column + 1).Value = number;
                    }
                    else
                    {
                        sheet.Cell(row + 2, column + 1).Value = text ?? string.Empty;
                    }
                }
            }

            sheet.Columns().AdjustToContents();
            sheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var excelName = string.IsNullOrWhiteSpace(request.FileName) ? request.Title : request.FileName;

            return File(stream.ToArray(), ExcelType, $"{excelName}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        [HttpPost]
        public IActionResult Pdf([FromBody] TableExportRequest request)
        {
            if (request is null
                || request.Headings.Count is not (> 0 and <= MaxColumns)
                || request.Rows.Count > MaxRows)
            {
                return BadRequest();
            }

            var pdfName = string.IsNullOrWhiteSpace(request.FileName) ? request.Title : request.FileName;

            return new ViewAsPdf("Pdf", request)
            {
                FileName = Uri.EscapeDataString($"{pdfName}-{DateTime.Now:yyyyMMdd-HHmm}.pdf"),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
    }
}
