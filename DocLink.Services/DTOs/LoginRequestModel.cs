using System.Text.Json.Serialization;
using DocLink.Core.Models;

namespace DocLink.Services.DTO_s;

public class LoginRequestModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    
}