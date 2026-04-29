using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using System.Linq.Expressions;

namespace SANTA.PoS.Business.Services
{
    public class ProductService(IProductRepository repository, IMapper mapper)
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductDto> CreateProductAsync(CreateUpdateProductDto product)
        {
            ArgumentNullException.ThrowIfNull(product);

            var createProduct = _mapper.Map<Producto>(product);

            var productCreated = await _repository.CreateAsync(createProduct);

            return _mapper.Map<ProductDto>(productCreated);
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id);

            Producto? product = await _repository.GetByIdAsync(id);

            if (product is null)
            {
                throw new DomainException($"Producto con código de barras '{id}' no encontrado.");
            }

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task UpdateProductAsync(CreateUpdateProductDto product)
        {
            ArgumentNullException.ThrowIfNull(product);
            var updateProduct = _mapper.Map<Producto>(product);
            await _repository.UpdateAsync(updateProduct);
        }

        public async Task DeleteProductAsync(string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> GetFilteredProductsAsync(string stringFilter)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(stringFilter, nameof(stringFilter));

            Expression<Func<Producto, bool>> filter = p =>
                p.Nombre.Contains(stringFilter) ||
                p.IdProducto.Contains(stringFilter);

            var filteredProducts = await _repository.GetFilteredProductsAsync(filter);
            return _mapper.Map<IEnumerable<ProductDto>>(filteredProducts);
        }
    }
}
