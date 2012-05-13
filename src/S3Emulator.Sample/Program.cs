using System;
using System.IO;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using S3Emulator.Config;
using S3Emulator.IO;
using S3Emulator.Server;

namespace S3Emulator.Sample
{
  class Program
  {
    private const int HostPort = 8878;
    private const int ProxyPort = 8877;
    private const bool IsProxyEnabled = true;
    private const string ServiceUrl = "s3.amazonaws.com";
    private const string AwsAccessKey = "foo";
    private const string AwsSecretAccessKey = "bar";
    private const string Bucket = "bucket1";
    private const string S3ObjectKey = "key1";

    static void Main()
    {
      var s3Server = CreateS3Server();
      s3Server.Start();

      var s3Client = CreateS3Client();

      try
      {
        CreateBucket(s3Client, Bucket);

        PutObject(s3Client, Bucket, S3ObjectKey);

        ListObjects(s3Client, Bucket);

        GetObject(s3Client, Bucket, S3ObjectKey);

        DeleteObject(s3Client, Bucket, S3ObjectKey);

        DeleteBucket(s3Client, Bucket);
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
      }
      finally
      {
        s3Client.Dispose();
        s3Server.Dispose();
      }

      Console.WriteLine("Finished.");
      Console.Read();
    }

    private static S3Server CreateS3Server()
    {
      var s3Configuration = new S3Configuration
      {
        ServiceUrl = ServiceUrl,
        Host = "localhost",
        HostPort = HostPort,
        ProxyPort = ProxyPort,
        IsProxyEnabled = IsProxyEnabled,
        DataDirectory = "Data",
        RunInMemory = true,
        MaxBytesPerSecond = ThrottledStream.Infinite
      };

      var s3Server = new S3Server(s3Configuration);
      return s3Server;
    }

    private static AmazonS3 CreateS3Client()
    {
      var config = new AmazonS3Config()
                      .WithCommunicationProtocol(Protocol.HTTP)
                      .WithServiceURL(ServiceUrl + ":" + HostPort);

      var client = AWSClientFactory.CreateAmazonS3Client(AwsAccessKey, AwsSecretAccessKey, config);
      return client;
    }

    private static void CreateBucket(AmazonS3 s3Client, string bucket)
    {
      var putBucketRequest = new PutBucketRequest { BucketName = bucket };
      s3Client.PutBucket(putBucketRequest);
    }

    private static void DeleteBucket(AmazonS3 s3Client, string bucket)
    {
      var deleteBucketRequest = new DeleteBucketRequest { BucketName = bucket };
      s3Client.DeleteBucket(deleteBucketRequest);
    }

    private static void PutObject(AmazonS3 s3Client, string bucket, string key)
    {
      var putObjectRequest = new PutObjectRequest();
      putObjectRequest.WithBucketName(bucket)
                      .WithKey(key)
                      .WithContentType("text/plain")
                      .WithContentBody(key);

      var objectResponse = s3Client.PutObject(putObjectRequest);
      objectResponse.Dispose();
    }

    private static void ListObjects(AmazonS3 s3Client, string bucket)
    {
      var request = new ListObjectsRequest();
      request.WithBucketName(bucket)
             .WithPrefix("key")
             .WithMaxKeys(4);
      do
      {
        ListObjectsResponse response = s3Client.ListObjects(request);

        if (response.IsTruncated)
        {
          request.Marker = response.NextMarker;
        }
        else
        {
          request = null;
        }
      } while (request != null);
    }

    private static void GetObject(AmazonS3 s3Client, string bucket, string key)
    {
      var getObjectRequest = new GetObjectRequest().WithBucketName(bucket).WithKey(key);
      using (var getObjectResponse = s3Client.GetObject(getObjectRequest))
      {
        var memoryStream = new MemoryStream();
        getObjectResponse.ResponseStream.CopyTo(memoryStream);
        var content = Encoding.Default.GetString(memoryStream.ToArray());
        Console.WriteLine(content);
      }
    }

    private static void DeleteObject(AmazonS3 s3Client, string bucket, string key)
    {
      var deleteObjectRequest = new DeleteObjectRequest().WithBucketName(bucket).WithKey(key);
      s3Client.DeleteObject(deleteObjectRequest);
    }
  }
}
