using System.Collections.Generic;
using S3Emulator.Model;

namespace S3Emulator.Server.Responses
{
  public class S3ObjectSearchResponse
  {
    public string BucketName { get; set; }
    public string Delimiter { get; set; }
    public string Marker { get; set; }
    public int? MaxKeys { get; set; }
    public string Prefix { get; set; }
    public bool IsTruncated { get; set; }
    public IList<S3Object> S3Objects { get; set; }
    public IList<string> Prefixes { get; set; }
  }
}