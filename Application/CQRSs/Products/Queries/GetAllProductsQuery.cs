using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CQRSs.Products.Queries
{
    public class GetAllProductsQuery : IRequest<CustomizedAPIResponse<IEnumerable<Product>>>
    {
        internal class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, CustomizedAPIResponse<IEnumerable<Product>>>
        {
            private readonly IApplicationDbContext _context;

            public GetAllProductsQueryHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<CustomizedAPIResponse<IEnumerable<Product>>> Handle(GetAllProductsQuery requestAllProducts, CancellationToken cancellationToken)
            {
                var products = await _context.Products1!.ToListAsync(cancellationToken);

                if (products is null)
                {
                    throw new ApiErrorExceptions($"No Products found.");
                }

                return new CustomizedAPIResponse<IEnumerable<Product>>(products, "Data fetched successfully.");
            }
        }
    }
}
