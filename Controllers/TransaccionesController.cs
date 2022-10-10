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

            return categorias.Select(x => new SelectListItem(x.Nombre, x.CategoriaId.ToString()));
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

            var modelo = new ReporteSemanalViewModel();

            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return await Task.Run(() => View(modelo));
        }

        public async Task<ActionResult> Mensual()
        {
            return await Task.Run(() => View());
        }

        public async Task<ActionResult> ExcelReporte()
        {
            return await Task.Run(() => View());
        }

        public async Task<ActionResult> Calendario()
        {
            return await Task.Run(() => View());
        }
    }
}
