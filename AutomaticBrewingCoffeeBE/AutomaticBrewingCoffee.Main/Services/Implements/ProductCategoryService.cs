using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using AutomaticBrewingCoffee.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.ProductCategory;
using Services.Interfaces;
using Services.Supabase;
using Services.Utils;

namespace Services.Implements;

public class ProductCategoryService : BaseService<ProductCategoryService>, IProductCategoryService
{
    private readonly ISupabaseStorageService _supabaseStorageService;

    public ProductCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor, ISupabaseStorageService supabaseStorageService) : base(
        unitOfWork,
        mapper,
        loggerFactory,
        httpContextAccessor
    )
    {
        _supabaseStorageService = supabaseStorageService;
    }


    public async Task<BaseResult<ProductCategoryQueryDto, Paginate<ProductCategoryDto>>> GetProductCategories(
        ProductCategoryQueryDto productCategoryQueryDto)
    {
        LogMessage(LogLevel.Information, "ProductCategoryService.GetProductCategories", productCategoryQueryDto);

        var predicate = _unitOfWork.GetRepository<ProductCategory>()
            .BuildSearchPredicate(productCategoryQueryDto.FilterQuery, productCategoryQueryDto.FilterBy);

        if (productCategoryQueryDto.Status is not null)
        {
            Expression<Func<ProductCategory, bool>> statusFilter = x =>
                x.Status == productCategoryQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<ProductCategory>(predicate, statusFilter);
        }

        if (productCategoryQueryDto.StartDate is not null && productCategoryQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<ProductCategory>().BuildDateRangePredicate(
                productCategoryQueryDto.StartDate,
                productCategoryQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        var orderBy = _unitOfWork.GetRepository<ProductCategory>()
            .BuildSortingQuery(productCategoryQueryDto.SortBy, productCategoryQueryDto.IsAsc);

        var productCategories = await _unitOfWork.GetRepository<ProductCategory>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: productCategoryQueryDto.Page,
            size: productCategoryQueryDto.Size
        );

        var productCategoryDtos = _mapper.Map<Paginate<ProductCategoryDto>>(productCategories);

        return new BaseResult<ProductCategoryQueryDto, Paginate<ProductCategoryDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Product>(),
            Request = productCategoryQueryDto,
            Response = productCategoryDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, ProductCategoryDto>> GetProductCategory(string productCategoryId)
    {
        LogMessage(LogLevel.Information, "ProductCategoryService.GetProductCategory", productCategoryId);

        var productCategory = await _unitOfWork.GetRepository<ProductCategory>()
            .SingleOrDefaultAsync(
                predicate: x => x.ProductCategoryId == productCategoryId
            );

        if (productCategory is null)
        {
            return new BaseResult<string, ProductCategoryDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Product>(),
                Request = productCategoryId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var productCategoryDto = _mapper.Map<ProductCategoryDto>(productCategory);

        return new BaseResult<string, ProductCategoryDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Product>(),
            Request = productCategoryId,
            Response = productCategoryDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateProductCategoryDto, ProductCategoryDto>> CreateProductCategory(
        CreateProductCategoryDto createProductCategoryDto)
    {
        LogMessage(LogLevel.Information, "In CreateProduct", createProductCategoryDto);
        var newProduct = _mapper.Map<ProductCategory>(createProductCategoryDto);

        if (!string.IsNullOrEmpty(createProductCategoryDto.ImageBase64))
        {
            var base64Data = createProductCategoryDto.ImageBase64.Contains(",")
                ? createProductCategoryDto.ImageBase64.Split(',')[1]
                : createProductCategoryDto.ImageBase64;

            var fileByte = Convert.FromBase64String(base64Data);
            var fileExtension = FileHelper.GetFileExtensionFromBase64(createProductCategoryDto.ImageBase64);
            var fileName = $"{newProduct.Name}{fileExtension}";
            var filePath = $"{SupabaseSetting.Root.Categories}/{newProduct.ProductCategoryId}/{fileName}";

            await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);

            var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

            newProduct.ImageUrl = imageUrl;
        }

        await _unitOfWork.GetRepository<ProductCategory>().InsertAsync(newProduct);
        var result = await _unitOfWork.CommitAsync();
        LogMessage(LogLevel.Information, "Insert Product", result);

        var productDto = _mapper.Map<ProductCategoryDto>(newProduct);

        LogMessage(LogLevel.Information, "Out CreateProduct", productDto);

        return new BaseResult<CreateProductCategoryDto, ProductCategoryDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<Product>(),
            Request = createProductCategoryDto,
            Response = productDto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateProductCategoryDto, ProductCategoryDto>> UpdateProductCategory(
        string productCategoryId,
        UpdateProductCategoryDto updateProductCategoryDto
    )
    {
        var product = await _unitOfWork.GetRepository<ProductCategory>()
            .SingleOrDefaultAsync(predicate: x => x.ProductCategoryId == productCategoryId);

        if (product is null)
        {
            return new BaseResult<UpdateProductCategoryDto, ProductCategoryDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.UpdateSuccess<Product>(),
                Request = updateProductCategoryDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        product = _mapper.Map(updateProductCategoryDto, product);

        if (!string.IsNullOrEmpty(updateProductCategoryDto.ImageBase64))
        {
            var base64Data = updateProductCategoryDto.ImageBase64.Contains(",")
                ? updateProductCategoryDto.ImageBase64.Split(',')[1]
                : updateProductCategoryDto.ImageBase64;

            var fileByte = Convert.FromBase64String(base64Data);
            var fileExtension = FileHelper.GetFileExtensionFromBase64(updateProductCategoryDto.ImageBase64);
            var fileName = $"{product.ProductCategoryId}{fileExtension}";
            var filePath = $"{SupabaseSetting.Root.Drinks}/{product.ProductCategoryId}/{fileName}";

            await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);

            var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

            product.ImageUrl = imageUrl;
        }

        _unitOfWork.GetRepository<ProductCategory>().Update(product);
        await _unitOfWork.CommitAsync();

        var productDto = _mapper.Map<ProductCategoryDto>(product);

        return new BaseResult<UpdateProductCategoryDto, ProductCategoryDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<Product>(),
            Request = updateProductCategoryDto,
            Response = productDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, ProductCategoryDto>> RemoveProductCategory(string productCategoryId)
    {
        LogMessage(LogLevel.Information, "In RemoveProduct", productCategoryId);

        var productCategory = await _unitOfWork.GetRepository<ProductCategory>()
            .SingleOrDefaultAsync(predicate: x => x.ProductCategoryId == productCategoryId);

        if (productCategory is null)
        {
            return new BaseResult<string, ProductCategoryDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Product>(),
                Request = productCategoryId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var productUsing = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
            predicate: x => x.ProductCategoryId == productCategoryId
        );

        if (productUsing is not null)
        {
            return new BaseResult<string, ProductCategoryDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<ProductCategory>(),
                Request = productCategoryId,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        productCategory.Delete();

        _unitOfWork.GetRepository<ProductCategory>().Update(productCategory);
        await _unitOfWork.CommitAsync();

        var productDto = _mapper.Map<ProductCategoryDto>(productCategory);

        LogMessage(LogLevel.Information, "Out RemoveProduct", productDto);

        return new BaseResult<string, ProductCategoryDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<Product>(),
            Request = productCategoryId,
            Response = productDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<ReorderProductCategoryDto, List<ProductCategoryDto>>> ReorderProductCategory(
        ReorderProductCategoryDto reorderProductCategoryDto)
    {
        var productCategories = await _unitOfWork.GetRepository<ProductCategory>().GetListAsync(
            orderBy: q => q.OrderBy(x => x.DisplayOrder)
        );

        var list = productCategories.ToList();

        var dragItem =
            list.FirstOrDefault(x => x.ProductCategoryId == reorderProductCategoryDto.DragProductCategoryId);
        var targetItem =
            list.FirstOrDefault(x => x.ProductCategoryId == reorderProductCategoryDto.TargetProductCategoryId);

        if (dragItem is null)
        {
            return new BaseResult<ReorderProductCategoryDto, List<ProductCategoryDto>>
            {
                IsSuccess = false,
                Message = "Drag or target item not found",
                Request = reorderProductCategoryDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        if (targetItem is null)
        {
            return new BaseResult<ReorderProductCategoryDto, List<ProductCategoryDto>>
            {
                IsSuccess = false,
                Message = "Drag or target item not found",
                Request = reorderProductCategoryDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        // Remove drag item temporarily
        list.Remove(dragItem);

        // Find index of target item
        var targetIndex = list.IndexOf(targetItem);
        var insertIndex = reorderProductCategoryDto.InsertAfter ? targetIndex + 1 : targetIndex;

        // Insert drag item to new position
        list.Insert(insertIndex, dragItem);

        // Reassign DisplayOrder
        for (int i = 0; i < list.Count; i++)
        {
            list[i].DisplayOrder = i + 1;
            _unitOfWork.GetRepository<ProductCategory>().Update(list[i]);
        }

        await _unitOfWork.CommitAsync();

        var productCategoryDtos = _mapper.Map<List<ProductCategoryDto>>(productCategories);

        return new BaseResult<ReorderProductCategoryDto, List<ProductCategoryDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<Product>(),
            Request = reorderProductCategoryDto,
            Response = productCategoryDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }
}