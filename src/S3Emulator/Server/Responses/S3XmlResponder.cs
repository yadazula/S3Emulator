using System;
using System.Collections.Generic;
using Nancy;
using S3Emulator.Server.Responses.Serializers;

namespace S3Emulator.Server.Responses
{
  public class S3XmlResponder : IS3Responder
  {
    private readonly IDictionary<Type, IS3Serializer> serializers;

    public S3XmlResponder(IDictionary<Type, IS3Serializer> serializers)
    {
      this.serializers = serializers;
    }

    public Response Respond<T>(T t)
    {
      var serializer = GetSerializer(t);
      var response = new Response { ContentType = "application/xml", Contents = (stream => serializer.Serialize(t, stream)) };
      return response;
    }

    public IS3Serializer GetSerializer(object o)
    {
      var type = o.GetType();
      return serializers.ContainsKey(type) ? serializers[type] : new NullSerializer();
    }
  }
}