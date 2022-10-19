using Presupuestos.Models;

namespace Presupuestos.Servicios.Categorias
{
    public interface ICategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int categoriaId);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);        
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacion);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<Categoria> ObtenerPorId(int categoriaId, int usuarioId);
    }
}
