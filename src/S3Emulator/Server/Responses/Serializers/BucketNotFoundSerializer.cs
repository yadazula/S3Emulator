namespace S3Emulator.Server.Responses.Serializers
{
  public class BucketNotFoundSerializer : AbstractS3Serializer<BucketNotFound>
  {
    protected override string SerializeInternal(BucketNotFound bucketNotFound)
    {
      dynamic builder = new DynamicXmlBuilder();
      builder.Declaration();
      builder.Error(DynamicXmlBuilder.Fragment(error =>
      {
        error.Code("NoSuchBucket");
        error.Message("The specified bucket does not exist");
        error.Resource(bucketNotFound.BucketName);
        error.RequestId(1);
        error.HostId(1);
      }));

      var responseBody = builder.ToString(false);
      return responseBody;
    }
  }
}