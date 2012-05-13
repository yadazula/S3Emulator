using Nancy;

namespace S3Emulator.Server.Responses
{
  public interface IS3Responder
  {
    Response Respond<TModel>(TModel model);
  }
}