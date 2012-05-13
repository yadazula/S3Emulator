using System.Collections.Generic;
using Nancy;
using S3Emulator.Model;
using S3Emulator.Server.Responses.Serializers;

namespace S3Emulator.Server.Responses
{
  public class S3XmlResponder : IS3Responder
  {
    public Response Respond<T>(T t)
    {
      var serializer = GetSerializer(t);
      var response = new Response { ContentType = "application/xml", Contents = (stream => serializer.Serialize(t, stream)) };
      return response;
    }

    protected IS3Serializer GetSerializer(object o)
    {
      if (o is IEnumerable<Bucket>)
        return new BucketListSerializer();

      if (o is S3ObjectSearchResponse)
        return new S3ObjectSearchResponeSerializer();

      if (o is BucketNotFound)
        return new BucketNotFoundSerializer();

      return new NullSerializer();
    }
  }
}