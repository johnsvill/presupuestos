using Presupuestos.Models;

namespace Presupuestos.Servicios.Transacciones
{
    public interface ITransacciones
    {
        Task Crear(Transaccion transaccion);
    }
}
