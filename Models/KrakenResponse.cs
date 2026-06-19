namespace OctopusData.Models;

public class KrakenResponse
{
    public KrakenData data { get; set; }
}

public class KrakenData
{
    public ObtainKrakenToken obtainKrakenToken { get; set; }
}

public class ObtainKrakenToken
{
    public string token { get; set; }
    public string refreshToken { get; set; }
    public Payload payload { get; set; }
    public long refreshExpiresIn { get; set; }
}

public class Payload
{
    public string sub { get; set; }
    public string gty { get; set; }
    public string email { get; set; }
    public string tokenUse { get; set; }
    public string iss { get; set; }
    public long iat { get; set; }
    public long exp { get; set; }
    public long origIat { get; set; }
}
