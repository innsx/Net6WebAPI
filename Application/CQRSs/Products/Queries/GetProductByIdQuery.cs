using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CQRSs.Products.Queries
{
    //This code defines a CQRS query, likely using the MediatR library in C#,
    //to fetch a Product by ID. It returns a wrapped ApiResponse<Product> object, promoting consistent API responses (data, success status, errors). 

    //IRequest<T>: MediatR interface indicating this query returns a T (here, ApiResponse<Product>).

    //ApiResponse<T>: A wrapper class commonly used to standardize response structure,
    //including data, success status, and error messages, ensuring consistent API responses.

    //Product: The domain model representing the product being retrieved.
    public class GetProductByIdQuery : IRequest<CustomizedAPIResponse<Product>>
    {
        public int Id { get; set; }

        //IRequestHandler<TRequest, TResponse>: MediatR interface for handling the query.
        //GetProductByIdQuery: The record or class containing the Id parameter that was set.
        internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, CustomizedAPIResponse<Product>>
        {
            // Interface for database interaction to retrieve the product.
            private readonly IApplicationDbContext _context;

            public GetProductByIdQueryHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            //Handle Method: Asynchronously fetches data and wraps it in a success or failure response. 
            public async Task<CustomizedAPIResponse<Product>> Handle(GetProductByIdQuery requestProduct, CancellationToken cancellationToken)
            {
                //Asynchronous: It executes the query against the database asynchronously,
                //preventing the application thread from blocking while waiting for the database to respond.

                //cancellationToken: This allows the operation to be cancelled
                //  (e.g., if the user navigates away from a page or the request times out).
                var product = await _context.Products1!.Where(product => product.Id == requestProduct.Id).FirstOrDefaultAsync();

                if (product is null)
                {
                    //here we throw the CUSTOMIZED EXCEPTION CLASS that is CREATED RELATED TO THIS EXCEPTION TYPE
                    throw new ApiErrorExceptions($"Product with Id {requestProduct.Id} not found.");
                }

                return new CustomizedAPIResponse<Product>(product, "Data fetched successfully.");
            }
        }
    }
}
