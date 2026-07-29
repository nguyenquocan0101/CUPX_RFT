using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.IO;

namespace Services.Utils;

public static class ExcelExporter
{
    /// <summary>
    /// Xuất ra file Excel dưới dạng byte[].
    /// </summary>
    public static byte[] Export<T>(
        IEnumerable<T> data,
        string sheetName = "Sheet1",
        string? title = null,
        RecyclableMemoryStreamManager? manager = null)
    {
        using var ms = manager?.GetStream("ExcelExporter.Export") ?? new MemoryStream();
        ExportToStream(data, ms, sheetName, title);
        return ms.ToArray();
    }

    /// <summary>
    /// Xuất trực tiếp vào stream (ví dụ Response.Body).
    /// </summary>
    public static void ExportToStream<T>(IEnumerable<T> data, Stream output, string sheetName = "Sheet1",
        string? title = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        // ===== Title (nếu có) đã xử lý trước đó =====
        var currentRow = 1;
        var headerRow = currentRow;
        var firstDataRow = headerRow + 1;
        var lastCol = props.Count;

        // ---------- HEADER ----------
        for (int i = 0; i < props.Count; i++)
        {
            var header = GetDisplayName(props[i]) ?? props[i].Name;
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = header;

            // Style header
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD"); // xanh đậm kiểu Office
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = XLColor.White;

            // tăng chiều cao header
            ws.Row(headerRow).Height = 22;
        }

        ws.SheetView.FreezeRows(headerRow);

        // ---------- DATA ----------
        int rowIndex = 0;
        foreach (var item in data ?? Enumerable.Empty<T>())
        {
            int r = firstDataRow + rowIndex;

            for (int c = 0; c < props.Count; c++)
            {
                var p = props[c];
                var cell = ws.Cell(r, c + 1);
                var val = p.GetValue(item, null);

                // Gán value gốc để ClosedXML hiểu kiểu (DateTime/decimal/bool...)
                if (val is null)
                {
                    cell.Value = "";
                }
                else
                {
                    cell.Value = val.ToString();

                    // Định dạng theo kiểu + DisplayFormatAttribute nếu có
                    var fmtAttr = p.GetCustomAttribute<DisplayFormatAttribute>();
                    var fmt = fmtAttr?.DataFormatString;

                    if (val is DateTime || val is DateTime?)
                    {
                        cell.Style.DateFormat.Format = fmt ?? "dd/MM/yyyy HH:mm:ss";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else if (val is decimal || val is double || val is float)
                    {
                        if (!string.IsNullOrWhiteSpace(fmt))
                            cell.Style.NumberFormat.Format = fmt!;
                        else
                            cell.Style.NumberFormat.Format = "#,##0.##";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    else if (val is bool)
                    {
                        // hiện True/False; nếu muốn text Việt, map ở DTO trước khi export
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }
                }
            }

            // Zebra striping (hàng chẵn tô màu nhạt)
            if (rowIndex % 2 == 1)
                ws.Range(r, 1, r, lastCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#F7F7F7");

            rowIndex++;
        }

        int lastDataRow = firstDataRow + Math.Max(rowIndex - 1, 0);

        // ---------- TABLE + BORDER + AUTOFILTER ----------
        if (rowIndex > 0)
        {
            var tableRange = ws.Range(headerRow, 1, lastDataRow, lastCol);
            var table = tableRange.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2; // theme đẹp sẵn
            table.ShowAutoFilter = true; // bật filter
        }

        // Viền ngoài + trong cho toàn bảng (kể cả khi table tắt)
        if (rowIndex > 0)
        {
            var rng = ws.Range(headerRow, 1, lastDataRow, lastCol);
            rng.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.OutsideBorderColor = XLColor.FromHtml("#BFBFBF");
            rng.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.InsideBorderColor = XLColor.FromHtml("#E6E6E6");
        }

        // ---------- CỘT RỘNG + WRAP ----------
        ws.Columns(1, lastCol).AdjustToContents();
        // ví dụ: ép một số cột hợp lý (đổi index cột tùy DTO của bạn)
        if (lastCol >= 3) ws.Column(3).Width = Math.Max(ws.Column(3).Width, 14); // "Tổng tiền"
        for (int c = 1; c <= lastCol; c++)
        {
            ws.Column(c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(c).Style.Alignment.WrapText = false; // muốn tự xuống dòng thì true
        }

        // ---------- PAGE SETUP (in ấn) ----------
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0); // co về vừa 1 trang ngang nếu vừa

        // ---------- LƯU ----------
        wb.SaveAs(output);
    }

    private static string? GetDisplayName(PropertyInfo p)
    {
        var display = p.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(display?.Name)) return display.Name;

        var dn = p.GetCustomAttribute<DisplayNameAttribute>();
        if (!string.IsNullOrWhiteSpace(dn?.DisplayName)) return dn.DisplayName;

        return null;
    }
}