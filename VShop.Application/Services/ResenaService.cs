using AutoMapper;
using VShop.Application.Dtos.Resena;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class ResenaService : BaseServices<Resena, ResenaDto>, IResenaService
    {
        public ResenaService(IMapper mapper, IResenaRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
