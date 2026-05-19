namespace TransportApp_API.DTOs.Admin.Timetable
{
    public class CreateTimetableRequest
    {
        public int StationId { get; set; }
        public string LineCode { get; set; } = null!;
        public int Hour { get; set; }
        public List<int> Minutes { get; set; } = new();
    }
}