using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BilliardManagement.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> GetDashboardAnalyticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i => i.CreatedAt >= today);
            var activeSessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(s => s.Status == Models.Enums.SessionStatus.Active);
            
            return new
            {
                TodayRevenue = invoices.Sum(i => i.TotalAmount),
                ActiveTables = activeSessions.Count(),
                TotalOrdersToday = invoices.Count()
            };
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i => i.CreatedAt >= startDate);

            return invoices.GroupBy(i => i.CreatedAt.Date)
                           .Select(g => new DailyRevenueDto
                           {
                               Date = g.Key,
                               TotalRevenue = g.Sum(i => i.TotalAmount)
                           })
                           .OrderBy(x => x.Date);
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupType)
        {
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

            if (groupType.ToLower() == "month")
            {
                return invoices.GroupBy(i => new DateTime(i.CreatedAt.Year, i.CreatedAt.Month, 1))
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
            else if (groupType.ToLower() == "year")
            {
                return invoices.GroupBy(i => new DateTime(i.CreatedAt.Year, 1, 1))
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
            else // Default to day
            {
                return invoices.GroupBy(i => i.CreatedAt.Date)
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
        }

        public async Task<byte[]> ExportRevenueToExcelAsync(DateTime startDate, DateTime endDate, string groupType)
        {
            var data = (await GetRevenueReportAsync(startDate, endDate, groupType)).ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Báo cáo Doanh thu");

                // Header Title
                worksheet.Cell(1, 1).Value = "BÁO CÁO DOANH THU QUÁN BILLIARD";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#4E73DF");
                worksheet.Range(1, 1, 1, 3).Merge();

                // Subtitle Info
                worksheet.Cell(2, 1).Value = $"Kỳ báo cáo: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy} (Phân loại: {groupType.ToUpper()})";
                worksheet.Cell(2, 1).Style.Font.Italic = true;
                worksheet.Cell(2, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
                worksheet.Range(2, 1, 2, 3).Merge();

                // Table Headers
                var headers = new string[] { "STT", "Thời Gian", "Doanh Thu (VND)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(4, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#4E73DF");
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }

                // Data Rows
                int row = 5;
                decimal total = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    total += item.TotalRevenue;

                    worksheet.Cell(row, 1).Value = i + 1;
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    var dateStr = groupType.ToLower() == "month" ? item.Date.ToString("MM/yyyy") :
                                  groupType.ToLower() == "year" ? item.Date.ToString("yyyy") :
                                  item.Date.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 2).Value = dateStr;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 3).Value = item.TotalRevenue;
                    worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    row++;
                }

                // Total Row
                worksheet.Cell(row, 1).Value = "TỔNG CỘNG";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Range(row, 1, row, 2).Merge();
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = total;
                worksheet.Cell(row, 3).Style.Font.Bold = true;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0 ₫";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 3).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E8EEF5");

                // Apply Border
                var tableRange = worksheet.Range(4, 1, row, 3);
                tableRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportRevenueToPdfAsync(DateTime startDate, DateTime endDate, string groupType)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var data = (await GetRevenueReportAsync(startDate, endDate, groupType)).ToList();
            var doc = new RevenueReportPdfDocument(data, startDate, endDate, groupType);
            return doc.GeneratePdf();
        }
    }

    public class RevenueReportPdfDocument : QuestPDF.Infrastructure.IDocument
    {
        private readonly List<DailyRevenueDto> _data;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly string _groupType;

        public RevenueReportPdfDocument(List<DailyRevenueDto> data, DateTime startDate, DateTime endDate, string groupType)
        {
            _data = data;
            _startDate = startDate;
            _endDate = endDate;
            _groupType = groupType;
        }

        public void Compose(QuestPDF.Infrastructure.IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text("BILLIARD MANAGEMENT SYSTEM").FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken3);
                    col.Item().Text("BÁO CÁO DOANH THU CHI TIẾT").FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Darken3);
                    col.Item().Text($"Thời gian: {_startDate:dd/MM/yyyy} - {_endDate:dd/MM/yyyy} | Phân loại: {_groupType.ToUpper()}").FontSize(10).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    var totalRevenue = _data.Sum(x => x.TotalRevenue);

                    // Summary Cards
                    col.Item().PaddingBottom(12).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(8).Column(c =>
                        {
                            c.Item().Text("TỔNG DOANH THU").FontSize(9).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            c.Item().Text($"{totalRevenue:N0} VNĐ").FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Green.Darken2);
                        });
                        row.ConstantItem(12);
                        row.RelativeItem().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(8).Column(c =>
                        {
                            c.Item().Text("SỐ KỲ GHI NHẬN").FontSize(9).Bold().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            c.Item().Text($"{_data.Count} kỳ").FontSize(14).Bold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                        });
                    });

                    // Data Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(QuestPDF.Helpers.Colors.Blue.Darken2).Padding(6).Text("STT").FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            header.Cell().Background(QuestPDF.Helpers.Colors.Blue.Darken2).Padding(6).Text("Thời gian").FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            header.Cell().Background(QuestPDF.Helpers.Colors.Blue.Darken2).Padding(6).AlignRight().Text("Doanh thu (VNĐ)").FontColor(QuestPDF.Helpers.Colors.White).Bold();
                        });

                        for (int i = 0; i < _data.Count; i++)
                        {
                            var item = _data[i];
                            var bg = i % 2 == 0 ? QuestPDF.Helpers.Colors.White : QuestPDF.Helpers.Colors.Grey.Lighten5;

                            var dateStr = _groupType.ToLower() == "month" ? item.Date.ToString("MM/yyyy") :
                                          _groupType.ToLower() == "year" ? item.Date.ToString("yyyy") :
                                          item.Date.ToString("dd/MM/yyyy");

                            table.Cell().Background(bg).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(6).Text((i + 1).ToString());
                            table.Cell().Background(bg).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(6).Text(dateStr);
                            table.Cell().Background(bg).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(6).AlignRight().Text($"{item.TotalRevenue:N0} ₫").Bold();
                        }
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }
    }
}
