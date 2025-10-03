namespace DocLink.Services.DTO_s;

public class RegistrationResponseModel
{
    public bool IsSuccessful { get; set; }
    public IEnumerable<string> Errors { get; set; }
}