namespace CrexiService.Models
{
    public class CommercialListing
    {
        public int ListingId { get; set; }
        public required string Title { get; set; }
        public int Price { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public string? BrokerName { get; set; }
    }
}
