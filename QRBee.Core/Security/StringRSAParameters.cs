using Newtonsoft.Json;

namespace QRBee.Core.Security
{
    public class StringRSAParameters
    {
        public string StringExponent { get; set; }

        public string StringModulus { get; set; }

        public string StringP { get; set; }

        public string StringQ { get; set; }

        public string StringDP { get; set; }

        public string StringDQ { get; set; }

        public string StringInverseQ { get; set; }

        public string StringD { get; set; }

        public string ConvertToJson() => JsonConvert.SerializeObject(this);

        public static StringRSAParameters ConvertFromJson(string json) => JsonConvert.DeserializeObject<StringRSAParameters>(json);
    }
}