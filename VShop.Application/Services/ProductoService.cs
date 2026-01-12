using AutoMapper;
using VShop.Application.Dtos.Producto;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class ProductoService : BaseServices<Producto, ProductoDto>, IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        public ProductoService(IMapper mapper, IProductoRepository _baseRepository) : base(mapper, _baseRepository)
        {
            _productoRepository = _baseRepository;
            _mapper = mapper;
        }

        public async Task<bool> CambiarEstadoProductoAsync(int productoId)
        {
            var producto = await _productoRepository.GetEntityByIdAsync(productoId);
            if (producto == null)
                return false;

            producto.EsActivo = !producto.EsActivo;

            await _productoRepository.UpdateEntityAsync(productoId, producto);
            return producto != null;
        }


        public async Task ActualizarStockAsync(int productoId, int cantidad)
        {
            var producto = await _productoRepository.GetEntityByIdAsync(productoId);

            producto.Stock += cantidad;

            // Lógica de negocio: desactivar si stock = 0
            if (producto.Stock <= 0)
            {
                producto.EsActivo = false;
                producto.Stock = 0; // Asegurar que no sea negativo
            }
            else if (!producto.EsActivo && producto.Stock > 0)
            {
                // Opcional: reactivar si vuelve a haber stock
                producto.EsActivo = true;
            }

            await _productoRepository.UpdateEntityAsync(productoId,producto);

        }


        public async Task DesactivarProductosSinStockAsync()
        {
            await _productoRepository.DesactivarProductosSinStockAsync();
        }
    }
}