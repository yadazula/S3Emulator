using System;
using System.Collections.Generic;
using S3Emulator.Model;
using S3Emulator.Server;
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
      var s3XmlResponder = (S3XmlResponder)Bootstrapper.BuildResponder();

      var serializer = s3XmlResponder.GetSerializer(new List<Bucket>());
      Assert.IsType<BucketListSerializer>(serializer);

      serializer = s3XmlResponder.GetSerializer(new S3ObjectSearchResponse());
      Assert.IsType<S3ObjectSearchSerializer>(serializer);

      serializer = s3XmlResponder.GetSerializer(new BucketNotFound());
      Assert.IsType<BucketNotFoundSerializer>(serializer);

      serializer = s3XmlResponder.GetSerializer(new DeleteRequest());
      Assert.IsType<DeleteResultSerializer>(serializer);

      serializer = s3XmlResponder.GetSerializer(new object());
      Assert.IsType<NullSerializer>(serializer);
    }
  }
}