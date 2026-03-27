using FluentValidation.Results;

namespace Application.Exceptions
{
    public class ValidationErrorExceptions : Exception
    {
        public List<string>? Errors { get; set; }

        //In C#, the : base("somestring") syntax is used in a derived class constructor
        //to call a specific constructor in the base class, passing a string argument.
        //: base("somestring")  ensures the parent class is initialized with necessary data
        //before the child class executes. 
        public ValidationErrorExceptions() : base("One or More Validatons Occurred.")
        {
            //intialize Errors Property to a List of String
            Errors = new List<string>();
        }

        //Constructor Chaining: : this() (C#) or this() (Java) is used to
        //call another constructor (the 1st CONSTRUCTOR in this case)
        //of the current class from within a different constructor.
        //This helps to reuse initialization logic and avoid code duplication.

        //2nd CONSTRUCTOR created to take in a parameter of type List<ValidationFailure>
        public ValidationErrorExceptions(List<ValidationFailure> failures) : this()
        {
            foreach (var failure in failures)
            {
                Errors?.Add(failure.ErrorMessage);
            }
        }
    }
}
