namespace Application.Helpers
{
    public class OrderValidationError
    {
        public string OrderNumber { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}

