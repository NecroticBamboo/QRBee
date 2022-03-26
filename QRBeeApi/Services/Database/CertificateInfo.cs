using MongoDB.Bson.Serialization.Attributes;

namespace QRBee.Api.Services.Database
{
    public class CertificateInfo
    {

        [BsonId] public string? Id      { get; set; }
        public string? ClientId         { get; set; }
        public string? Email            { get; set; }
        public string? Certificate      { get; set; }
        public DateTime ServerTimeStamp { get; set; }
    }
}
