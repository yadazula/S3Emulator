namespace S3Emulator.Server.Responses.Serializers
{
  public class ACLSerializer : AbstractS3Serializer<ACLRequest>
  {
    protected override string SerializeInternal(ACLRequest model)
    {
      return
        @"<?xml version='1.0'?>
          <AccessControlPolicy xmlns='http://s3.amazonaws.com/doc/2006-03-01/'>
            <Owner>
              <ID>id</ID>
              <DisplayName>name</DisplayName>
            </Owner>
            <AccessControlList>
              <Grant>
                <Grantee xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:type='CanonicalUser'>
                  <ID>id</ID>
                  <DisplayName>name</DisplayName>
                </Grantee>
                <Permission>FULL_CONTROL</Permission>
              </Grant>
            </AccessControlList>
          </AccessControlPolicy>";
    }
  }
}