namespace Asos.Models;

public class TokenDetails
{
    public string Token { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UsedDate { get; set; }

}