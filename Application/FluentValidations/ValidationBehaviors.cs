using Application.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.FluentValidations
{

    //This code implements a generic ValidationBehavior class for MediatR in C# using FluentValidation.
    //It acts as a middleware pipeline to validate requests before they reach the command/query handler,
    //throwing a ValidationException if validation fails.
    //It requires IEnumerable<IValidator<TRequest>> to be injected.

    //IPipelineBehavior<TRequest, TResponse>: Defines this class as a middleware component in the MediatR pipeline.

    //where TRequest : IRequest<TResponse>:
    //Ensures this behavior only applies to types that implement IRequest<TResponse>, providing type safety.

    //IValidator<TRequest>: The FluentValidation interface used to validate the request object.
    public class ValidationBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validator;

        //IEnumerable<IValidator<TRequest>> is injected.
        public ValidationBehaviors(IEnumerable<IValidator<TRequest>> validator)
        {
            _validator = validator;
        }

        //The code snippet you provided is the signature for the Handle method
        //within the MediatR library's IPipelineBehavior<TRequest, TResponse> interface.
        //This Handle method is the core of a pipeline behavior,
        //which allows for cross-cutting concerns to be executed around the primary request handler
        //in a CQRS (Command Query Responsibility Segregation) pattern implementation.

        //TRequest request: The incoming command or query object being processed by MediatR.

        //RequestHandlerDelegate<TResponse> next: A delegate that, when awaited (await next()),
        //calls the next behavior in the pipeline or, eventually, the final request handler itself.

        //CancellationToken cancellationToken: A mechanism for cooperative cancellation,
        //allowing the operation to be stopped if a cancellation is requested from an external source
        //(e.g., a user closing a web browser).

        //Task<TResponse>: The method returns a Task that represents the asynchronous operation,
        //eventually yielding a response of type TResponse.
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            //PRE-processing logic such as Logging, Validation go here
            if (_validator.Any())
            {
                // is commonly used within a MediatR pipeline behavior
                // to initiate validation of a request object using the FluentValidation library.
                // Creates a Validation Context: It creates an instance of FluentValidation.ValidationContext<TRequest>,
                // which acts as a container for the request object that needs to be validated.

                //Enables FluentValidation: This context is then passed to all
                //registered validators (IValidator<TRequest>) for the specific TRequest type.

                //Centralizes Validation Logic: The code is part of a ValidationBehavior class that acts
                //as a middleware in the MediatR pipeline.
                //This centralizes validation logic, ensuring all incoming requests are automatically validated
                //before they reach their corresponding handlers.
                //This keeps the business logic handlers clean and focused on their primary responsibilities.
                var validationContext = new ValidationContext<TRequest>(request);


                //is a standard pattern used in C# asynchronous programming to execute multiple,
                //independent validation tasks concurrently and wait for all of them to complete.

                //Task.WhenAll(...): This method takes the collection of tasks and
                //returns a single Task that will complete only when all of the individual validation tasks have finished.
                //This allows the validations to run in parallel, which can significantly improve performance compared
                //to running them sequentially.

                //_validator.Select(...): This iterates over a collection of _validator instances
                //(likely injected via dependency injection) and transforms each one into a Task representing
                //an asynchronous validation operation using v.ValidateAsync(...).

                //cancellationToken: The cancellationToken is passed to each ValidateAsync call and to Task.WhenAll.
                //This allows external code to signal a request for cancellation of the entire operation.
                //If cancellation is requested and any task throws a TaskCanceledException,
                //the aggregate task will also reflect the cancellation.

                //After this line executes, the results are typically processed to check for any failures:
                var validationResultTasks = _validator
                                    .Select(v => v.ValidateAsync(validationContext, cancellationToken));


                // is a C# asynchronous pattern that concurrently executes a collection of tasks (validationResultTasks)
                // and pauses the method execution until all tasks complete.
                // It efficiently aggregates the results into an array (T[]) without blocking the calling thread

                //Exception Handling: If multiple tasks fail, Task.WhenAll only throws the exception
                //from the first failed task, though all exceptions can be inspected.

                //Avoid .Result: Use await with WhenAll instead of .Result or .Wait() to prevent thread blocking and potential deadlocks.

                //WHEN to use Task.WhenAll: when tasks are independent and performance matters.

                //Task.WhenAll is a powerful tool for running independent async operations concurrently in .NET.
                //It can drastically reduce execution time compared to sequential execution
                var validationResults = await Task.WhenAll(validationResultTasks);

                //The code then usually aggregates the ValidationResult objects returned by each ValidateAsync call.
                //Any validation failures are extracted into a list.
                var errors = validationResults                                                
                            .SelectMany(r => r.Errors)                                                
                            .Where(r => r != null)                                                
                            .ToList();

                //If failures exist, a custom ValidationException is typically thrown,
                //which stops the request from reaching its handler.
                if (errors.Any())
                {
                    //In C#, the ValidationException class is a CUSTOMIZED CLASS.
                    throw new ValidationErrorExceptions(errors);
                }
            }

            //If there are no failures, the pipeline continues to the next step,
            //ultimately reaching the main request handler. 
            var response = await next();

            //POST-processing logic such as Responses, modification go here
            return response;
        }
    }
}
