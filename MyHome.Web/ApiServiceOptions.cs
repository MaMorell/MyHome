using System.ComponentModel.DataAnnotations;

namespace MyHome.Web;

public class ApiServiceOptions
{
    public const string Name = "ApiService";

    [Required]
    public required Uri BaseUrl { get; set; }
}
