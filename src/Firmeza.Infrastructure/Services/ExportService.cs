using System.Globalization;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

public class ExportService(IUnitOfWork unitOfWork) : IExportService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private static readonly CultureInfo ColCulture = new("es-CO");

    static ExportService()
    {
        ExcelPackage.License.SetNonCommercialPersonal("Firmeza");
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Exportación de Productos (Excel & PDF)

    public async Task<byte[]> ExportarProductosExcelAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var productos = (await _unitOfWork.Productos.GetAllWithDetallesAsync(filter, cancellationToken)).ToList();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Catálogo de Productos");
        ws.View.ShowGridLines = true;

        // Título del reporte
        ws.Cells["A1:G1"].Merge = true;
        ws.Cells["A1"].Value = "FIRMEZA - REPORTE DE CATÁLOGO E INVENTARIO DE PRODUCTOS";
        ws.Cells["A1"].Style.Font.Size = 14;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(37, 99, 235)); // Azul
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 28;

        // Encabezados
        string[] headers = ["Código ID", "Nombre del Producto", "Descripción", "Unidad de Medida", "Precio Unitario", "Stock Actual", "Estado"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[3, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(241, 245, 249)); // Gris claro
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.LightGray);
        }

        int row = 4;
        foreach (var p in productos)
        {
            ws.Cells[row, 1].Value = p.Id.ToString()[..8].ToUpperInvariant();
            ws.Cells[row, 2].Value = p.Nombre;
            ws.Cells[row, 3].Value = p.Descripcion;
            ws.Cells[row, 4].Value = p.UnidadMedida;
            
            ws.Cells[row, 5].Value = p.PrecioUnitario;
            ws.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";

            ws.Cells[row, 6].Value = p.StockActual;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";

            ws.Cells[row, 7].Value = p.Activo ? "Activo" : "Inactivo";

            for (int col = 1; col <= 7; col++)
            {
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(226, 232, 240));
            }
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarProductosPdfAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var productos = (await _unitOfWork.Productos.GetAllWithDetallesAsync(filter, cancellationToken)).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("FIRMEZA MATERIALES").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                                c.Item().Text("Catálogo General de Productos e Inventario").FontSize(10).FontColor(Colors.Grey.Darken1);
                            });
                            r.ConstantItem(150).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Total Registros: {productos.Count}").FontSize(8).Bold();
                            });
                        });
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(45);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1f);
                        columns.RelativeColumn(0.9f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("CÓD").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("PRODUCTO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("UNIDAD").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("PRECIO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("STOCK").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("ESTADO").Bold().FontColor(Colors.White);
                    });

                    foreach (var p in productos)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.Id.ToString()[..6].ToUpperInvariant());
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.Nombre).SemiBold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.UnidadMedida);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(p.PrecioUnitario.ToString("C0", ColCulture));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(p.StockActual.ToString("N0"));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(p.Activo ? "Activo" : "Inactivo")
                            .FontColor(p.Activo ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Exportación de Clientes (Excel & PDF)

    public async Task<byte[]> ExportarClientesExcelAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var clientes = (await _unitOfWork.Clientes.GetAllWithVentasAsync(filter, cancellationToken)).ToList();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Directorio de Clientes");
        ws.View.ShowGridLines = true;

        ws.Cells["A1:G1"].Merge = true;
        ws.Cells["A1"].Value = "FIRMEZA - DIRECTORIO Y REGISTRO DE CLIENTES";
        ws.Cells["A1"].Style.Font.Size = 14;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(15, 118, 110)); // Verde azulado
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 28;

        string[] headers = ["NIT / Documento", "Razón Social / Nombre", "Teléfono", "Email", "Dirección de Envío", "Total Compras", "Monto Facturado"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[3, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 253, 250));
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.LightGray);
        }

        int row = 4;
        foreach (var c in clientes)
        {
            ws.Cells[row, 1].Value = c.DocumentoIdentidad;
            ws.Cells[row, 2].Value = c.RazonSocial;
            ws.Cells[row, 3].Value = c.Telefono;
            ws.Cells[row, 4].Value = c.Email;
            ws.Cells[row, 5].Value = c.DireccionEnvio;
            ws.Cells[row, 6].Value = c.Ventas.Count;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";

            var totalCompras = c.Ventas.Sum(v => v.Total);
            ws.Cells[row, 7].Value = totalCompras;
            ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

            for (int col = 1; col <= 7; col++)
            {
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 251, 241));
            }
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarClientesPdfAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var clientes = (await _unitOfWork.Clientes.GetAllWithVentasAsync(filter, cancellationToken)).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("FIRMEZA - DIRECTORIO DE CLIENTES").FontSize(16).Bold().FontColor(Colors.Teal.Darken2);
                                c.Item().Text("Listado de constructoras, contratistas y clientes particulares").FontSize(10).FontColor(Colors.Grey.Darken1);
                            });
                            r.ConstantItem(160).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Total Clientes: {clientes.Count}").FontSize(8).Bold();
                            });
                        });
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(95);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("NIT / CÉDULA").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("RAZÓN SOCIAL").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("TELÉFONO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("EMAIL").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("DIRECCIÓN DE ENVÍO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Teal.Darken2).Padding(5).AlignRight().Text("TOTAL COMPRAS").Bold().FontColor(Colors.White);
                    });

                    foreach (var c in clientes)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.DocumentoIdentidad).Bold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.RazonSocial).SemiBold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.Telefono);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.Email);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.DireccionEnvio);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight()
                            .Text(c.Ventas.Sum(v => v.Total).ToString("C0", ColCulture));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Exportación de Ventas (Excel & PDF)

    public async Task<byte[]> ExportarVentasExcelAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var ventas = (await _unitOfWork.Ventas.GetAllWithDetailsAsync(cancellationToken)).ToList();

        // Aplicar filtros opcionales
        if (filter != null)
        {
            if (filter.ClienteId.HasValue)
                ventas = ventas.Where(v => v.ClienteId == filter.ClienteId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(filter.EstadoDespacho))
                ventas = ventas.Where(v => v.EstadoDespacho.Equals(filter.EstadoDespacho, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filter.FechaInicio.HasValue)
                ventas = ventas.Where(v => v.FechaVenta >= filter.FechaInicio.Value).ToList();

            if (filter.FechaFin.HasValue)
                ventas = ventas.Where(v => v.FechaVenta <= filter.FechaFin.Value).ToList();
        }

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Historial de Ventas");
        ws.View.ShowGridLines = true;

        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "FIRMEZA - HISTORIAL GENERAL DE VENTAS Y ÓRDENES DE DESPACHO";
        ws.Cells["A1"].Style.Font.Size = 14;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(67, 56, 202)); // Índigo
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 28;

        string[] headers = ["N° Recibo", "Fecha Venta", "NIT Cliente", "Cliente / Razón Social", "Ítems", "Subtotal (Base)", "IVA (19%)", "Total Venta"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[3, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(238, 242, 255));
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.LightGray);
        }

        int row = 4;
        decimal sumaTotal = 0;
        foreach (var v in ventas)
        {
            ws.Cells[row, 1].Value = $"REC-{v.Id.ToString()[..8].ToUpperInvariant()}";
            ws.Cells[row, 2].Value = v.FechaVenta.ToString("yyyy-MM-dd HH:mm");
            ws.Cells[row, 3].Value = v.Cliente?.DocumentoIdentidad ?? "";
            ws.Cells[row, 4].Value = v.Cliente?.RazonSocial ?? "Cliente General";
            ws.Cells[row, 5].Value = v.Detalles.Sum(d => d.Cantidad);
            ws.Cells[row, 5].Style.Numberformat.Format = "#,##0";

            var subtotal = Math.Round(v.Total / 1.19m, 2);
            var iva = v.Total - subtotal;

            ws.Cells[row, 6].Value = subtotal;
            ws.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";

            ws.Cells[row, 7].Value = iva;
            ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";

            ws.Cells[row, 8].Value = v.Total;
            ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";

            sumaTotal += v.Total;

            for (int col = 1; col <= 8; col++)
            {
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(224, 231, 255));
            }
            row++;
        }

        // Fila de Total Acumulado
        ws.Cells[row, 1, row, 7].Merge = true;
        ws.Cells[row, 1].Value = "TOTAL ACUMULADO:";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        ws.Cells[row, 8].Value = sumaTotal;
        ws.Cells[row, 8].Style.Font.Bold = true;
        ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";
        ws.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(254, 240, 138));

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarVentasPdfAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var ventas = (await _unitOfWork.Ventas.GetAllWithDetailsAsync(cancellationToken)).ToList();

        if (filter != null)
        {
            if (filter.ClienteId.HasValue)
                ventas = ventas.Where(v => v.ClienteId == filter.ClienteId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(filter.EstadoDespacho))
                ventas = ventas.Where(v => v.EstadoDespacho.Equals(filter.EstadoDespacho, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filter.FechaInicio.HasValue)
                ventas = ventas.Where(v => v.FechaVenta >= filter.FechaInicio.Value).ToList();

            if (filter.FechaFin.HasValue)
                ventas = ventas.Where(v => v.FechaVenta <= filter.FechaFin.Value).ToList();
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("FIRMEZA - REPORTE DE VENTAS").FontSize(16).Bold().FontColor(Colors.Indigo.Darken2);
                                c.Item().Text("Historial de órdenes y facturación comercial").FontSize(10).FontColor(Colors.Grey.Darken1);
                            });
                            r.ConstantItem(160).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Total Facturado: {ventas.Sum(v => v.Total).ToString("C0", ColCulture)}").FontSize(9).Bold().FontColor(Colors.Green.Darken2);
                            });
                        });
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(75);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.2f);
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).Text("N° RECIBO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).Text("FECHA").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).Text("CLIENTE").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("ESTADO").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("CANT").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignRight().Text("TOTAL").Bold().FontColor(Colors.White);
                    });

                    foreach (var v in ventas)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"REC-{v.Id.ToString()[..6].ToUpperInvariant()}").Bold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(v.FechaVenta.ToString("yyyy-MM-dd"));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(v.Cliente?.RazonSocial ?? "General").SemiBold();
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(v.EstadoDespacho);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(v.Detalles.Sum(d => d.Cantidad).ToString());
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(v.Total.ToString("C0", ColCulture)).Bold();
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    #region Generación y Guardado de Comprobantes / Recibos PDF (QuestPDF)

    public async Task<byte[]> GenerarComprobanteReciboPdfAsync(Guid ventaId, CancellationToken cancellationToken = default)
    {
        var venta = await _unitOfWork.Ventas.GetByIdWithDetailsAsync(ventaId, cancellationToken);
        if (venta == null)
        {
            throw new KeyNotFoundException($"No se encontró la orden de venta con identificador {ventaId}");
        }

        var cliente = venta.Cliente ?? new Cliente { RazonSocial = "Cliente General", DocumentoIdentidad = "N/A" };
        var numeroRecibo = $"REC-{venta.Id.ToString()[..8].ToUpperInvariant()}";
        var subtotalBase = Math.Round(venta.Total / 1.19m, 2);
        var ivaLiquidado = venta.Total - subtotalBase;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.8f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily("Lato"));

                // 1. Encabezado del Recibo
                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            // Logo / Datos Empresa
                            r.RelativeItem(3).Column(c =>
                            {
                                c.Item().Text("FIRMEZA S.A.S.").FontSize(18).ExtraBold().FontColor(Colors.Blue.Darken3);
                                c.Item().Text("Soluciones Integrales en Materiales de Construcción").FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                                c.Item().PaddingTop(4).Text("NIT: 901.845.320-1 | Régimen Común").FontSize(8).FontColor(Colors.Grey.Darken2);
                                c.Item().Text("Dirección: Cra 68D # 19-45 Zona Industrial, Bogotá D.C.").FontSize(8).FontColor(Colors.Grey.Darken2);
                                c.Item().Text("PBX: +57 (601) 745-8900 | info@firmeza.com").FontSize(8).FontColor(Colors.Grey.Darken2);
                            });

                            // Cuadro de Recibo / Número
                            r.RelativeItem(2).Border(1.5f).BorderColor(Colors.Blue.Darken2).Background(Colors.Blue.Lighten5).Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("RECIBO DE CAJA / DESPACHO").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                                c.Item().AlignCenter().PaddingTop(2).Text(numeroRecibo).FontSize(13).ExtraBold().FontColor(Colors.Red.Darken2);
                                c.Item().PaddingTop(6).Text($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm}").FontSize(8.5f);
                                c.Item().Text($"Estado: {venta.EstadoDespacho}").FontSize(8.5f).Bold();
                            });
                        });

                        col.Item().PaddingTop(12).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken2);
                    });
                });

                // 2. Contenido del Recibo
                page.Content().PaddingTop(15).Column(col =>
                {
                    // Bloque de Datos del Cliente
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DATOS DEL CLIENTE / RECEPTOR").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().PaddingTop(3).Text($"Razón Social: {cliente.RazonSocial}").SemiBold();
                            c.Item().Text($"NIT / Cédula: {cliente.DocumentoIdentidad}");
                            c.Item().Text($"Teléfono: {(string.IsNullOrWhiteSpace(cliente.Telefono) ? "No registrado" : cliente.Telefono)}");
                        });

                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LUGAR DE ENTREGA & CONTACTO").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().PaddingTop(3).Text($"Dirección de Envío: {(string.IsNullOrWhiteSpace(cliente.DireccionEnvio) ? "Despacho en Bodega" : cliente.DireccionEnvio)}");
                            c.Item().Text($"Email: {(string.IsNullOrWhiteSpace(cliente.Email) ? "No registrado" : cliente.Email)}");
                            c.Item().Text($"Condición de Pago: Contado / Transferencia");
                        });
                    });

                    // Tabla de Productos Comprados
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3.5f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(1.8f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text("#").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("DESCRIPCIÓN DEL MATERIAL").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text("U.M.").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text("CANT.").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignRight().Text("PRECIO UNIT.").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignRight().Text("SUBTOTAL").Bold().FontColor(Colors.White);
                        });

                        int itemIndex = 1;
                        foreach (var d in venta.Detalles)
                        {
                            var prodNombre = d.Producto?.Nombre ?? "Material General";
                            var prodUm = d.Producto?.UnidadMedida ?? "UND";
                            var itemSubtotal = d.Cantidad * d.PrecioAplicado;

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(itemIndex.ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(prodNombre).SemiBold();
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(prodUm);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(d.Cantidad.ToString("N0"));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(d.PrecioAplicado.ToString("C0", ColCulture));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(itemSubtotal.ToString("C0", ColCulture)).SemiBold();

                            itemIndex++;
                        }
                    });

                    // Desglose de Totales Financieros e IVA
                    col.Item().PaddingTop(15).Row(r =>
                    {
                        // Notas y Firmas
                        r.RelativeItem(3).Column(c =>
                        {
                            c.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(notes =>
                            {
                                notes.Item().Text("NOTAS DE DESPACHO Y GARANTÍA:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                                notes.Item().Text("- La entrega de materiales pesados se realiza a pie de obra en vehículo de carga.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                                notes.Item().Text("- Todo reclamo o avería debe reportarse dentro de las 48 horas siguientes.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                                notes.Item().Text("- Comprobante emitido electrónicamente por el sistema FIRMEZA.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });

                            c.Item().PaddingTop(25).Row(signatures =>
                            {
                                signatures.RelativeItem().Column(s =>
                                {
                                    s.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(2).AlignCenter().Text("Entregado por / Conductor").FontSize(8);
                                });
                                signatures.ConstantItem(30);
                                signatures.RelativeItem().Column(s =>
                                {
                                    s.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(2).AlignCenter().Text("Recibido a Conformidad").FontSize(8);
                                });
                            });
                        });

                        // Tabla de Totales
                        r.RelativeItem(2).PaddingLeft(15).Column(c =>
                        {
                            c.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Column(tot =>
                            {
                                tot.Item().Padding(5).Row(sub =>
                                {
                                    sub.RelativeItem().Text("Subtotal Base:").FontSize(9);
                                    sub.RelativeItem().AlignRight().Text(subtotalBase.ToString("C0", ColCulture)).FontSize(9);
                                });
                                tot.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                tot.Item().Padding(5).Row(iva =>
                                {
                                    iva.RelativeItem().Text("IVA (19%):").FontSize(9);
                                    iva.RelativeItem().AlignRight().Text(ivaLiquidado.ToString("C0", ColCulture)).FontSize(9);
                                });
                                tot.Item().Background(Colors.Blue.Darken3).Padding(8).Row(totalRow =>
                                {
                                    totalRow.RelativeItem().Text("TOTAL A PAGAR:").Bold().FontColor(Colors.White).FontSize(11);
                                    totalRow.RelativeItem().AlignRight().Text(venta.Total.ToString("C0", ColCulture)).ExtraBold().FontColor(Colors.White).FontSize(11);
                                });
                            });
                        });
                    });
                });

                // 3. Pie de página del Recibo
                page.Footer().Element(footer =>
                {
                    footer.Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Firmeza v2.0").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                            r.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<string> GuardarComprobanteReciboAsync(Guid ventaId, string wwwrootPath, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await GenerarComprobanteReciboPdfAsync(ventaId, cancellationToken);

        var recibosDir = Path.Combine(wwwrootPath, "recibos");
        if (!Directory.Exists(recibosDir))
        {
            Directory.CreateDirectory(recibosDir);
        }

        var fileName = $"recibo_{ventaId}.pdf";
        var fullPath = Path.Combine(recibosDir, fileName);

        await File.WriteAllBytesAsync(fullPath, pdfBytes, cancellationToken);
        return $"/recibos/{fileName}";
    }

    #endregion
}
