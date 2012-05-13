using Nancy;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;

namespace S3Emulator.Server.Modules
{
  public class ServiceModule : NancyModule
  {
    private readonly IS3Storage storage;
    private readonly IS3Responder responder;

    public ServiceModule(IS3Storage storage, IS3Responder responder)
    {
      this.storage = storage;
      this.responder = responder;
      Get["/"] = x => ListBuckets();
    }

    private Response ListBuckets()
    {
      var bucketList = storage.GetBuckets();

      var response = responder.Respond(bucketList);
      return response;
    }
  }
}