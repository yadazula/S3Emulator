namespace S3Emulator.Server.Responses
{
  public class S3ObjectSearchRequest
  {
    public string BucketName { get; set; }
    public string Delimiter { get; set; }
    public string Marker { get; set; }
    public int? MaxKeys { get; set; }
    public string Prefix { get; set; }
  }
}