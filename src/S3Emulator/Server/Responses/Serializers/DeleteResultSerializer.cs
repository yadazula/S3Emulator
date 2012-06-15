namespace S3Emulator.Server.Responses.Serializers
{
  public class DeleteResultSerializer : AbstractS3Serializer<DeleteRequest>
  {
    protected override string SerializeInternal(DeleteRequest model)
    {
      dynamic builder = new DynamicXmlBuilder();
      builder.Declaration();
      builder.DeleteResult(new { xmlns = "http://s3.amazonaws.com/doc/2006-03-01/" }, DynamicXmlBuilder.Fragment(deleteResult =>
      {
        deleteResult.Deleted(DynamicXmlBuilder.Fragment(deleted =>
        {
          deleted.Key(model.Object.Key);
        }));

      }));

      var result = builder.ToString(false);
      return result;
    }
  }
}