using System;
using System.IO;
using Nancy;
using S3Emulator.Config;
using S3Emulator.IO;
using S3Emulator.Model;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;

namespace S3Emulator.Server.Modules
{
  public class S3ObjectModule : NancyModule
  {
    private readonly S3Configuration configuration;
    private readonly IS3Storage storage;

    public S3ObjectModule(S3Configuration configuration, IS3Storage storage, IS3Responder responder)
    {
      this.configuration = configuration;
      this.storage = storage;

      Get["/{bucket}/{key}"] = x => GetObject(x.bucket, x.key);
      Put["/{bucket}/{key}"] = x => AddObject(x.bucket, x.key, Request.Body);
      Delete["/{bucket}/{key}"] = x => DeleteObject(x.bucket, x.key);
    }

    private Response AddObject(string bucket, string key, Stream stream)
    {
      var content = stream.Copy(configuration.MaxBytesPerSecond);

      var s3Object = new S3Object
      {
        Bucket = bucket,
        Key = key,
        ContentType = Request.Headers.ContentType,
        CreationDate = DateTime.UtcNow,
        Content = () => content,
        ContentMD5 = content.GenerateMD5CheckSum(),
        Size = content.Length
      };

      storage.AddObject(s3Object);

      var response = new Response { StatusCode = HttpStatusCode.OK };
      response.WithHeader("ETag", string.Format("\"{0}\"", s3Object.ContentMD5));
      return response;
    }

    private Response GetObject(string bucket, string key)
    {
      var s3Object = storage.GetObject(bucket, key);
      if (s3Object == null)
      {
        return new Response { StatusCode = HttpStatusCode.NotFound };
      }

      var stream = s3Object.Content();

      var response = new Response { StatusCode = HttpStatusCode.OK, ContentType = s3Object.ContentType };
      response.WithHeader("ETag", string.Format("\"{0}\"", s3Object.ContentMD5));
      response.WithHeader("Accept-Ranges", "bytes");
      response.Contents = x =>
      {
        var throttledStream = new ThrottledStream(stream, configuration.MaxBytesPerSecond);
        throttledStream.CopyTo(x);
      };

      return response;
    }

    private Response DeleteObject(string bucket, string key)
    {
      storage.DeleteObject(bucket, key);

      var response = new Response { StatusCode = HttpStatusCode.NoContent };
      return response;
    }
  }
}