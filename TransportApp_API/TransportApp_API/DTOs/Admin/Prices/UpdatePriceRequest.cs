namespace TransportApp_API.DTOs.Admin.Prices
{
    public class UpdatePriceRequest
    {
        public string Name { get; set; } = null!;
        public string Price { get; set; } = null!;
    }
}