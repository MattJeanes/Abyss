namespace Abyss.Web.Data
{
    public class JwtOptions
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public int ValidMinutes { get; set; }
    }
}
