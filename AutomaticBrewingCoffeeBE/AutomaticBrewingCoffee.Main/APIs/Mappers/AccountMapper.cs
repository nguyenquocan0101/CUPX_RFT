using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Account;
using Services.Dtos.Auth;

namespace AutomaticBrewingCoffee.API.Mappers;

public class AccountMapper : Profile
{
    public AccountMapper()
    {
        CreateMap<CreateAccountDto, Account>().ReverseMap();
        
        CreateMap<IPaginate<AccountDto>, IPaginate<Action>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<Account, AccountDto>()
            .ReverseMap();
    }
}