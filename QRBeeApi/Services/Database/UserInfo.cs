using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QRBee.Api.Services.Database;

public record UserInfo
{
#pragma warning disable CS8618
    public UserInfo()
#pragma warning restore CS8618
    {

    }

    public UserInfo(string name, string email, string dateOfBirth)
    {
        Name        = name;
        Email       = email;
        DateOfBirth = dateOfBirth;
    }

    /// <summary>
    /// Never use directly. Use <see cref="ClientId"/> instead.
    /// </summary>
    [BsonId] public ObjectId Id { get; set; }

    [BsonIgnore] public string? ClientId => Id == ObjectId.Empty ? null :  Id.ToString();
    public string Name             { get; set; }
    public string Email            { get; set; }
    public string DateOfBirth      { get; set; }
    public bool RegisterAsMerchant { get; set; }
}