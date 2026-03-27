using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CQRSs.Products.Commands
{
    public class DeleteProductCommand : IRequest<CustomizedAPIResponse<int>>
    {
        public int Id { get; set; }

        internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, CustomizedAPIResponse<int>>
        {
            private readonly IApplicationDbContext _context;

            public DeleteProductCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<CustomizedAPIResponse<int>> Handle(DeleteProductCommand deleteProduct, CancellationToken cancellationToken)
            {
                var productToDelete =
                   await _context.Products1!.Where(product => product.Id == deleteProduct.Id).FirstOrDefaultAsync(); ;

                if (productToDelete is null)
                {
                    throw new ApiErrorExceptions($"Product with Id {deleteProduct.Id} not found, therefore can not delete.");
                }

                _context.Products1!.Remove(productToDelete);

                await _context.SaveChangesAsync();

                return new CustomizedAPIResponse<int>(productToDelete.Id, $"Product with Id {productToDelete.Id} deleted successfully.");
            }
        }
    }
}
