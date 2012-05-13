using System;
using Nancy;
using S3Emulator.Model;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;

namespace S3Emulator.Server.Modules
{
  public class BucketModule : NancyModule
  {
    private readonly IS3Storage storage;
    private readonly IS3Responder responder;

    public BucketModule(IS3Storage storage, IS3Responder responder)
    {
      this.storage = storage;
      this.responder = responder;

      Get["/{bucket}"] = x => GetBucket(x.bucket, Request.Method);
      Put["/{bucket}"] = x => AddBucket(x.bucket);
      Delete["/{bucket}"] = x => DeleteBucket(x.bucket);
    }

    private Response AddBucket(string bucketName)
    {
      var bucket = new Bucket { Id = bucketName, CreationDate = DateTime.UtcNow };
      storage.AddBucket(bucket);

      var response = new Response { StatusCode = HttpStatusCode.OK };
      return response;
    }

    private Response DeleteBucket(string bucket)
    {
      storage.DeleteBucket(bucket);

      var response = new Response { StatusCode = HttpStatusCode.NoContent };
      return response;
    }

    private Response GetBucket(string bucket, string method)
    {
      if (method.ToUpperInvariant() == "HEAD")
      {
        return CheckBucketExist(bucket);
      }

      return ListObjects(bucket);
    }

    private Response CheckBucketExist(string bucket)
    {
      var bucketObject = storage.GetBucket(bucket);
      if (bucketObject == null)
      {
        var responseNotFound = responder.Respond(new BucketNotFound { BucketName = bucket });
        responseNotFound.StatusCode = HttpStatusCode.NotFound;
        return responseNotFound;
      }

      var response = new Response { StatusCode = HttpStatusCode.OK };
      return response;
    }

    private Response ListObjects(string bucket)
    {
      var bucketObject = storage.GetBucket(bucket);
      if (bucketObject == null)
      {
        var responseNotFound = responder.Respond(new BucketNotFound { BucketName = bucket });
        responseNotFound.StatusCode = HttpStatusCode.NotFound;
        return responseNotFound;
      }

      var searchRequest = new S3ObjectSearchRequest
      {
        BucketName = bucket,
        Prefix = Request.Query.prefix.HasValue ? Request.Query.prefix : string.Empty,
        Delimiter = Request.Query.delimiter.HasValue ? Request.Query.delimiter : string.Empty,
        Marker = Request.Query.marker.HasValue ? Request.Query.marker : string.Empty,
        MaxKeys = Request.Query.maxkeys.HasValue ? Request.Query.maxkeys : 1000,
      };

      var searchResponse = storage.GetObjects(searchRequest);
      var response = responder.Respond(searchResponse);
      return response;
    }
  }
}