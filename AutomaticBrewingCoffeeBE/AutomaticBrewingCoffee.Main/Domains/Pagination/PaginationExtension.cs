using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Repository.Pagination;

public static class PaginationExtension
{
    public static async Task<IPaginate<T>> ToPaginateAsync<T>(this IQueryable<T> queryable, int page, int size,
        int firstPage = 1, bool ignorePaging = false)
    {
        var total = await queryable.CountAsync();
        List<T> items;
        int totalPages;

        if (ignorePaging)
        {
            items = await queryable.ToListAsync();
            totalPages = 1;
            page = firstPage;
            size = total == 0 ? 1 : total;
        }
        else
        {
            if (firstPage > page)
                throw new ArgumentException($"page ({page}) must greater or equal than firstPage ({firstPage})");

            total = await queryable.CountAsync();
            items = await queryable.Skip((page - firstPage) * size).Take(size).ToListAsync();
            totalPages = (int)Math.Ceiling(total / (double)size);
        }

        return new Paginate<T>
        {
            Page = page,
            Size = size,
            Total = total,
            Items = items,
            TotalPages = totalPages
        };
    }
}