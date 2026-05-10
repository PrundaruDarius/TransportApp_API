namespace TransportApp_API.DTOs.Admin.Timetable
{
    public class UpdateTimetableRequest
    {
        public List<int> Minutes { get; set; } = new();
        public bool IsActive { get; set; }
    }
}