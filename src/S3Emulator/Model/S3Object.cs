using System;
using System.IO;
using Newtonsoft.Json;

namespace S3Emulator.Model
{
  public class S3Object
  {
    private string bucket;
    private string key;

    public string Id { get; set; }
    public string ContentMD5 { get; set; }
    public string ContentType { get; set; }
    public DateTime CreationDate { get; set; }
    public long Size { get; set; }

    [JsonIgnore]
    public Func<Stream> Content { get; set; }

    public string Key
    {
      get { return key; }
      set
      {
        key = value;
        Id = bucket + "/" + key;
      }
    }

    public string Bucket
    {
      get { return bucket; }
      set
      {
        bucket = value;
        Id = bucket + "/" + key;
      }
    }
  }
}