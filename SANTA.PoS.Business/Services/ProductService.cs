using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Helpers;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using System.Linq.Expressions;

namespace SANTA.PoS.Business.Services;

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
        var product = await _repository.GetByIdAsync(id);

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

    public async Task UpdateProductDataAsync(string id, CreateUpdateProductDto updatedProduct)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(id);

        await UpdateProductAsync(id, updatedProduct);
    }

    public async Task UpdateProductStatusAsync(string id, CreateUpdateProductDto updatedProduct)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(id);

        await UpdateProductWithParamsAsync(id, updatedProduct, "Status");
    }

    public async Task UpdateProductDiscountAsync(string id, CreateUpdateProductDto updatedProduct)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(id);

        await UpdateProductWithParamsAsync(id, updatedProduct, "Descuento");
    }

    private async Task UpdateProductAsync(string id, CreateUpdateProductDto updatedProduct)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new DomainException($"Producto con código de barras '{id}' no encontrado.");

        if (id != updatedProduct.IdProducto)
        {
            var existingProductWithSameId = await _repository.GetByIdAsync(updatedProduct.IdProducto!);
            if (existingProductWithSameId is not null)
                throw new DomainException($"Producto con código de barras '{updatedProduct.IdProducto}' ya existe bajo el nombre de '{existingProductWithSameId.Nombre}'. Favor de usar un código de barras diferente.");
        }

        PropertyUpdateHelper.UpdateEntityFromDto(product, updatedProduct);
        product.Fechaultact = DateTime.Now;

        await _repository.UpdateAsync(product);
    }

    private async Task UpdateProductWithParamsAsync(string id, CreateUpdateProductDto updatedProduct, params string[] propertyNames)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new DomainException($"Producto con código de barras '{id}' no encontrado.");

        PropertyUpdateHelper.UpdateEntityFromDtoSelective(product, updatedProduct, propertyNames);
        product.Fechaultact = DateTime.Now;

        await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(string id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new DomainException($"Producto con código de barras '{id}' no encontrado.");

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

    public async Task<IEnumerable<ProductDto>> GetProductsWithDiscountAsync()
    {
        Expression<Func<Producto, bool>> filter = p => p.Descuento != null;
        var productsWithDiscount = await _repository.GetFilteredProductsAsync(filter);
        return _mapper.Map<IEnumerable<ProductDto>>(productsWithDiscount);
    }
}
