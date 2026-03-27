using Application.Interfaces;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.CQRSs.Products.Commands
{
    public class CreateProductCommand : IRequest<CustomizedAPIResponse<int>>
    {
        public string? Name { get; set; }

        //public string? Description { get; set; }  //matching CreateProductCommand Description PROPERTY to Product Description Property
        //automapping CreateProductCommand Remarks Property to Product Description Property
        public string? Remarks { get; set; }  
        public decimal Rate { get; set; }


        //internal: The class is visible only within its own assembly.

        //IRequestHandler<TRequest, TResponse>: The MediatR interface that this class implements.

        //ApiResponse<int>: A generic wrapper for API responses,
        //indicating it returns an int (likely the ID of the newly created product)
        //along with metadata like status or success messages
        internal class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CustomizedAPIResponse<int>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;
            private readonly IAuthenticatedUser _authenticatedUser;
            public CreateProductCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthenticatedUser authenticatedUser)
            {
                _context = context;
                _mapper = mapper;
                _authenticatedUser = authenticatedUser;
            }

            public async Task<CustomizedAPIResponse<int>> Handle(CreateProductCommand createProduct, CancellationToken cancellationToken)
            {
                //HERE is the ACTUAL MAPPING TAKES PLACED
                var product = _mapper.Map<Product>(createProduct);
                product.CreatedBy = _authenticatedUser.UserId;
                product.CreatedOn = DateTime.Now;

                await _context.Products1!.AddAsync(product);
                await _context.SaveChangesAsync();

                return new CustomizedAPIResponse<int>(product.Id, "Product Created successfully.");


                //MANUALLY MAPPING: destProperty = sourceProperty;
                //var newProduct = new Product();
                //newProduct.Name = createProduct.Name!;
                //newProduct.Description = createProduct.Description!;
                //newProduct.Rate = createProduct.Rate;
                //newProduct.CreatedOn = DateTime.Now;
                //await _context.Products1!.AddAsync(newProduct);
                //await _context.SaveChangesAsync();
                //return newProduct.Id;
            }
        }
    }
}
