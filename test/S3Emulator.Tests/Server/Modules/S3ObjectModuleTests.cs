using System;
using System.IO;
using System.Text;
using Moq;
using Nancy;
using Nancy.Testing;
using S3Emulator.Model;
using S3Emulator.Server.Modules;
using S3Emulator.Server.Responses;
using Xunit;

namespace S3Emulator.Tests.Server.Modules
{
  public class S3ObjectModuleTests : AbstractModuleTests<S3ObjectModule>
  {
    [Fact]
    public void AddObject_Should_Initialize_S3Object()
    {
      S3Object s3Object = null;
      mockStorage.Setup(x => x.AddObject(It.IsAny<S3Object>()))
                 .Callback<S3Object>(x => s3Object = x)
                 .Verifiable();

      var stream = new MemoryStream(Encoding.UTF8.GetBytes("foo"));
      browser.Put("/bucket1/object1", x => x.Body(stream));

      Assert.Equal("bucket1", s3Object.Bucket);
      Assert.Equal("object1", s3Object.Key);
      Assert.True(s3Object.CreationDate > DateTime.UtcNow.AddMinutes(-1));
      Assert.Equal(stream.Length, s3Object.Content().Length);
      Assert.NotNull(s3Object.ContentMD5);
      Assert.NotNull(s3Object.ContentType);

      mockStorage.Verify();
    }

    [Fact]
    public void AddObject_Respone_Should_Include_ETag_Header()
    {
      var stream = new MemoryStream(Encoding.UTF8.GetBytes("foo"));
      var response = browser.Put("/bucket1/object1", x => x.Body(stream));
      Assert.NotNull(response.Headers["ETag"]);
    }

    [Fact]
    public void GetObject_Should_Return_StatusNotFound_If_Object_Not_Found()
    {
      mockStorage.Setup(x => x.GetObject("bucket1", "object1"))
                 .Verifiable();

      var response = browser.Get("/bucket1/object1");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

      mockStorage.Verify();
    }

    [Fact]
    public void GetObject_Response_Should_Include_ETag_And_AcceptRanges_Headers()
    {
      var stream = new MemoryStream(Encoding.UTF8.GetBytes("foo"));

      var s3Object = new S3Object
      {
        Content = () => stream,
        ContentMD5 = "ContentMD5",
        ContentType = "ContentType"
      };

      mockStorage.Setup(x => x.GetObject("bucket1", "object1"))
                 .Returns(s3Object).Verifiable();

      var response = browser.Get("/bucket1/object1");
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("ContentType", response.Context.Response.ContentType);
      Assert.NotNull(response.Headers["ETag"]);
      Assert.NotNull(response.Headers["Accept-Ranges"]);
      Assert.Equal("foo", response.Body.AsString());
    }

    [Fact]
    public void DeleteObject_Should_Return_StatusNoContent()
    {
      mockStorage.Setup(x => x.DeleteObject("bucket1", "object1"))
                 .Verifiable();

      var response = browser.Delete("/bucket1/object1");

      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

      mockStorage.Verify();
    }
  }
}