using Presupuestos.Models;

namespace Presupuestos.Servicios.Categorias
{
    public interface ICategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int categoriaId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacion);
        Task<Categoria> ObtenerPorId(int categoriaId, int usuarioId);
    }
}
