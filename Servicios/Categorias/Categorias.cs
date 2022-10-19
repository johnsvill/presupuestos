using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Servicios.Categorias
{
    public class Categorias : ICategorias
    {
        private readonly string _connectionString;

        public Categorias(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(this._connectionString);

            var id = await connection.QuerySingleAsync<int>(@$"
                            INSERT INTO [dbo].[Categorias] (Nombre, TipoOperacionId, UsuarioId)
                            VALUES(@Nombre, @TipoOperacionId, @UsuarioId)

                            SELECT SCOPE_IDENTITY();", categoria);

            categoria.CategoriaId = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<Categoria>(
                           @$"SELECT * FROM [dbo].[Categorias]
                              WHERE [UsuarioId] = @usuarioId
                              ORDER BY [Nombre]
                              OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT
                              {paginacion.RecordsPorPagina} ROWS ONLY",
                              new { usuarioId });
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.ExecuteScalarAsync<int>(
                            @$"SELECT COUNT(*) FROM [Categorias]
                            WHERE [UsuarioId] = @usuarioId", new {usuarioId});
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacion)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<Categoria>(@$"
                              SELECT * FROM [dbo].[Categorias]
                              WHERE [UsuarioId] = @usuarioId
                              AND TipoOperacionId = @tipoOperacion",
                              new { usuarioId, tipoOperacion });
        }

        public async Task<Categoria> ObtenerPorId(int categoriaId, int usuarioId)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categoria>(@$"
                              SELECT * FROM [dbo].[Categorias]
                              WHERE [UsuarioId] = @usuarioId
                              AND [CategoriaId] = @categoriaId",
                              new { categoriaId, usuarioId});   
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@$"
                                    UPDATE [dbo].[Categorias]
                                    SET Nombre = @Nombre, 
                                    TipoOperacionId = @TipoOperacionId
                                    WHERE CategoriaId = @CategoriaId", categoria);
        }

        public async Task Borrar(int categoriaId)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@$"
                                 DELETE FROM [dbo].[Categorias]
                                 WHERE CategoriaId = @categoriaId",
                                 new { categoriaId });
        }
    }
}
