using Services.Base;

namespace Services.Dtos.Account;

public class AccountQueryDto : BaseQuery
{
    public bool? IsBanned { get; set; }
    public string? Status { get; set; }
}