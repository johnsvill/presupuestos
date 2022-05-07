using AutoMapper;
using Presupuestos.Models;
using Presupuestos.ViewModels;

namespace Presupuestos.Servicios.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cuenta, CuentaViewModel>();
        }
    }
}
