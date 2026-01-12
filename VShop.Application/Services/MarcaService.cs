using AutoMapper;
using VShop.Application.Dtos.Marca;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class MarcaService : BaseServices<Marca, MarcaDto>, IMarcaService
    {
        public MarcaService(IMapper mapper, IMarcaRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
