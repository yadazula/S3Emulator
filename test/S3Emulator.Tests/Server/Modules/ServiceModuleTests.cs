using System.Collections.Generic;
using Moq;
using Nancy;
using S3Emulator.Model;
using S3Emulator.Server.Modules;
using Xunit;

namespace S3Emulator.Tests.Server.Modules
{
  public class ServiceModuleTests : AbstractModuleTests<ServiceModule>
  {
    [Fact]
    public void ListBuckets_Should_Return_StatusOK()
    {
      mockStorage.Setup(x => x.GetBuckets())
                 .Returns(new List<Bucket>())
                 .Verifiable();

      mockResponder.Setup(x => x.Respond(It.IsAny<IEnumerable<Bucket>>()))
                   .Returns(new Response { StatusCode = HttpStatusCode.OK })
                   .Verifiable();

      var response = browser.Get("/");

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      mockStorage.Verify();
      mockResponder.Verify();
    }
  }
}