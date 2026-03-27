using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ApiErrorExceptions : Exception
    {
        //public Api Error Exception(string error Message): base(error Message)
        //demonstrates the definition of a custom exception class in C#.
        //This pattern is a standard practice for creating specialized exceptions that inherit from a base exception class,
        //typically System.Exception or a more specific one like System.ApplicationException,
        //allowing for meaningful, specific error handling within a software application or API.

        //: base(error Message): This is a constructor initializer.
        //It calls the constructor of the base class (indicated by base)
        //and passes the errorMessage string to it.
        //The base class, which is usually System.Exception,
        //handles the internal storage and management of the error message. 
        public ApiErrorExceptions(string errorMessage): base(errorMessage)
        {
            
        }
    }
}
