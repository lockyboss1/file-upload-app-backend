namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; }
        public string? AlternateOrderNumber { get; set; }
        public string? OrderDate { get; set; }
        public string ShipToName { get; set; }
        public string? ShipToCompany { get; set; }
        public string ShipToAddress1 { get; set; }
        public string? ShipToAddress2 { get; set; }
        public string? ShipToAddress3 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToPostalCode { get; set; }
        public string ShipToCountry { get; set; }
        public string? ShipToPhone { get; set; }
        public string? ShipToEmail { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public string RequestedWarehouse { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? Tags { get; set; }
    }
}

