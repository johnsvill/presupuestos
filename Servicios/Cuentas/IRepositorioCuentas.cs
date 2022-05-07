using Presupuestos.Models;
using Presupuestos.ViewModels;

namespace Presupuestos.Servicios.Cuentas
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> BuscarCuenta(int usuarioId);
        Task CrearCuenta(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int TipoCuentaId, int UsuarioId);
    }
}
