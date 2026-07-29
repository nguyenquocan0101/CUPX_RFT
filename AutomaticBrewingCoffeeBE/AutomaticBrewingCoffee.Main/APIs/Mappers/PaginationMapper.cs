using AutoMapper;
using AutomaticBrewingCoffee.Repository.Pagination;

namespace AutomaticBrewingCoffee.API.Mappers
{
    public class PaginationMapper : Profile
    {
        public PaginationMapper()
        {
            CreateMap(typeof(Paginate<>), typeof(Paginate<>));
        }
    }
}