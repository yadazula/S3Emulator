using System.Collections.Generic;
using S3Emulator.Model;
using S3Emulator.Server.Responses;
using S3Emulator.Server.Responses.Serializers;
using Xunit;

namespace S3Emulator.Tests.Server.Responses
{
  public class S3XmlResponderTests
  {
    [Fact]
    public void GetSerializer_Should_Return_Proper_Serializer()
    {
      var s3XmlResponder = new MockS3XmlResponder();

      var serializer = s3XmlResponder.PublicGetSerializer(new List<Bucket>());
      Assert.IsType<BucketListSerializer>(serializer);

      serializer = s3XmlResponder.PublicGetSerializer(new S3ObjectSearchResponse());
      Assert.IsType<S3ObjectSearchResponeSerializer>(serializer);
      
      serializer = s3XmlResponder.PublicGetSerializer(new BucketNotFound());
      Assert.IsType<BucketNotFoundSerializer>(serializer);

      serializer = s3XmlResponder.PublicGetSerializer(new object());
      Assert.IsType<NullSerializer>(serializer);
    }

    class MockS3XmlResponder : S3XmlResponder
    {
      public IS3Serializer PublicGetSerializer(object o)
      {
        return GetSerializer(o);
      }
    }
  }
}