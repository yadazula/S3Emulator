using System.IO;

namespace S3Emulator.Server.Responses.Serializers
{
  public interface IS3Serializer
  {
    void Serialize<TModel>(TModel model, Stream stream);
  }
}