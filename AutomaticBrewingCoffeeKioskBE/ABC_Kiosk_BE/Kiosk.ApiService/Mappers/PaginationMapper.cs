using AutoMapper;
using Domain.Pagination;

namespace Kiosk.ApiService.Mappers
{
    public class PaginationMapper : Profile
    {
        public PaginationMapper()
        {
            CreateMap(typeof(Paginate<>), typeof(Paginate<>));
        }
    }
}
