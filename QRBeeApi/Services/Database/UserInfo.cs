namespace QRBee.Api.Services;

public record UserInfo
{
    public UserInfo()
    {

    }

    public UserInfo(string name, string email, string dateOfBirth)
    {
        Name = name;
        Email = email;
        DateOfBirth = dateOfBirth;
    }

    public string? Name
    {
        get;
        set;
    }

    public string? Email
    {
        get;
        set;
    }

    public string? DateOfBirth
    {
        get;
        set;
    }

    public bool RegisterAsMerchant
    {
        get;
        set;
    }
}