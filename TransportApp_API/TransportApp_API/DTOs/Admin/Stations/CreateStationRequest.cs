namespace TransportApp_API.DTOs.Admin.Stations
{
    public class CreateStationRequest
    {
        public int LineId { get; set; }
        public string Name { get; set; } = null!;
        public int Order { get; set; }
    }
}