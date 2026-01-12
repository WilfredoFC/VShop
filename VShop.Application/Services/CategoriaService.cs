using AutoMapper;
using VShop.Application.Dtos.Categoria;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class CategoriaService : BaseServices<Categoria, CategoriaDto>, ICategoriaService
    {
        public CategoriaService(IMapper mapper, ICategoriaRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
