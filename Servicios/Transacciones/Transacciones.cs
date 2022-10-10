using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Servicios.Transacciones
{
    public class Transacciones : ITransacciones
    {
        private readonly string _connectionString;

        public Transacciones(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection"); 
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(this._connectionString);

            var id = await connection.QuerySingleAsync<int>("[dbo].[Transacciones_Insertar]",
                                 new 
                                 {
                                     transaccion.UsuarioId,
                                     transaccion.FechaTransac,
                                     transaccion.Monto,
                                     transaccion.CategoriaId,
                                     transaccion.CuentaId,
                                     transaccion.Nota
                                 },
                                 commandType: System.Data.CommandType.StoredProcedure);

            transaccion.IdTransac = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<Transaccion>(@$"
                        SELECT T.IdTransac, T.Monto, T.FechaTransaccion AS FechaTransac, 
                        c.Nombre AS Categoria, c1.Nombre AS Cuenta, c.TipoOperacionId                        
                        FROM Transacciones t
                        INNER JOIN Categorias c ON c.CategoriaId = T.CategoriaId
                        INNER JOIN Cuentas c1 ON c1.CuentaId = T.CuentaId
                        WHERE t.CuentaId = @CuentaId AND t.UserId = @UsuarioId
                        AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<Transaccion>(@$"
                        SELECT T.IdTransac, T.Monto, T.FechaTransaccion AS FechaTransac, 
                        c.Nombre AS Categoria, c1.Nombre AS Cuenta, c.TipoOperacionId                        
                        FROM Transacciones t
                        INNER JOIN Categorias c ON c.CategoriaId = T.CategoriaId
                        INNER JOIN Cuentas c1 ON c1.CuentaId = T.CuentaId
                        WHERE t.UserId = @UsuarioId
                        AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                        ORDER BY t.FechaTransaccion", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> 
            ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@$"

                    SELECT DATEDIFF(d, @FechaInicio, t.FechaTransaccion) / 7 + 1 AS Semana, 
                    SUM(t.Monto) AS Monto, cat.TipoOperacionId
                    FROM Transacciones t
                    INNER JOIN Categorias cat ON cat.CategoriaId = t.CategoriaId
                    WHERE t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    AND t.UserId = @UsuarioId
                    GROUP BY DATEDIFF(d, @FechaInicio, t.FechaTransaccion) / 7,
                    cat.TipoOperacionId", modelo);
        }
    }
}
