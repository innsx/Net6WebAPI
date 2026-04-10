using Application.CQRSs.Products.Commands;
using Application.CQRSs.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    //Video: https://www.youtube.com/watch?v=-TIMkYGO9RU&list=PLyTjFFFANHHfPsdxw_BX5IxK0Ayk-fCWv&index=8

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //sends a command or query object to a corresponding handler using the MediatR library
        //in a .NET application

        //Send(...): This is an asynchronous method provided by MediatR used to dispatch a request.
        //For a given request, MediatR ensures it is handled by exactly one handler.

        //cancellationToken: This parameter enables cooperative cancellation of the asynchronous operation.
        //It is a standard practice in modern .NET to pass
        //and respect cancellation tokens in asynchronous methods. DEFAULT value is "false"

        //await: The await keyword pauses the execution of the current method
        //until the asynchronous operation started by _mediator.Send(...) completes.
        [HttpGet("products")]
        //You use [ProducesResponseType] for each possible path to tell the consumer what to expect in different scenarios.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(products);
        }

        //[Authorize(Roles ="User")]
        //[Authorize(Roles ="Basic")]
        [HttpGet("product/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProductById(int id) 
        {
            var product = await _mediator.Send(new GetProductByIdQuery { Id = id});
            return Ok(product);
        }


        [HttpPost("product")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct(CreateProductCommand createProduct, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(createProduct, cancellationToken);
            return Ok(result);
        }

        [HttpPut("product")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(UpdateProductCommand updateProduct, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(updateProduct, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("product/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id }, cancellationToken);
            return Ok(result);
        }
    }
}


