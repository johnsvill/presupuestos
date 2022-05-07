using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;
using Presupuestos.ViewModels;

namespace Presupuestos.Servicios.Cuentas
{
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string _connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection"); 
        }

        public async Task CrearCuenta(Cuenta cuenta)
        {
            using var connection = new SqlConnection(this._connectionString);

            var id = await connection.QuerySingleAsync<int>($@"
                            INSERT INTO [dbo].[Cuentas] ([Nombre], [TipoCuentaId], [Balance], [Descripcion])
                            VALUES(@Nombre, @TipoCuentaId, @Balance, @Descripcion);

                            SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.CuentaId = id;
        }

        public async Task<IEnumerable<Cuenta>> BuscarCuenta(int usuarioId)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<Cuenta>(@"
                        SELECT c.CuentaId, c.Nombre, c.Balance, tc.Nombre as TipoCuenta, c.TipoCuentaId
                        FROM [dbo].[TiposCuentas] tc
                        INNER JOIN [dbo].[Cuentas] c
                        on tc.TipoCuentaId = c.CuentaId
                        WHERE tc.UsuarioId = @UsuarioId
                        ORDER BY tc.Orden", new { usuarioId });
        }

        public async Task<Cuenta> ObtenerPorId(int TipoCuentaId, int UsuarioId) 
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"
                        SELECT c.CuentaId, c.Nombre, c.Balance, c.Descripcion,
                        tc.TipoCuentaId
                        FROM [dbo].[TiposCuentas] tc
                        INNER JOIN [dbo].[Cuentas] c
                        ON tc.TipoCuentaId = c.CuentaId
                        WHERE tc.UsuarioId = @UsuarioId
                        AND c.CuentaId = @TipoCuentaId",
                        new { TipoCuentaId, UsuarioId });
        }

        public async Task Actualizar(CuentaViewModel cuenta)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@"
                                        UPDATE [dbo].[Cuentas] SET Nombre = @Nombre,
                                        Balance = @Balance, Descripcion = @Descripcion,
                                        TipoCuentaId = @TipoCuentaId
                                        WHERE CuentaId = @CuentaId", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(this._connectionString);

            await connection.ExecuteAsync(@$"
                                        DELETE [dbo].[Cuentas] WHERE CuentaId = @id",
                                        new { id });
        }
    }
}
