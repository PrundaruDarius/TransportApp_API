namespace TransportApp_API.DTOs.Admin.Lines
{
    public class LineDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}