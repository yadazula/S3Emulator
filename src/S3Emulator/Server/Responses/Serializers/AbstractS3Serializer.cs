using System.IO;

namespace S3Emulator.Server.Responses.Serializers
{
  public abstract class AbstractS3Serializer<TModel> : IS3Serializer
  {
    void IS3Serializer.Serialize<T>(T t, Stream stream)
    {
      var model = (TModel)(object)t;
      var response = SerializeInternal(model);
      var sw = new StreamWriter(stream);
      sw.Write(response);
      sw.Flush();
    }

    protected abstract string SerializeInternal(TModel model);
  }
}