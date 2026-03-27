namespace Application.DTOs
{
    public class EmailRequestDto
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; } = null;
        public bool IsHtmlBody { get; set; } 
    }
}
