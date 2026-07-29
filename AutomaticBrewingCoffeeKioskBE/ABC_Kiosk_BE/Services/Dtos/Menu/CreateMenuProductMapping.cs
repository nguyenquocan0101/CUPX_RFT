using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Menu
{
    public class CreateMenuProductMappingDto
    {
        [StringLength(50)][Required] public string ProductId { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public BaseStatus Status { get; set; }
    }
}
