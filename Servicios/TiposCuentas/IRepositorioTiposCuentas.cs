using Presupuestos.Models;

namespace Presupuestos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task CrearTipoCuenta(TipoCuenta tipoCuenta);
        Task EliminarTipoCuenta(int TipoCuentaId);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId);
        Task OrdenarTiposCuentas(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
        Task UpdateTipoCuentas(TipoCuenta tipoCuenta);
    }
}
