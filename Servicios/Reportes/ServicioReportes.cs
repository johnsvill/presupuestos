using Presupuestos.Models;
using Presupuestos.Servicios.Transacciones;

namespace Presupuestos.Servicios.Reportes
{
    public class ServicioReportes : IServicioReportes
    {

        private readonly string _connectionString;
        private readonly ITransacciones _transacciones;
        private readonly HttpContext _httpContext;

        public ServicioReportes(ITransacciones transacciones,
            IHttpContextAccessor httpContextAccessor)
        {
            this._transacciones = transacciones;
            this._httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ReporteTransaccionesDetalladas>
            ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId,
            int mes, int anio, dynamic ViewBag)
        {
            //TUPLA
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await this._transacciones
                .ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            var modelo = GenerarMetodoTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        private void AsignarValoresViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = this._httpContext.Request.Path + this._httpContext.Request.QueryString;
        }

        private static ReporteTransaccionesDetalladas GenerarMetodoTransaccionesDetalladas(DateTime fechaInicio, 
            DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransac)
                .GroupBy(x => x.FechaTransac)
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            return modelo;
        }

        //TUPLA
        private static (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int anio)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes is <= 0 || mes is > 12 || anio is <= 1900)
            {
                var hoy = DateTime.Today;

                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

        public async Task<ReporteTransaccionesDetalladas>
            ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anio, dynamic ViewBag)
        {
            //TUPLA
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await this._transacciones.ObtenerPorUsuarioId(parametro);

            var modelo = GenerarMetodoTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> 
            ObtenerReporteSemanal(int usuarioId, int mes, int anio, dynamic ViewBag)
        {
            //TUPLA
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            AsignarValoresViewBag(ViewBag, fechaInicio);

            var modelo = await this._transacciones.ObtenerPorSemana(parametro);

            return modelo;
        }
    }
}
