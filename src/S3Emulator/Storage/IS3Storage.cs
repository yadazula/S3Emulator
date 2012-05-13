using System.Collections.Generic;
using S3Emulator.Model;
using S3Emulator.Server.Responses;

namespace S3Emulator.Storage
{
  public interface IS3Storage
  {
    void AddBucket(Bucket bucket);
    IEnumerable<Bucket> GetBuckets();
    void DeleteBucket(string bucket);
    Bucket GetBucket(string bucket);
    S3ObjectSearchResponse GetObjects(S3ObjectSearchRequest searchRequest);
    void AddObject(S3Object s3Object);
    void DeleteObject(string bucket, string key);
    S3Object GetObject(string bucket, string key);
  }
}