using System.Linq.Expressions;
using AutoMapper;
using Domain.Models;
using Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Base;
using Services.Dtos.Product;
using Services.Utils;
using Domain.Enums;


namespace Services.Implements
{
    public class ProductService : BaseService<ProductService>, IProductService
    {
        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor
        ) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
        {
        }

        public async Task<BaseResult<ProductQueryDto, Paginate<ProductDto>>> GetProducts(
            ProductQueryDto productQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetProducts", productQueryDto);

            var predicate = _unitOfWork.GetRepository<Product>()
                .BuildSearchPredicate(productQueryDto.FilterQuery, productQueryDto.FilterBy);


            var orderBy = _unitOfWork.GetRepository<Product>()
                .BuildSortingQuery(productQueryDto.SortBy, productQueryDto.IsAsc);

            var products = await _unitOfWork.GetRepository<Product>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: productQueryDto.Page,
                size: productQueryDto.Size,
                include: x => x.Include(x => x.Parent)
            );

            var productsDto = _mapper.Map<Paginate<ProductDto>>(products);

            LogMessage(LogLevel.Information, "Out GetProducts", productsDto);

            return new BaseResult<ProductQueryDto, Paginate<ProductDto>>()
            {
                IsSuccess = true,
                Message = "Products found.",
                Request = productQueryDto,
                Response = productsDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, ProductDto>> GetProduct(string productId)
        {
            LogMessage(LogLevel.Information, "In GetProduct", productId);

            var product = await _unitOfWork.GetRepository<Product>()
                .SingleOrDefaultAsync(
                    predicate: x => x.ProductId == productId,
                    include: x => x.Include(x => x.Parent)
                );

            if (product is null)
            {
                return new BaseResult<string, ProductDto>()
                {
                    IsSuccess = false,
                    Message = "Product not found.",
                    Request = productId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var productDto = _mapper.Map<ProductDto>(product);

            LogMessage(LogLevel.Information, "Out GetProduct", productDto);

            return new BaseResult<string, ProductDto>()
            {
                IsSuccess = true,
                Message = "Product found.",
                Request = productId,
                Response = productDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<CreateProductDto, ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            LogMessage(LogLevel.Information, "In CreateProduct", createProductDto);
            var newProduct = new Product
            {
                ProductId = Guid.NewGuid().ToString(),
                Description = createProductDto.Description,
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Size = createProductDto.Size,
                Type = createProductDto.Type,
                ParentId = createProductDto.ParentId,
                ImageUrl = createProductDto.ImageUrl,
            };

            await _unitOfWork.GetRepository<Product>().InsertAsync(newProduct);
            var result = await _unitOfWork.CommitAsync() > 0;
            LogMessage(LogLevel.Information, "Insert Product", result);

            var productDto = _mapper.Map<ProductDto>(newProduct);

            LogMessage(LogLevel.Information, "Out CreateProduct", productDto);

            return new BaseResult<CreateProductDto, ProductDto>
            {
                IsSuccess = result,
                Message = "Product created.",
                Request = createProductDto,
                Response = productDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<BaseResult<UpdateProductDto, ProductDto>> UpdateProduct(string productId,
            UpdateProductDto updateProductDto)
        {
            var product = await _unitOfWork.GetRepository<Product>()
                .SingleOrDefaultAsync(predicate: x => x.ProductId == productId);

            if (product is null)
            {
                return new BaseResult<UpdateProductDto, ProductDto>()
                {
                    IsSuccess = false,
                    Message = "Product update fail.",
                    Request = updateProductDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            product = _mapper.Map(updateProductDto, product);

            _unitOfWork.GetRepository<Product>().Update(product);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;

            var productDto = _mapper.Map<ProductDto>(product);

            return new BaseResult<UpdateProductDto, ProductDto>()
            {
                IsSuccess = isSuccess,
                Message = "Product updated.",
                Request = updateProductDto,
                Response = productDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, ProductDto>> RemoveProduct(string productId)
        {
            LogMessage(LogLevel.Information, "In RemoveProduct", productId);

            var product = await _unitOfWork.GetRepository<Product>()
                .SingleOrDefaultAsync(predicate: x => x.ProductId == productId);

            if (product is null)
            {
                return new BaseResult<string, ProductDto>()
                {
                    IsSuccess = false,
                    Message = "Product not found.",
                    Request = productId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            _unitOfWork.GetRepository<Product>().Update(product);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;

            var productDto = _mapper.Map<ProductDto>(product);

            LogMessage(LogLevel.Information, "Out RemoveProduct", productDto);
            return new BaseResult<string, ProductDto>()
            {
                IsSuccess = isSuccess,
                Message = "Product removed.",
                Request = productId,
                Response = productDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}