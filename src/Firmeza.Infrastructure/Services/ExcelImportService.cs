using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Firmeza.Application.DTOS.Importacion;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Firmeza.Infrastructure.Services;

public class ExcelImportService(ApplicationDbContext context, IUnitOfWork unitOfWork) : IExcelImportService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    static ExcelImportService()
    {
        // Configuración requerida de licencia para EPPlus 8+
        ExcelPackage.License.SetNonCommercialPersonal("Firmeza");
    }

    private enum CampoEntidad
    {
        Desconocido,
        // Cliente
        ClienteDocumento,
        ClienteRazonSocial,
        ClienteTelefono,
        ClienteDireccion,
        ClienteEmail,
        // Producto
        ProductoNombre,
        ProductoDescripcion,
        ProductoUnidadMedida,
        ProductoPrecioUnitario,
        ProductoStockActual,
        ProductoActivo,
        // Venta y Detalles
        VentaCantidad,
        VentaPrecioAplicado,
        VentaFecha,
        VentaEstadoDespacho,
        VentaFacturaRef
    }

    public async Task<ExcelImportResultDto> ImportarExcelDesorganizadoAsync(
        Stream excelStream,
        ExcelImportOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ExcelImportOptionsDto();
        var resultado = new ExcelImportResultDto();

        if (excelStream == null || excelStream.Length == 0)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Fila = 0,
                Columna = "N/A",
                Campo = "Archivo",
                Mensaje = "El archivo Excel proporcionado está vacío o no es válido.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Estructura"
            });
            resultado.Resumen = "Error: El archivo Excel no contiene datos.";
            return resultado;
        }

        try
        {
            using var package = new ExcelPackage(excelStream);
            var worksheets = package.Workbook.Worksheets;

            if (worksheets.Count == 0)
            {
                resultado.Errores.Add(new ExcelImportErrorDto
                {
                    Fila = 0,
                    Columna = "N/A",
                    Campo = "Hojas",
                    Mensaje = "El libro de Excel no contiene ninguna hoja de cálculo visible.",
                    Nivel = NivelInconsistencia.Error,
                    EntidadAfectada = "Estructura"
                });
                resultado.Resumen = "Error: Libro de Excel sin hojas.";
                return resultado;
            }

            // Cache en memoria para resolver rápidamente clientes y productos existentes
            var clientesExistentes = await _context.Clientes
                .ToDictionaryAsync(c => c.DocumentoIdentidad.Trim().ToLower(), c => c, cancellationToken);

            var productosExistentes = await _context.Productos
                .ToDictionaryAsync(p => p.Nombre.Trim().ToLower(), p => p, cancellationToken);

            // Estructuras temporales para agrupar ventas en memoria si comparten referencia o cliente+fecha
            var ventasEnMemoria = new List<Venta>();

            foreach (var ws in worksheets)
            {
                if (!string.IsNullOrWhiteSpace(options.NombreHoja) &&
                    !string.Equals(ws.Name, options.NombreHoja, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ws.Dimension == null || ws.Dimension.End.Row < 1)
                {
                    resultado.Errores.Add(new ExcelImportErrorDto
                    {
                        Hoja = ws.Name,
                        Fila = 0,
                        Columna = "N/A",
                        Campo = "Hoja",
                        Mensaje = $"La hoja '{ws.Name}' está vacía y fue omitida.",
                        Nivel = NivelInconsistencia.Advertencia,
                        EntidadAfectada = "Estructura"
                    });
                    continue;
                }

                resultado.TotalHojasProcesadas++;

                // 1. Detectar fila de encabezados y mapeo de columnas
                var (filaEncabezado, mapeoColumnas) = DetectarEncabezados(ws, resultado);
                if (mapeoColumnas.Count == 0)
                {
                    resultado.Errores.Add(new ExcelImportErrorDto
                    {
                        Hoja = ws.Name,
                        Fila = filaEncabezado > 0 ? filaEncabezado : 1,
                        Columna = "Todas",
                        Campo = "Encabezados",
                        Mensaje = $"No se pudieron reconocer columnas válidas de clientes, productos o ventas en la hoja '{ws.Name}'.",
                        Nivel = NivelInconsistencia.Advertencia,
                        EntidadAfectada = "Estructura"
                    });
                    continue;
                }

                // 2. Procesar filas de datos
                int filaInicio = filaEncabezado + 1;
                int filaFin = ws.Dimension.End.Row;

                for (int row = filaInicio; row <= filaFin; row++)
                {
                    if (EsFilaVacia(ws, row, mapeoColumnas))
                    {
                        continue;
                    }

                    resultado.TotalFilasLeidas++;
                    bool filaProcesadaConExito = false;

                    try
                    {
                        // Extraer valores crudos de la fila
                        var rowValues = ExtraerValoresFila(ws, row, mapeoColumnas);

                        // Normalizar y Procesar Cliente
                        Cliente? clienteFila = await ProcesarClienteFilaAsync(
                            ws.Name, row, rowValues, options, clientesExistentes, resultado);

                        // Normalizar y Procesar Producto
                        Producto? productoFila = await ProcesarProductoFilaAsync(
                            ws.Name, row, rowValues, options, productosExistentes, resultado);

                        // Normalizar y Procesar Venta / Detalle
                        if (options.CrearVentasSiHayDatos)
                        {
                            await ProcesarVentaFilaAsync(
                                ws.Name, row, rowValues, options, clienteFila, productoFila,
                                ventasEnMemoria, resultado);
                        }

                        if (clienteFila != null || productoFila != null)
                        {
                            filaProcesadaConExito = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add(new ExcelImportErrorDto
                        {
                            Hoja = ws.Name,
                            Fila = row,
                            Columna = "General",
                            Campo = "Fila",
                            ValorInvalido = null,
                            Mensaje = $"Excepción al procesar fila: {ex.Message}",
                            Nivel = NivelInconsistencia.Error,
                            EntidadAfectada = "Estructura"
                        });
                    }

                    if (filaProcesadaConExito)
                    {
                        resultado.TotalFilasProcesadasCorrectamente++;
                    }
                }
            }

            // 3. Persistir ventas y todos los cambios en la base de datos
            if (ventasEnMemoria.Count > 0)
            {
                await _context.Ventas.AddRangeAsync(ventasEnMemoria, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Generar resumen final
            GenerarResumenFinal(resultado);
        }
        catch (Exception ex)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Fila = 0,
                Columna = "N/A",
                Campo = "General",
                Mensaje = $"Error crítico durante la lectura del archivo Excel: {ex.Message}",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Estructura"
            });
            resultado.Resumen = $"Falló la importación: {ex.Message}";
        }

        return resultado;
    }

    #region Normalización de Filas y Entidades

    private async Task<Cliente?> ProcesarClienteFilaAsync(
        string hoja,
        int fila,
        Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues,
        ExcelImportOptionsDto options,
        Dictionary<string, Cliente> clientesCache,
        ExcelImportResultDto resultado)
    {
        string? docRaw = ObtenerString(rowValues, CampoEntidad.ClienteDocumento);
        string? nombreRaw = ObtenerString(rowValues, CampoEntidad.ClienteRazonSocial);
        string? telRaw = ObtenerString(rowValues, CampoEntidad.ClienteTelefono);
        string? dirRaw = ObtenerString(rowValues, CampoEntidad.ClienteDireccion);
        string? emailRaw = ObtenerString(rowValues, CampoEntidad.ClienteEmail);

        // Si la fila no contiene ninguna información de cliente, no hacemos nada
        if (string.IsNullOrWhiteSpace(docRaw) && string.IsNullOrWhiteSpace(nombreRaw) &&
            string.IsNullOrWhiteSpace(telRaw) && string.IsNullOrWhiteSpace(dirRaw) && string.IsNullOrWhiteSpace(emailRaw))
        {
            return null;
        }

        // Si no hay documento pero sí nombre, intentamos buscar si existe por nombre o creamos documento provisional
        string docNormalizado = LimpiarDocumento(docRaw);
        string nombreNormalizado = LimpiarTexto(nombreRaw);

        if (string.IsNullOrWhiteSpace(docNormalizado) && string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ClienteDocumento) ?? "Documento/Nombre",
                Campo = "Cliente",
                ValorInvalido = docRaw,
                Mensaje = "Se detectaron datos parciales de cliente pero falta tanto el Documento de Identidad (NIT/Cédula) como la Razón Social.",
                Nivel = NivelInconsistencia.Advertencia,
                EntidadAfectada = "Cliente"
            });
            return null;
        }

        // Si falta documento, usamos el nombre normalizado o generamos un ID provisional
        if (string.IsNullOrWhiteSpace(docNormalizado))
        {
            docNormalizado = "CLI-" + Math.Abs(nombreNormalizado.GetHashCode());
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ClienteDocumento) ?? "Documento",
                Campo = "DocumentoIdentidad",
                ValorInvalido = "(Vacío)",
                Mensaje = $"El cliente '{nombreNormalizado}' no tenía documento de identidad. Se le asignó el código provisional '{docNormalizado}'.",
                Nivel = NivelInconsistencia.Advertencia,
                EntidadAfectada = "Cliente"
            });
        }

        // Si falta nombre / razón social pero hay documento
        if (string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            nombreNormalizado = $"Cliente {docNormalizado}";
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ClienteRazonSocial) ?? "RazonSocial",
                Campo = "RazonSocial",
                ValorInvalido = "(Vacío)",
                Mensaje = $"No se especificó la Razón Social para el documento '{docNormalizado}'. Se autocompletó con '{nombreNormalizado}'.",
                Nivel = NivelInconsistencia.Advertencia,
                EntidadAfectada = "Cliente"
            });
        }

        // Truncar campos si superan longitud máxima
        if (docNormalizado.Length > 50) docNormalizado = docNormalizado[..50];
        if (nombreNormalizado.Length > 200) nombreNormalizado = nombreNormalizado[..200];

        string telNormalizado = LimpiarTexto(telRaw);
        if (telNormalizado.Length > 20) telNormalizado = telNormalizado[..20];

        string dirNormalizado = LimpiarTexto(dirRaw);
        if (dirNormalizado.Length > 250) dirNormalizado = dirNormalizado[..250];

        string emailNormalizado = LimpiarTexto(emailRaw).ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(emailNormalizado))
        {
            if (emailNormalizado.Length > 150) emailNormalizado = emailNormalizado[..150];
            if (!EsEmailValido(emailNormalizado))
            {
                resultado.Errores.Add(new ExcelImportErrorDto
                {
                    Hoja = hoja,
                    Fila = fila,
                    Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ClienteEmail) ?? "Email",
                    Campo = "Email",
                    ValorInvalido = emailRaw,
                    Mensaje = $"El formato de correo '{emailRaw}' no parece válido para el cliente '{nombreNormalizado}'.",
                    Nivel = NivelInconsistencia.Advertencia,
                    EntidadAfectada = "Cliente"
                });
            }
        }

        var docKey = docNormalizado.Trim().ToLowerInvariant();

        if (clientesCache.TryGetValue(docKey, out var clienteExistente))
        {
            if (options.ActualizarSiExiste)
            {
                bool huboCambios = false;
                if (!string.IsNullOrWhiteSpace(nombreNormalizado) && clienteExistente.RazonSocial != nombreNormalizado)
                {
                    clienteExistente.RazonSocial = nombreNormalizado;
                    huboCambios = true;
                }
                if (!string.IsNullOrWhiteSpace(telNormalizado) && clienteExistente.Telefono != telNormalizado)
                {
                    clienteExistente.Telefono = telNormalizado;
                    huboCambios = true;
                }
                if (!string.IsNullOrWhiteSpace(dirNormalizado) && clienteExistente.DireccionEnvio != dirNormalizado)
                {
                    clienteExistente.DireccionEnvio = dirNormalizado;
                    huboCambios = true;
                }
                if (!string.IsNullOrWhiteSpace(emailNormalizado) && clienteExistente.Email != emailNormalizado)
                {
                    clienteExistente.Email = emailNormalizado;
                    huboCambios = true;
                }

                if (huboCambios)
                {
                    if (_context.Entry(clienteExistente).State != EntityState.Added)
                    {
                        _context.Clientes.Update(clienteExistente);
                    }
                    resultado.ClientesActualizados++;
                }
                else
                {
                    resultado.ClientesOmitidos++;
                }
            }
            else
            {
                resultado.ClientesOmitidos++;
            }
            return clienteExistente;
        }
        else
        {
            var nuevoCliente = new Cliente
            {
                Id = Guid.NewGuid(),
                DocumentoIdentidad = docNormalizado,
                RazonSocial = nombreNormalizado,
                Telefono = telNormalizado,
                DireccionEnvio = dirNormalizado,
                Email = emailNormalizado
            };

            await _context.Clientes.AddAsync(nuevoCliente);
            clientesCache[docKey] = nuevoCliente;
            resultado.ClientesCreados++;
            return nuevoCliente;
        }
    }

    private async Task<Producto?> ProcesarProductoFilaAsync(
        string hoja,
        int fila,
        Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues,
        ExcelImportOptionsDto options,
        Dictionary<string, Producto> productosCache,
        ExcelImportResultDto resultado)
    {
        string? nombreRaw = ObtenerString(rowValues, CampoEntidad.ProductoNombre);
        string? descRaw = ObtenerString(rowValues, CampoEntidad.ProductoDescripcion);
        string? umRaw = ObtenerString(rowValues, CampoEntidad.ProductoUnidadMedida);
        decimal? precioRaw = ObtenerDecimal(rowValues, CampoEntidad.ProductoPrecioUnitario);
        decimal? precioVentaRaw = ObtenerDecimal(rowValues, CampoEntidad.VentaPrecioAplicado);
        int? stockRaw = ObtenerInt(rowValues, CampoEntidad.ProductoStockActual);
        bool? activoRaw = ObtenerBool(rowValues, CampoEntidad.ProductoActivo);

        // Si no hay datos de producto en la fila
        if (string.IsNullOrWhiteSpace(nombreRaw) && string.IsNullOrWhiteSpace(descRaw) &&
            !precioRaw.HasValue && !stockRaw.HasValue && string.IsNullOrWhiteSpace(umRaw))
        {
            return null;
        }

        string nombreNormalizado = LimpiarTexto(nombreRaw);

        if (string.IsNullOrWhiteSpace(nombreNormalizado))
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ProductoNombre) ?? "Nombre Producto",
                Campo = "Nombre",
                ValorInvalido = nombreRaw,
                Mensaje = "El nombre del producto es obligatorio y no puede estar vacío.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Producto"
            });
            return null;
        }

        if (nombreNormalizado.Length > 150)
        {
            nombreNormalizado = nombreNormalizado[..150];
        }

        string descNormalizada = LimpiarTexto(descRaw);
        if (descNormalizada.Length > 500) descNormalizada = descNormalizada[..500];

        string umNormalizada = LimpiarTexto(umRaw);
        if (string.IsNullOrWhiteSpace(umNormalizada)) umNormalizada = "UND";
        if (umNormalizada.Length > 50) umNormalizada = umNormalizada[..50];

        // Validar Precio
        decimal precioFinal = 0m;
        if (precioRaw.HasValue && precioRaw.Value >= 0)
        {
            precioFinal = precioRaw.Value;
        }
        else if (precioVentaRaw.HasValue && precioVentaRaw.Value >= 0)
        {
            precioFinal = precioVentaRaw.Value;
        }
        else if (precioRaw.HasValue && precioRaw.Value < 0)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ProductoPrecioUnitario) ?? "Precio",
                Campo = "PrecioUnitario",
                ValorInvalido = precioRaw.ToString(),
                Mensaje = $"El precio unitario no puede ser negativo para el producto '{nombreNormalizado}'. Se ajustó a $0.00.",
                Nivel = NivelInconsistencia.Advertencia,
                EntidadAfectada = "Producto"
            });
        }

        // Validar Stock
        int stockFinal = 0;
        if (stockRaw.HasValue)
        {
            if (stockRaw.Value < 0)
            {
                resultado.Errores.Add(new ExcelImportErrorDto
                {
                    Hoja = hoja,
                    Fila = fila,
                    Columna = ObtenerNombreColumna(rowValues, CampoEntidad.ProductoStockActual) ?? "Stock",
                    Campo = "StockActual",
                    ValorInvalido = stockRaw.ToString(),
                    Mensaje = $"El stock no puede ser negativo para '{nombreNormalizado}'. Se ajustó a 0.",
                    Nivel = NivelInconsistencia.Advertencia,
                    EntidadAfectada = "Producto"
                });
            }
            else
            {
                stockFinal = stockRaw.Value;
            }
        }

        bool activoFinal = activoRaw ?? true;
        var nameKey = nombreNormalizado.Trim().ToLowerInvariant();

        if (productosCache.TryGetValue(nameKey, out var productoExistente))
        {
            if (options.ActualizarSiExiste)
            {
                bool huboCambios = false;
                if (!string.IsNullOrWhiteSpace(descNormalizada) && productoExistente.Descripcion != descNormalizada)
                {
                    productoExistente.Descripcion = descNormalizada;
                    huboCambios = true;
                }
                if (!string.IsNullOrWhiteSpace(umNormalizada) && productoExistente.UnidadMedida != umNormalizada)
                {
                    productoExistente.UnidadMedida = umNormalizada;
                    huboCambios = true;
                }
                if (precioFinal > 0 && productoExistente.PrecioUnitario != precioFinal)
                {
                    productoExistente.PrecioUnitario = precioFinal;
                    huboCambios = true;
                }
                if (stockRaw.HasValue && productoExistente.StockActual != stockFinal)
                {
                    productoExistente.StockActual = stockFinal;
                    huboCambios = true;
                }
                if (activoRaw.HasValue && productoExistente.Activo != activoFinal)
                {
                    productoExistente.Activo = activoFinal;
                    huboCambios = true;
                }

                if (huboCambios)
                {
                    if (_context.Entry(productoExistente).State != EntityState.Added)
                    {
                        _context.Productos.Update(productoExistente);
                    }
                    resultado.ProductosActualizados++;
                }
                else
                {
                    resultado.ProductosOmitidos++;
                }
            }
            else
            {
                resultado.ProductosOmitidos++;
            }
            return productoExistente;
        }
        else
        {
            var nuevoProducto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = nombreNormalizado,
                Descripcion = descNormalizada,
                UnidadMedida = umNormalizada,
                PrecioUnitario = precioFinal,
                StockActual = stockFinal,
                Activo = activoFinal
            };

            await _context.Productos.AddAsync(nuevoProducto);
            productosCache[nameKey] = nuevoProducto;
            resultado.ProductosCreados++;
            return nuevoProducto;
        }
    }

    private Task ProcesarVentaFilaAsync(
        string hoja,
        int fila,
        Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues,
        ExcelImportOptionsDto options,
        Cliente? cliente,
        Producto? producto,
        List<Venta> ventasEnMemoria,
        ExcelImportResultDto resultado)
    {
        int? cantRaw = ObtenerInt(rowValues, CampoEntidad.VentaCantidad);
        decimal? precioAplicadoRaw = ObtenerDecimal(rowValues, CampoEntidad.VentaPrecioAplicado);
        DateTime? fechaVentaRaw = ObtenerDateTime(rowValues, CampoEntidad.VentaFecha);
        string? estadoRaw = ObtenerString(rowValues, CampoEntidad.VentaEstadoDespacho);
        string? facturaRefRaw = ObtenerString(rowValues, CampoEntidad.VentaFacturaRef);

        // Si no hay indicadores de venta en la fila, no procesamos venta
        if (!cantRaw.HasValue && !precioAplicadoRaw.HasValue && !fechaVentaRaw.HasValue && string.IsNullOrWhiteSpace(facturaRefRaw))
        {
            return Task.CompletedTask;
        }

        // Validar que exista el cliente
        if (cliente == null)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = "Venta",
                Campo = "Cliente",
                ValorInvalido = null,
                Mensaje = "Se detectó registro de venta/orden en la fila pero no se pudo asociar a ningún Cliente válido.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Venta"
            });
            return Task.CompletedTask;
        }

        // Validar que exista el producto
        if (producto == null)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = "Venta",
                Campo = "Producto",
                ValorInvalido = null,
                Mensaje = "Se detectó registro de venta/orden en la fila pero no se pudo asociar a ningún Producto válido.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Venta"
            });
            return Task.CompletedTask;
        }

        int cantidad = cantRaw ?? 1;
        if (cantidad <= 0)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.VentaCantidad) ?? "Cantidad",
                Campo = "Cantidad",
                ValorInvalido = cantRaw?.ToString(),
                Mensaje = $"La cantidad vendida ({cantidad}) debe ser mayor a 0 para el producto '{producto.Nombre}'.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Venta"
            });
            return Task.CompletedTask;
        }

        decimal precioAplicado = precioAplicadoRaw ?? (producto.PrecioUnitario > 0 ? producto.PrecioUnitario : 0m);
        if (precioAplicado < 0)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = hoja,
                Fila = fila,
                Columna = ObtenerNombreColumna(rowValues, CampoEntidad.VentaPrecioAplicado) ?? "Precio Aplicado",
                Campo = "PrecioAplicado",
                ValorInvalido = precioAplicadoRaw?.ToString(),
                Mensaje = $"El precio de venta aplicado no puede ser negativo.",
                Nivel = NivelInconsistencia.Error,
                EntidadAfectada = "Venta"
            });
            return Task.CompletedTask;
        }

        DateTime fechaVenta = fechaVentaRaw ?? DateTime.UtcNow;
        string estadoDespacho = NormalizarEstadoDespacho(estadoRaw);

        // Agrupación en memoria: buscar si ya existe una venta para el mismo cliente en la misma fecha/factura
        Venta? ventaExistente = null;
        if (!string.IsNullOrWhiteSpace(facturaRefRaw))
        {
            // Podríamos correlacionar por factura si aplica
            ventaExistente = ventasEnMemoria.FirstOrDefault(v => v.ClienteId == cliente.Id && v.FechaVenta.Date == fechaVenta.Date);
        }
        else
        {
            ventaExistente = ventasEnMemoria.FirstOrDefault(v => v.ClienteId == cliente.Id && Math.Abs((v.FechaVenta - fechaVenta).TotalMinutes) < 2);
        }

        var detalle = new VentaDetalle
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            Producto = producto,
            Cantidad = cantidad,
            PrecioAplicado = precioAplicado
        };

        if (ventaExistente != null)
        {
            detalle.VentaId = ventaExistente.Id;
            detalle.Venta = ventaExistente;
            ventaExistente.Detalles.Add(detalle);
            ventaExistente.Total += (cantidad * precioAplicado);
        }
        else
        {
            var nuevaVenta = new Venta
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Cliente = cliente,
                FechaVenta = fechaVenta,
                EstadoDespacho = estadoDespacho,
                Total = cantidad * precioAplicado
            };

            detalle.VentaId = nuevaVenta.Id;
            detalle.Venta = nuevaVenta;
            nuevaVenta.Detalles.Add(detalle);

            ventasEnMemoria.Add(nuevaVenta);
            resultado.VentasCreadas++;
        }

        // Descontar inventario si la opción está activa
        if (options.DescontarStockDeVentas)
        {
            if (producto.StockActual >= cantidad)
            {
                producto.StockActual -= cantidad;
            }
            else
            {
                resultado.Errores.Add(new ExcelImportErrorDto
                {
                    Hoja = hoja,
                    Fila = fila,
                    Columna = "Stock",
                    Campo = "StockActual",
                    ValorInvalido = $"Stock: {producto.StockActual}, Vendido: {cantidad}",
                    Mensaje = $"Inventario insuficiente para el producto '{producto.Nombre}'. Se registró la venta y el stock quedó en 0.",
                    Nivel = NivelInconsistencia.Advertencia,
                    EntidadAfectada = "Producto"
                });
                producto.StockActual = 0;
            }
            if (_context.Entry(producto).State != EntityState.Added)
            {
                _context.Productos.Update(producto);
            }
        }

        resultado.DetallesVentaCreados++;
        resultado.MontoTotalVentasImportadas += (cantidad * precioAplicado);

        return Task.CompletedTask;
    }

    #endregion

    #region Detección y Mapeo Inteligente de Columnas

    private static (int FilaEncabezado, Dictionary<int, (CampoEntidad Campo, string NombreOriginal)> Mapeo) DetectarEncabezados(
        ExcelWorksheet ws,
        ExcelImportResultDto resultado)
    {
        int maxScanRows = Math.Min(10, ws.Dimension.End.Row);
        int mejorFila = 1;
        int maxCoincidencias = 0;
        Dictionary<int, (CampoEntidad Campo, string NombreOriginal)> mejorMapeo = new();

        for (int r = 1; r <= maxScanRows; r++)
        {
            var mapeoFila = new Dictionary<int, (CampoEntidad Campo, string NombreOriginal)>();
            int coincidencias = 0;

            for (int col = 1; col <= ws.Dimension.End.Column; col++)
            {
                var valorCelda = ws.Cells[r, col].Text?.Trim();
                if (string.IsNullOrWhiteSpace(valorCelda)) continue;

                var campo = MapearNombreColumnaACampo(valorCelda);
                if (campo != CampoEntidad.Desconocido)
                {
                    mapeoFila[col] = (campo, valorCelda);
                    coincidencias++;
                }
            }

            if (coincidencias > maxCoincidencias)
            {
                maxCoincidencias = coincidencias;
                mejorFila = r;
                mejorMapeo = mapeoFila;
            }
        }

        // Si se encontraron encabezados, reportar log informativo
        if (maxCoincidencias > 0)
        {
            resultado.Errores.Add(new ExcelImportErrorDto
            {
                Hoja = ws.Name,
                Fila = mejorFila,
                Columna = "Encabezados",
                Campo = "Estructura",
                Mensaje = $"Se identificaron {maxCoincidencias} columnas mapeables en la fila {mejorFila} de la hoja '{ws.Name}'.",
                Nivel = NivelInconsistencia.Info,
                EntidadAfectada = "Estructura"
            });
        }

        return (mejorFila, mejorMapeo);
    }

    private static CampoEntidad MapearNombreColumnaACampo(string headerName)
    {
        var clean = NormalizarTextoClave(headerName);
        if (string.IsNullOrWhiteSpace(clean)) return CampoEntidad.Desconocido;

        var tokens = clean.Split('_', StringSplitOptions.RemoveEmptyEntries);

        // 1. Venta / Detalle
        if (clean.Contains("precio_venta") || clean.Contains("precio_aplicado") || clean.Contains("valor_venta") || clean.Contains("facturado"))
            return CampoEntidad.VentaPrecioAplicado;

        if (clean.Contains("cantidad") || clean.Contains("cant") || clean.Contains("unidades_vend") || clean.Contains("comprada") || clean.Contains("vendida") || tokens.Contains("cant") || tokens.Contains("unidades"))
            return CampoEntidad.VentaCantidad;

        if (clean.Contains("fecha") || clean.Contains("date"))
            return CampoEntidad.VentaFecha;

        if (clean.Contains("estado") || clean.Contains("despacho") || clean.Contains("status"))
            return CampoEntidad.VentaEstadoDespacho;

        if (clean.Contains("factura") || clean.Contains("nro_orden") || clean.Contains("referencia") || clean == "ref" || clean == "orden")
            return CampoEntidad.VentaFacturaRef;

        // 2. Cliente
        if (clean.Contains("email") || clean.Contains("correo") || clean.Contains("mail"))
            return CampoEntidad.ClienteEmail;

        if (clean.Contains("telefono") || clean.Contains("celular") || clean.Contains("movil") || clean.Contains("phone") || tokens.Contains("tel") || tokens.Contains("cel"))
            return CampoEntidad.ClienteTelefono;

        if (clean.Contains("direccion") || clean.Contains("domicilio") || clean.Contains("entrega") || clean.Contains("envio") || clean.Contains("ubicacion") || clean.Contains("address"))
            return CampoEntidad.ClienteDireccion;

        if (tokens.Contains("nit") || tokens.Contains("cedula") || tokens.Contains("cc") || tokens.Contains("rut") || tokens.Contains("ruc") || tokens.Contains("dni") || tokens.Contains("nif") ||
            clean.Contains("documento") || clean.Contains("identificacion") || clean.Contains("id_cliente") || clean.Contains("doc_identidad") || clean == "doc")
            return CampoEntidad.ClienteDocumento;

        if (clean.Contains("razon") || clean.Contains("social") || clean.Contains("empresa") || clean.Contains("comprador") || clean.Contains("titular") || clean.Contains("nombre_cliente") || clean == "cliente")
            return CampoEntidad.ClienteRazonSocial;

        // 3. Producto
        if (clean.Contains("stock") || clean.Contains("inventario") || clean.Contains("existencia") || clean.Contains("disponible"))
            return CampoEntidad.ProductoStockActual;

        if (clean.Contains("precio") || clean.Contains("valor") || clean.Contains("costo") || clean.Contains("p_unit") || clean.Contains("unit_price"))
            return CampoEntidad.ProductoPrecioUnitario;

        if (clean.Contains("unidad") || clean.Contains("medida") || clean.Contains("presentacion") || clean.Contains("u_m") || tokens.Contains("um") || tokens.Contains("unit"))
            return CampoEntidad.ProductoUnidadMedida;

        if (clean.Contains("descripcion") || clean.Contains("detalle") || clean.Contains("especificacion") || clean.Contains("observacion"))
            return CampoEntidad.ProductoDescripcion;

        if (clean.Contains("activo") || clean.Contains("habilitado") || clean.Contains("is_active"))
            return CampoEntidad.ProductoActivo;

        if (clean.Contains("producto") || clean.Contains("articulo") || clean.Contains("material") || clean.Contains("item"))
            return CampoEntidad.ProductoNombre;

        return CampoEntidad.Desconocido;
    }

    private static string NormalizarTextoClave(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c) || c == '_' || c == '-' || c == '.' || c == '/' || c == '\\' || c == '|')
                {
                    sb.Append('_');
                }
            }
        }

        return Regex.Replace(sb.ToString().Trim('_'), "_+", "_");
    }

    #endregion

    #region Utilidades de Extracción y Conversión

    private static bool EsFilaVacia(ExcelWorksheet ws, int row, Dictionary<int, (CampoEntidad Campo, string NombreOriginal)> mapeo)
    {
        foreach (var col in mapeo.Keys)
        {
            if (!string.IsNullOrWhiteSpace(ws.Cells[row, col].Text))
            {
                return false;
            }
        }
        return true;
    }

    private static Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> ExtraerValoresFila(
        ExcelWorksheet ws,
        int row,
        Dictionary<int, (CampoEntidad Campo, string NombreOriginal)> mapeo)
    {
        var dict = new Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)>();

        foreach (var kvp in mapeo)
        {
            int colIdx = kvp.Key;
            var (campo, colName) = kvp.Value;
            var cell = ws.Cells[row, colIdx];
            dict[campo] = (colIdx, colName, cell.Value);
        }

        return dict;
    }

    private static string? ObtenerString(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        if (rowValues.TryGetValue(campo, out var item) && item.Value != null)
        {
            var str = item.Value.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
        return null;
    }

    private static decimal? ObtenerDecimal(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        if (rowValues.TryGetValue(campo, out var item) && item.Value != null)
        {
            if (item.Value is double d) return Convert.ToDecimal(d);
            if (item.Value is decimal m) return m;
            if (item.Value is int i) return Convert.ToDecimal(i);
            if (item.Value is long l) return Convert.ToDecimal(l);

            var str = item.Value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(str)) return null;

            // Limpieza de símbolos de moneda y caracteres de formato
            str = str.Replace("$", "").Replace("€", "").Replace("COP", "").Replace("USD", "").Trim();

            // Manejo inteligente de coma/punto de separadores decimales
            if (str.Contains(',') && str.Contains('.'))
            {
                // Ejemplo: 1,250.50 ó 1.250,50
                if (str.LastIndexOf('.') > str.LastIndexOf(','))
                {
                    str = str.Replace(",", "");
                }
                else
                {
                    str = str.Replace(".", "").Replace(',', '.');
                }
            }
            else if (str.Contains(','))
            {
                str = str.Replace(',', '.');
            }

            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }
        return null;
    }

    private static int? ObtenerInt(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        if (rowValues.TryGetValue(campo, out var item) && item.Value != null)
        {
            if (item.Value is int i) return i;
            if (item.Value is double d) return (int)Math.Round(d);
            if (item.Value is decimal m) return (int)Math.Round(m);
            if (item.Value is long l) return (int)l;

            var str = item.Value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (int.TryParse(str, out var parsedInt)) return parsedInt;

            var dec = ObtenerDecimal(rowValues, campo);
            if (dec.HasValue) return (int)Math.Round(dec.Value);
        }
        return null;
    }

    private static DateTime? ObtenerDateTime(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        if (rowValues.TryGetValue(campo, out var item) && item.Value != null)
        {
            if (item.Value is DateTime dt) return dt;
            if (item.Value is double oaDate)
            {
                try { return DateTime.FromOADate(oaDate); } catch { }
            }

            var str = item.Value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(str)) return null;

            string[] formats = ["dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy/MM/dd", "d/M/yyyy", "dd/MM/yyyy HH:mm", "yyyy-MM-ddTHH:mm:ss"];
            if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactParsed))
            {
                return exactParsed;
            }

            if (DateTime.TryParse(str, out var standardParsed))
            {
                return standardParsed;
            }
        }
        return null;
    }

    private static bool? ObtenerBool(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        if (rowValues.TryGetValue(campo, out var item) && item.Value != null)
        {
            if (item.Value is bool b) return b;

            var str = item.Value.ToString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (str is "si" or "sí" or "true" or "1" or "activo" or "habilitado" or "yes") return true;
            if (str is "no" or "false" or "0" or "inactivo" or "deshabilitado") return false;
        }
        return null;
    }

    private static string? ObtenerNombreColumna(Dictionary<CampoEntidad, (int ColIdx, string ColName, object? Value)> rowValues, CampoEntidad campo)
    {
        return rowValues.TryGetValue(campo, out var item) ? item.ColName : null;
    }

    private static string LimpiarTexto(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.Trim(), @"\s+", " ");
    }

    private static string LimpiarDocumento(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        // Eliminar puntos y espacios comunes en cédulas y NITs, pero conservar guion de dígito de verificación
        return input.Replace(".", "").Replace(" ", "").Trim().ToUpperInvariant();
    }

    private static bool EsEmailValido(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static string NormalizarEstadoDespacho(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return "Pendiente";
        var clean = estado.Trim().ToLowerInvariant();

        if (clean.Contains("ruta") || clean.Contains("transito") || clean.Contains("camino"))
            return "En Ruta";
        if (clean.Contains("entrega") || clean.Contains("complet") || clean.Contains("despachado") || clean.Contains("finaliz"))
            return "Entregado";
        if (clean.Contains("cancel") || clean.Contains("anul"))
            return "Cancelado";

        return "Pendiente";
    }

    private static void GenerarResumenFinal(ExcelImportResultDto resultado)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Procesamiento completado con éxito:");
        sb.AppendLine($"- Hojas evaluadas: {resultado.TotalHojasProcesadas}");
        sb.AppendLine($"- Filas de datos leídas: {resultado.TotalFilasLeidas}");
        sb.AppendLine($"- Clientes: {resultado.ClientesCreados} creados, {resultado.ClientesActualizados} actualizados, {resultado.ClientesOmitidos} omitidos.");
        sb.AppendLine($"- Productos: {resultado.ProductosCreados} creados, {resultado.ProductosActualizados} actualizados, {resultado.ProductosOmitidos} omitidos.");
        sb.AppendLine($"- Ventas registradas: {resultado.VentasCreadas} ventas con {resultado.DetallesVentaCreados} ítems por un monto total de ${resultado.MontoTotalVentasImportadas:N2}.");

        if (resultado.TotalErrores > 0)
        {
            sb.AppendLine($"- Inconsistencias: Se registraron {resultado.TotalErrores} errores críticos y {resultado.TotalAdvertencias} advertencias.");
        }
        else if (resultado.TotalAdvertencias > 0)
        {
            sb.AppendLine($"- Advertencias: {resultado.TotalAdvertencias} inconsistencias menores corregidas o advertidas.");
        }

        resultado.Resumen = sb.ToString();
    }

    #endregion

    #region Generación de Plantilla de Ejemplo

    public byte[] GenerarPlantillaEjemplo()
    {
        using var package = new ExcelPackage();

        // Hoja 1: Datos Desorganizados y Mezclados en una sola tabla (Demostración de normalización automática)
        var ws1 = package.Workbook.Worksheets.Add("Ventas y Catalogo Mezclado");
        ws1.View.ShowGridLines = true;

        // Encabezados en orden no convencional y mezclando cliente, producto y venta
        string[] headers1 = [
            "NIT / Cedula", "Razon Social", "Producto / Material", "Cantidad Vendida", 
            "Precio Unitario", "Telefono", "Direccion de Envio", "Unidad Medida", 
            "Stock Inicial", "Fecha de Venta", "Estado Despacho", "Email Cliente"
        ];

        for (int i = 0; i < headers1.Length; i++)
        {
            var cell = ws1.Cells[1, i + 1];
            cell.Value = headers1[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185)); // Azul
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Datos de ejemplo mezclados
        object[,] sampleData1 = {
            { "900.123.456-1", "Constructora Bolívar S.A.", "Cemento Gris Argos 50kg", 150, 32500m, "3104567890", "Carrera 7 # 156-20 Bogotá", "Bulto", 500, DateTime.UtcNow.AddDays(-2), "Entregado", "compras@constructorabolivar.com" },
            { "800.987.654-3", "Ingeniería & Diseños Andinos", "Varilla Corrugada 1/2 pulgada", 80, 48900m, "6017894561", "Av El Dorado # 68C-61", "Unidad", 350, DateTime.UtcNow.AddDays(-1), "En Ruta", "contacto@andinos.com" },
            { "900.123.456-1", "Constructora Bolívar S.A.", "Arena de Río Lavada m3", 12, 65000m, "3104567890", "Carrera 7 # 156-20 Bogotá", "M3", 80, DateTime.UtcNow.AddDays(-2), "Entregado", "compras@constructorabolivar.com" },
            { "79.852.147", "Juan Camilo Pérez (Contratista)", "Ladrillo Estructurado Limpio", 1200, 1850m, "3209876543", "Calle 134 # 45-12 Suba", "Millar", 5000, DateTime.UtcNow, "Pendiente", "juan.perez@gmail.com" },
            { "901.456.789-0", "Ferretería El Maestro SAS", "Pintura Vinilo Tipo 1 Blanco Cuñete", 25, 145000m, "3156789012", "Calle 80 # 72-10", "Cuñete", 120, DateTime.UtcNow.AddDays(-3), "Entregado", "ferreteriaelmaestro@gmail.com" },
            { "901.456.789-0", "Ferretería El Maestro SAS", "Tubo PVC Presión 1/2 pulgada 6m", 60, 22000m, "3156789012", "Calle 80 # 72-10", "Tira", 200, DateTime.UtcNow.AddDays(-3), "Entregado", "ferreteriaelmaestro@gmail.com" },
            { "1.020.345.678", "María Fernanda Gómez", "Cemento Gris Argos 50kg", 10, 32500m, "3123456789", "Diag 45 # 23-11", "Bulto", 500, DateTime.UtcNow, "Pendiente", "mafe.gomez@hotmail.com" }
        };

        for (int r = 0; r < sampleData1.GetLength(0); r++)
        {
            for (int c = 0; c < sampleData1.GetLength(1); c++)
            {
                var val = sampleData1[r, c];
                var cell = ws1.Cells[r + 2, c + 1];
                cell.Value = val;

                if (val is decimal)
                {
                    cell.Style.Numberformat.Format = "$#,##0.00";
                }
                else if (val is DateTime)
                {
                    cell.Style.Numberformat.Format = "yyyy-mm-dd";
                }
                else if (val is int)
                {
                    cell.Style.Numberformat.Format = "#,##0";
                }
            }
        }

        ws1.Cells[ws1.Dimension.Address].AutoFitColumns();

        // Hoja 2: Ejemplo de solo Productos/Inventario
        var ws2 = package.Workbook.Worksheets.Add("Catalogo de Materiales");
        ws2.View.ShowGridLines = true;
        string[] headers2 = ["Articulo", "Detalle", "U.M.", "Precio Base", "Cant Disponible", "Activo"];
        for (int i = 0; i < headers2.Length; i++)
        {
            var cell = ws2.Cells[1, i + 1];
            cell.Value = headers2[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(39, 174, 96)); // Verde
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        object[,] sampleData2 = {
            { "Malla Electrosoldada 4mm 2.15x6m", "Malla para refuerzo de pisos y losas", "Hoja", 95000m, 150, "SI" },
            { "Alambre Negro Calibre 18", "Rollo de alambre para amarre", "Kg", 8500m, 300, "SI" },
            { "Gravilla Triturada 1/2 pulgada", "Agregado grueso para concreto estructural", "M3", 72000m, 90, "SI" }
        };

        for (int r = 0; r < sampleData2.GetLength(0); r++)
        {
            for (int c = 0; c < sampleData2.GetLength(1); c++)
            {
                var val = sampleData2[r, c];
                var cell = ws2.Cells[r + 2, c + 1];
                cell.Value = val;
                if (val is decimal) cell.Style.Numberformat.Format = "$#,##0.00";
                if (val is int) cell.Style.Numberformat.Format = "#,##0";
            }
        }
        ws2.Cells[ws2.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    #endregion
}
