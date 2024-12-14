namespace Proj_ICFT.Models
{
    public class TokenJson
    {

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

        public string location { get; set; }
        public object value { get; set; }
        public int statusCode { get; set; }
        public object contentType { get; set; }
    }
}
