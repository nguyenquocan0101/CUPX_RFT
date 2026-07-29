using Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Menu
{
    public class MenuProductMappingQueryDto : BaseQuery
    {
        public BaseStatus? Status { get; set; } = null!;
    }
}
