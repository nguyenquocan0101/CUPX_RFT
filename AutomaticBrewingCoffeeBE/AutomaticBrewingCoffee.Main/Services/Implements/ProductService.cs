using Services.Interfaces;
using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using AutomaticBrewingCoffee.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Product;
using Services.Supabase;
using Services.Utils;

namespace Services.Implements
{
    public class ProductService : BaseService<ProductService>, IProductService
    {
        private readonly ISupabaseStorageService _supabaseStorageService;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor,
            ISupabaseStorageService supabaseStorageService
        ) : base(
            unitOfWork,
            mapper,
            loggerFactory,
            httpContextAccessor
        )
        {
            _supabaseStorageService = supabaseStorageService;
        }

        public async Task<BaseResult<ProductQueryDto, Paginate<ProductDto>>> GetProducts(
            ProductQueryDto productQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetProducts", productQueryDto);

            var predicate = _unitOfWork.GetRepository<Product>()
                .BuildSearchPredicate(productQueryDto.FilterQuery, productQueryDto.FilterBy);

            Expression<Func<Product, bool>> isDeletedFilter = x =>
                x.IsDeleted == false;
            predicate = ExpressionHelper.CombineExpressions<Product>(predicate, isDeletedFilter);

            if (productQueryDto.StartDate is not null && productQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Product>().BuildDateRangePredicate(
                    productQueryDto.StartDate,
                    productQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            if (productQueryDto.Status is not null)
            {
                Expression<Func<Product, bool>> statusFilter = x =>
                    x.Status == productQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, statusFilter);
            }

            if (productQueryDto.ProductSize is not null)
            {
                Expression<Func<Product, bool>> statusFilter = x =>
                    x.Size == productQueryDto.ProductSize;
                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, statusFilter);
            }

            if (productQueryDto.ProductType is not null)
            {
                Expression<Func<Product, bool>> statusFilter = x =>
                    x.Type == productQueryDto.ProductType;
                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, statusFilter);
            }

            if (productQueryDto.TagName is not null)
            {
                Expression<Func<Product, bool>> statusFilter = x =>
                    x.TagName == productQueryDto.TagName;
                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, statusFilter);
            }

            if (productQueryDto.CategoryName is not null)
            {
                Expression<Func<Product, bool>> statusFilter = x =>
                    x.ProductCategory!.Name == productQueryDto.CategoryName;
                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, statusFilter);
            }

            if (productQueryDto.IsHasWorkflow is not null)
            {
                Expression<Func<Product, bool>> workflowFilter = x =>
                    productQueryDto.IsHasWorkflow == true
                        ? x.Workflows != null && x.Workflows.Any()
                        : x.Workflows == null || !x.Workflows.Any();

                predicate = ExpressionHelper.CombineExpressions<Product>(predicate, workflowFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Product>()
                .BuildSortingQuery(productQueryDto.SortBy, productQueryDto.IsAsc);

            var products = await _unitOfWork.GetRepository<Product>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: productQueryDto.Page,
                size: productQueryDto.Size,
                include: x => x.Include(x => x.Parent)
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Workflows)
                    .Include(x => x.ProductAttributes)
                    .ThenInclude(x => x.AttributeOptions)
            );

            var productsDto = _mapper.Map<Paginate<ProductDto>>(products);

            LogMessage(LogLevel.Information, "Out GetProducts", productsDto);

            return new BaseResult<ProductQueryDto, Paginate<ProductDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Product>(),
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
                        .Include(x => x.ProductCategory)
                        .Include(x => x.ProductAttributes)
                        .ThenInclude(x => x.AttributeOptions)
                );

            if (product is null)
            {
                return new BaseResult<string, ProductDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Product>(),
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
                Message = MessageUtil.ReadSuccess<Product>(),
                Request = productId,
                Response = productDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<CreateProductDto, ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            LogMessage(LogLevel.Information, "In CreateProduct", createProductDto);
            var newProduct = _mapper.Map<Product>(createProductDto);

            if (!string.IsNullOrEmpty(createProductDto.ImageBase64))
            {
                var base64Data = createProductDto.ImageBase64.Contains(",")
                    ? createProductDto.ImageBase64.Split(',')[1]
                    : createProductDto.ImageBase64;

                var fileByte = Convert.FromBase64String(base64Data);
                var fileExtension = FileHelper.GetFileExtensionFromBase64(createProductDto.ImageBase64);
                var fileName = $"{newProduct.ProductId}{fileExtension}";
                var filePath = $"{SupabaseSetting.Root.Drinks}/{newProduct.ProductId}/{fileName}";

                await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);

                var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

                newProduct.ImageUrl = imageUrl;
            }

            await _unitOfWork.GetRepository<Product>().InsertAsync(newProduct);
            var result = await _unitOfWork.CommitAsync();
            LogMessage(LogLevel.Information, "Insert Product", result);

            var productDto = _mapper.Map<ProductDto>(newProduct);

            LogMessage(LogLevel.Information, "Out CreateProduct", productDto);

            return new BaseResult<CreateProductDto, ProductDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Product>(),
                Request = createProductDto,
                Response = productDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<BaseResult<UpdateProductDto, ProductDto>> UpdateProduct(string productId,
            UpdateProductDto updateProductDto)
        {
            // Lấy Product kèm ProductAttributes và AttributeOptions
            var product = await _unitOfWork.GetRepository<Product>()
                .SingleOrDefaultAsync(
                    predicate: x => x.ProductId == productId,
                    include: x => x.Include(x => x.ProductAttributes)
                        .ThenInclude(x => x.AttributeOptions)
                );

            if (product is null)
            {
                return new BaseResult<UpdateProductDto, ProductDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Product>(),
                    Request = updateProductDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Xoá toàn bộ ProductAttributes (EF sẽ xoá luôn AttributeOptions do quan hệ Cascade)
            if (product.ProductAttributes is not null && product.ProductAttributes.Any())
            {
                _unitOfWork.GetRepository<ProductAttribute>().DeleteRange(product.ProductAttributes);
                await _unitOfWork.CommitAsync(); // Commit để EF không track nữa
            }

            // Ánh xạ lại các trường mới của Product từ DTO (trừ ProductAttributes)
            _mapper.Map(updateProductDto, product);

            // Xử lý ảnh nếu có
            if (!string.IsNullOrEmpty(updateProductDto.ImageBase64))
            {
                var base64Data = updateProductDto.ImageBase64.Contains(",")
                    ? updateProductDto.ImageBase64.Split(',')[1]
                    : updateProductDto.ImageBase64;

                var fileByte = Convert.FromBase64String(base64Data);
                var fileExtension = FileHelper.GetFileExtensionFromBase64(updateProductDto.ImageBase64);
                var fileName = $"{product.ProductId}{fileExtension}";
                var filePath = $"{SupabaseSetting.Root.Drinks}/{product.ProductId}/{fileName}";

                await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);
                var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

                product.ImageUrl = imageUrl;
            }

            // Tách riêng ProductAttributes mới
            var newProductAttributes = _mapper.Map<List<ProductAttribute>>(updateProductDto.ProductAttributes);

            // Gán null trước để tránh EF nghĩ là đang cập nhật danh sách
            product.ProductAttributes = null;

            // Cập nhật product trước
            _unitOfWork.GetRepository<Product>().Update(product);
            await _unitOfWork.CommitAsync();

            // Gán productId cho từng ProductAttribute mới (vì map mới không có quan hệ)
            foreach (var attr in newProductAttributes)
            {
                attr.ProductId = product.ProductId;
                if (attr.AttributeOptions != null)
                {
                    foreach (var option in attr.AttributeOptions)
                    {
                        option.ProductAttribute = attr;
                    }
                }
            }

            // Thêm mới lại ProductAttributes (và cả AttributeOptions con)
            if (newProductAttributes.Any())
            {
                await _unitOfWork.GetRepository<ProductAttribute>().InsertRangeAsync(newProductAttributes);
                await _unitOfWork.CommitAsync();
            }

            // Map lại ra ProductDto để trả về
            var productDto = _mapper.Map<ProductDto>(product);

            return new BaseResult<UpdateProductDto, ProductDto>
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Product>(),
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
                    Message = MessageUtil.NotFound<Product>(),
                    Request = productId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (product.Type == nameof(EProductType.Parent))
            {
                var productUsing = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
                    predicate: x => x.ProductId == productId
                );

                if (productUsing is not null)
                {
                    return new BaseResult<string, ProductDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.AlreadyUsing<Product, Menu>(),
                        Request = productId,
                        Response = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            if (product.Type == nameof(EProductType.Child))
            {
                var productUsing = await _unitOfWork.GetRepository<Workflow>().SingleOrDefaultAsync(
                    predicate: x => x.ProductId == productId
                );

                if (productUsing is not null)
                {
                    return new BaseResult<string, ProductDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.AlreadyUsing<Product, Workflow>(),
                        Request = productId,
                        Response = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            product.Delete();

            _unitOfWork.GetRepository<Product>().Update(product);
            await _unitOfWork.CommitAsync();

            var productDto = _mapper.Map<ProductDto>(product);

            LogMessage(LogLevel.Information, "Out RemoveProduct", productDto);
            return new BaseResult<string, ProductDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Product>(),
                Request = productId,
                Response = productDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, ProductDto>> CloneProduct(string productId)
        {
            var existProduct = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                predicate: x => x.ProductId == productId,
                include: x => x.Include(x => x.Workflows)
                    .ThenInclude(x => x.Steps)
                    .Include(x => x.ProductAttributes)
                    .ThenInclude(x => x.AttributeOptions)
            );

            if (existProduct is null)
            {
                return new BaseResult<string, ProductDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Product>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null,
                    Request = productId
                };
            }

            var productClone = _mapper.Map<Product>(existProduct);

            if (existProduct.Type == EProductType.Parent.ToString())
            {
                productClone.ParentId = existProduct.ProductId;
                productClone.Name = $"{productClone.Name} ({EProductType.Child.ToString()})";
            }

            if (existProduct.Type == EProductType.Child.ToString())
            {
                productClone.ParentId = existProduct.ParentId;
                productClone.Name = $"{productClone.Name}";
            }

            productClone.Type = EProductType.Child.ToString();
            productClone.TagName = EProductType.Child.ToString();

            if (
                !string.IsNullOrEmpty(existProduct.ImageUrl) &&
                _supabaseStorageService.IsSupabaseResource(existProduct.ImageUrl)
            )
            {
                var bytes = await _supabaseStorageService.DownloadFile(
                    SupabaseSetting.Bucket.Images, existProduct.ImageUrl);

                var oldExtension = Path.GetExtension(existProduct.ImageUrl);
                var fileName = $"{productClone.ProductId}{oldExtension}";
                var filePath = $"{SupabaseSetting.Root.Drinks}/{productClone.ProductId}/{fileName}";

                await _supabaseStorageService.UploadFile(bytes, filePath, SupabaseSetting.Bucket.Images, true);

                var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);
                productClone.ImageUrl = imageUrl;
            }

            var workflowClones = productClone.Workflows;

            var stepClones = new List<Step>();

            foreach (var workflow in productClone.Workflows!)
            {
                if (workflow.Steps is not null)
                {
                    stepClones.AddRange(workflow.Steps);
                }

                workflow.Steps = null;
            }

            productClone.Workflows = null;

            await _unitOfWork.GetRepository<Product>().InsertAsync(productClone);
            await _unitOfWork.CommitAsync();


            if (workflowClones is not null && workflowClones.Count > 0)
            {
                await _unitOfWork.GetRepository<Workflow>().InsertRangeAsync(workflowClones);
                await _unitOfWork.CommitAsync();
            }

            if (stepClones.Count > 0)
            {
                await _unitOfWork.GetRepository<Step>().InsertRangeAsync(stepClones);
                await _unitOfWork.CommitAsync();
            }

            var productDto = _mapper.Map<ProductDto>(productClone);

            return new BaseResult<string, ProductDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Product>(),
                Request = productId,
                Response = productDto,
            };
        }

        public async Task<BaseResult<string, ProductDto>> UploadImage(string productId,
            UploadProductImageDto uploadProductImageDto)
        {
            LogMessage(LogLevel.Information, "In UploadImage");

            var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                predicate: x => x.ProductId == productId);

            if (product is null)
            {
                return new BaseResult<string, ProductDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Product>(),
                    Request = productId,
                    Response = null,
                };
            }

            var fileByte = await FileHelper.ToByteArrayAsync(uploadProductImageDto.File);
            var fileName = $"{product.Name + FileHelper.GetExtension(uploadProductImageDto.File)}";
            var filePath = $"{SupabaseSetting.Root.Drinks}/{productId}/{fileName}";

            await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);
            var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

            product.ImageUrl = imageUrl;
            _unitOfWork.GetRepository<Product>().Update(product);
            await _unitOfWork.CommitAsync();
            var productDto = _mapper.Map<ProductDto>(product);

            LogMessage(LogLevel.Information, "Out UploadImage");

            return new BaseResult<string, ProductDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Product>(),
                Request = productId,
                Response = productDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}