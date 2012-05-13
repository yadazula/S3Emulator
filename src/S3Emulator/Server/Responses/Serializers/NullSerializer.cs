using System.IO;

namespace S3Emulator.Server.Responses.Serializers
{
  public class NullSerializer : IS3Serializer
  {
    public void Serialize<TModel>(TModel model, Stream stream)
    {
    }
  }
}