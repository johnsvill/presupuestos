using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presupuestos.Models;
using Presupuestos.Servicios.Categorias;
using Presupuestos.Servicios.Cuentas;
using Presupuestos.Servicios.Reportes;
using Presupuestos.Servicios.Transacciones;
using Presupuestos.Servicios.Usuarios;
using Presupuestos.ViewModels;
using System.Reflection;
using System.Data;
using ClosedXML.Excel;

namespace Presupuestos.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransacciones _transacciones;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly ICategorias _categorias;
        private readonly IServicioReportes _servicioReportes;

        public TransaccionesController(ITransacciones transacciones,
            IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
            ICategorias categorias, IServicioReportes servicioReportes)
        {
            this._transacciones = transacciones;
            this._servicioUsuarios = servicioUsuarios;
            this._repositorioCuentas = repositorioCuentas;
            this._categorias = categorias;
            this._servicioReportes = servicioReportes;
        }

        public async Task<ActionResult> Crear()
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var modelo = new TransaccionViewModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return await Task.Run(() => View(modelo));
        }

        [HttpPost, ActionName("Crear")]
        public async Task<ActionResult> Crear(TransaccionViewModel transaccion)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                transaccion.Cuentas = await ObtenerCuentas(usuarioId);
                transaccion.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);

                return await Task.Run(() => View(transaccion));
            }

            var cuenta = await this._repositorioCuentas.ObtenerPorId(transaccion.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = this._categorias.ObtenerPorId(transaccion.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            transaccion.UsuarioId = usuarioId;

            if(transaccion.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await this._transacciones.Crear(transaccion);

            return RedirectToAction("CategoriaUsuario", "Categorias");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await this._repositorioCuentas.BuscarCuenta(usuarioId);

            return cuentas.Select(x => new SelectListItem
                                           (x.Nombre, x.CuentaId.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
            TipoOperacion tipoOperacion)
        {
            var categorias = await this._categorias.Obtener(usuarioId, tipoOperacion);

            var resultado = categorias
                .Select(x => new SelectListItem(x.Nombre, x.CategoriaId.ToString())).ToList();

            var opcionPorDefecto = new SelectListItem("-- Seleccione algo --", "0", true);

            resultado.Insert(0, opcionPorDefecto);

            return resultado;
        }

        public async Task<ActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);

            return Ok(categorias);
        }

        //INDEX
        public async Task<ActionResult> TransaccionesPorUsuario(int mes, int anio)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var modelo = await this._servicioReportes
                .ObtenerReporteTransaccionesDetalladas(usuarioId, mes, anio, ViewBag);

            return await Task.Run(() => View(modelo));
        }

        public async Task<ActionResult> Semanal(int mes, int anio)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await this._servicioReportes
                .ObtenerReporteSemanal(usuarioId, mes, anio, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x =>
            new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                     .Select(x => x.Monto).FirstOrDefault(),

                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                     .Select(x => x.Monto).FirstOrDefault()
            }).ToList(); 

            if(anio == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);

            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anio, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(anio, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

#pragma warning disable IDE0017 // Simplify object initialization
            var modelo = new ReporteSemanalViewModel();

            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return await Task.Run(() => View(modelo));
        }

        public async Task<ActionResult> Mensual(int anio)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            if (anio == 0)
            {
                anio = DateTime.Today.Year;
            }

            var transaccionesPorMes = await this._transacciones.ObtenerPorMes(usuarioId, anio);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ResultadosObtenerPorMes()
                {
                    Mes = x.Key,
                    Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                    .Select(x => x.Monto).FirstOrDefault(),

                    Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                    .Select(x => x.Monto).FirstOrDefault()
                }).ToList();

            for (int mes = 1; mes is <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(anio, mes, 1);

                if(transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadosObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel()
            {
                Anio = anio,
                TransaccionesPorMes = transaccionesAgrupadas
            };

            return await Task.Run(() => View(modelo));
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo,
            IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");

            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            }); 

            foreach(var transaccion in transacciones)
            {
                dataTable.Rows.Add(transaccion.FechaTransac,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }

            using(XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using(MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo);
                }
            }
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAnio(int anio)
        {
            var fechaInicio = new DateTime(anio, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);

            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);

            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        public async Task<ActionResult> ExcelReporte()
        {
            return await Task.Run(() => View());
        }

        public async Task<ActionResult> Calendario()
        {
            return await Task.Run(() => View());
        }

        [HttpGet, ActionName("ObtenerTransaccionesCalendario")]
        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, 
            DateTime end)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = start,
                    FechaFin = end
                });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransac.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransac.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto) ? "Red" : "Blue" 
            });

            return Json(eventosCalendario);
        }

        [HttpGet, ActionName("ObtenerTransaccionesPorFecha")]
        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fecha,
                    FechaFin = fecha
                });

            return Json(transacciones);
        }
    }
}
