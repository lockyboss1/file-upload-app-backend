using Application.Helpers;

namespace Application.Exceptions
{
    public class CustomValidationException : Exception
    {
        public IEnumerable<OrderValidationError> Errors { get; }

        public CustomValidationException(IEnumerable<OrderValidationError> errors)
            : base("Validation errors occurred.")
        {
            Errors = errors;
        }
    }
}

