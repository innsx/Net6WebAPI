using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CQRSs.Products.Commands
{
    public class UpdateProductCommand : IRequest<CustomizedAPIResponse<int>>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Rate { get; set; }

        internal class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, CustomizedAPIResponse<int>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IAuthenticatedUser _authenticatedUser;

            public UpdateProductCommandHandler(IApplicationDbContext context, IAuthenticatedUser authenticatedUser)
            {
                _context = context;
                _authenticatedUser = authenticatedUser;
            }

            public async Task<CustomizedAPIResponse<int>> Handle(UpdateProductCommand requestUpdateProduct, CancellationToken cancellationToken)
            {
                var product =
                   await _context.Products1!.Where(product => product.Id == requestUpdateProduct.Id).FirstOrDefaultAsync(); ;

                if (product is null)
                {
                    throw new ApiErrorExceptions($"Unable find a Product with {requestUpdateProduct.Id} to Update Product.");
                }

                product.Name = requestUpdateProduct.Name!;
                product.Description = requestUpdateProduct.Description!;
                product.Rate = requestUpdateProduct.Rate;
                product.ModifieddBy = _authenticatedUser.UserId;
                product.ModifieddOn = DateTime.Now;

                await _context.SaveChangesAsync();

                return new CustomizedAPIResponse<int>(product.Id, $"Product with Id {product.Id} is updated successfully.");

            }
        }
    }
}
