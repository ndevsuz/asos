namespace Asos.Dtos;

public class TokenResultDto
{
    public string Token { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UsedDate { get; set; }
}