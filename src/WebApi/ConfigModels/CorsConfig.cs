namespace Sindibad.SAD.WebTemplate.WebApi.ConfigModels;
public class CorsConfig
{
    public required bool Enabled { get; set; }

    public required string[] AllowedOrigins { get; set; }
    public required string[] AllowedMethods { get; set; }
    public required string[] AllowedHeaders { get; set; }
}
