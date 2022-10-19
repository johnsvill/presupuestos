using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Servicios
{  
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string _connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection");
        }

        public async Task CrearTipoCuenta(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(this._connectionString);

            //Parametros usando tipo anonimo
            var id = await connection.QuerySingleAsync<int>
                            ("[dbo].[sp_TiposCuentas_Insertar]", 
                            new { usuarioId = tipoCuenta.UsuarioId,
                            nombre = tipoCuenta.Nombre },
                            commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.TipoCuentaId = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId, int id = 0)
        {
            using var connection = new SqlConnection(this._connectionString);

            var existe = await connection.QueryFirstOrDefaultAsync<int>($@"
                                SELECT 1
                                FROM [dbo].[TiposCuentas]
                                WHERE [Nombre] = @Nombre AND [UsuarioId] = @UsuarioId
                                AND [TipoCuentaId] <> @id;",
                                new {nombre, usuarioId, id});

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<TipoCuenta>(@"
                                    SELECT [TipoCuentaId], [Nombre], [Orden]
                                    FROM [dbo].[TiposCuentas]
                                    WHERE UsuarioId = @UsuarioId
                                    ORDER BY [Orden]", new { usuarioId });
        }

        public async Task UpdateTipoCuentas(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@"
                            UPDATE [dbo].[TiposCuentas]
                            SET Nombre = @Nombre
                            WHERE TipoCuentaId = @TipoCuentaId", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int TipoCuentaId, int usuarioId)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"
                        SELECT [TipoCuentaId], [Nombre], [Orden]
                        FROM [dbo].[TiposCuentas]
                        WHERE TipoCuentaId = @TipoCuentaId AND UsuarioId = @UsuarioId",
                        new { TipoCuentaId, usuarioId });    
        }

        public async Task EliminarTipoCuenta(int TipoCuentaId)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@"
                            DELETE FROM [dbo].[TiposCuentas]
                            WHERE TipoCuentaId = @TipoCuentaId", new { TipoCuentaId });
        }

        public async Task OrdenarTiposCuentas(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = @"UPDATE [dbo].[TiposCuentas] SET [Orden]
                                          = @Orden WHERE [TipoCuentaId] = @TipoCuentaId";

            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }
}
