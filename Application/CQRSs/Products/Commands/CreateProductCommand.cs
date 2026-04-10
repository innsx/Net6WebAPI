using Application.Interfaces;
using Application.Wrappers;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.CQRSs.Products.Commands
{
    //MediatR.IRequest: Marks this class as a request for MediatR to pass to a handler.
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
                //AuthenticatedUser, using the injected IHttpContextAccessor to get user details
                //from the HttpContext.User property (which is a ClaimsPrincipal). 
                _authenticatedUser = authenticatedUser;
            }

            public async Task<CustomizedAPIResponse<int>> Handle(CreateProductCommand createProduct, CancellationToken cancellationToken)
            {
                //this line is the AUTO-MAPPER ACTUAL MAPPING TAKES PLACED
                var product = _mapper.Map<Product>(createProduct);

                //explicitly properties mappings
                //using IAuthenticatedUser, we can get the CURRENT LOGIN USER's ID 
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
