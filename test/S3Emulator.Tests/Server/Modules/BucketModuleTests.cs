using System;
using System.Collections.Generic;
using Moq;
using Nancy;
using Nancy.Testing;
using S3Emulator.Model;
using S3Emulator.Server.Modules;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;
using Xunit;

namespace S3Emulator.Tests.Server.Modules
{
  public class BucketModuleTests : AbstractModuleTests<BucketModule>
  {
    [Fact]
    public void AddBucket_Should_Initialize_Name_And_CreationDate()
    {
      const string bucketName = "bucket1";
      Bucket bucket = null;

      mockStorage.Setup(x => x.AddBucket(It.IsAny<Bucket>()))
                 .Callback<Bucket>(x => bucket = x)
                 .Verifiable();

      browser.Put("/" + bucketName);

      Assert.Equal(bucketName, bucket.Id);
      Assert.True(bucket.CreationDate > DateTime.UtcNow.AddMinutes(-1));

      mockStorage.Verify();
    }

    [Fact]
    public void AddBucket_Should_Return_StatusOK()
    {
      var response = browser.Put("/bucket1");

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void DeleteBucket_Should_Return_StatusNoContent()
    {
      const string bucketName = "bucket1";

      mockStorage.Setup(x => x.DeleteBucket(bucketName))
                 .Verifiable();

      var response = browser.Delete("/" + bucketName);

      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

      mockStorage.Verify();
    }

    [Fact]
    public void HeadBucket_Should_Return_StatusOK_If_Bucket_Exist()
    {
      mockStorage.Setup(x => x.GetBucket("bucket1"))
                 .Returns(new Bucket())
                 .Verifiable();

      var response = browser.Head("/bucket1");

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      mockStorage.Verify();
    }

    [Fact]
    public void HeadBucket_Should_Return_StatusNotFound_If_Bucket_Not_Exist()
    {
      mockStorage.Setup(x => x.GetBucket("bucket1"))
                 .Verifiable();

      mockResponder.Setup(x => x.Respond(It.IsAny<BucketNotFound>()))
                   .Returns(new Response())
                   .Verifiable();

      var response = browser.Head("/bucket1");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

      mockStorage.Verify();
      mockResponder.Verify();
    }

    [Fact]
    public void ListObjects_Should_Map_Query_Parameters()
    {
      S3ObjectSearchRequest searchRequest = null;

      mockStorage.Setup(x => x.GetBucket("bucket1"))
                 .Returns(new Bucket())
                 .Verifiable();

      mockStorage.Setup(x => x.GetObjects(It.IsAny<S3ObjectSearchRequest>()))
                 .Returns(new S3ObjectSearchResponse())
                 .Callback<S3ObjectSearchRequest>(x => searchRequest = x)
                 .Verifiable();

      mockResponder.Setup(x => x.Respond(It.IsAny<S3ObjectSearchResponse>()))
             .Returns(new Response())
             .Verifiable();

      browser.Get("/bucket1", x =>
      {
        x.Query("prefix", "p");
        x.Query("delimiter", "d");
        x.Query("marker", "m");
        x.Query("max-keys", "10");
      });

      Assert.Equal("p", searchRequest.Prefix);
      Assert.Equal("d", searchRequest.Delimiter);
      Assert.Equal("m", searchRequest.Marker);
      Assert.Equal(10, searchRequest.MaxKeys);

      mockStorage.Verify();
      mockResponder.Verify();
    }

    [Fact]
    public void ListObjects_Should_Return_StatusNotFound_If_Bucket_Does_Not_Exist()
    {
      mockStorage.Setup(x => x.GetBucket("bucket1"))
        .Verifiable();

      mockResponder.Setup(x => x.Respond(It.IsAny<BucketNotFound>()))
        .Returns(new Response())
        .Verifiable();

      var response = browser.Get("/bucket1");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

      mockStorage.Verify();
      mockResponder.Verify();
    }
  }
}