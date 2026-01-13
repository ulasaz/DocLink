using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using DocLink.Core.Models;

namespace DocLink.Services.DTO_s;

public class LoginResponseModel
{
    public string? Token { get; set; }
    public bool IsSuccessful { get; set; }
    public IEnumerable<string> Errors { get; set; }
        
}