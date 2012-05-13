using System;
using System.IO;
using System.Linq;
using Raven.Client.Embedded;
using Raven.Json.Linq;
using S3Emulator.IO;
using S3Emulator.Model;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;
using S3Emulator.Storage.Indexes;
using Xunit;

namespace S3Emulator.Tests.Storage
{
  public class RavenDBStorageTests
  {
    [Fact]
    public void Should_Add_Bucket()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        var storage = new RavenDBStorage(documentStore);
        storage.AddBucket(new Bucket { Id = "bucket1", CreationDate = DateTime.UtcNow });

        using (var documentSession = documentStore.OpenSession())
        {
          var buckets = documentSession.Query<Bucket>().ToList();
          Assert.Equal(1, buckets.Count);
        }
      }
    }

    [Fact]
    public void Should_Get_Bucket_By_Key()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        using (var session = documentStore.OpenSession())
        {
          session.Store(new Bucket { Id = "bucket1" });
          session.SaveChanges();
        }

        var storage = new RavenDBStorage(documentStore);
        var bucket = storage.GetBucket("bucket1");
        Assert.NotNull(bucket);
        Assert.Equal("bucket1", bucket.Id);
      }
    }

    [Fact]
    public void Should_Delete_Bucket()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        using (var session = documentStore.OpenSession())
        {
          session.Store(new Bucket { Id = "bucket1" });
          session.SaveChanges();
        }

        var storage = new RavenDBStorage(documentStore);
        storage.DeleteBucket("bucket1");

        using (var documentSession = documentStore.OpenSession())
        {
          var bucket = documentSession.Load<Bucket>("bucket1");
          Assert.Null(bucket);
        }
      }
    }

    [Fact]
    public void Should_Get_Bucket_List()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        using (var session = documentStore.OpenSession())
        {
          session.Store(new Bucket { Id = "bucket1" });
          session.SaveChanges();
        }

        var storage = new RavenDBStorage(documentStore);
        var buckets = storage.GetBuckets();
        Assert.Equal(1, buckets.Count());
      }
    }

    [Fact]
    public void Should_Add_Content_As_Attachment()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        var storage = new RavenDBStorage(documentStore);

        var s3Object = new S3Object
        {
          Bucket = "bucket1",
          Key = "key1",
          ContentMD5 = "md5",
          ContentType = "text",
          Content = () => new MemoryStream(new byte[] { 1, 2, 3 })
        };

        storage.AddObject(s3Object);

        using (var documentSession = documentStore.OpenSession())
        {
          var s3Objects = documentSession.Query<S3Object>().ToList();
          Assert.Equal(1, s3Objects.Count);
          Assert.Null(s3Objects[0].Content);

          var attachment = documentSession.Advanced.DatabaseCommands.GetAttachment(s3Object.Id);
          Assert.NotNull(attachment);
          Assert.Equal(s3Object.Content().GenerateMD5CheckSum(), attachment.Data().GenerateMD5CheckSum());
        }
      }
    }

    [Fact]
    public void Should_Get_S3Object_By_Key()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        using (var session = documentStore.OpenSession())
        {
          var s3Object = new S3Object
          {
            Bucket = "bucket1",
            Key = "key1",
            ContentMD5 = "md5",
            ContentType = "text",
            Content = () => new MemoryStream(new byte[] { 1, 2, 3 })
          };
          session.Advanced.DatabaseCommands.PutAttachment(s3Object.Id, null, s3Object.Content(), new RavenJObject());
          session.Store(s3Object);
          session.SaveChanges();
        }

        var storage = new RavenDBStorage(documentStore);
        var s3Object2 = storage.GetObject("bucket1", "key1");
        Assert.NotNull(s3Object2);
      }
    }

    [Fact]
    public void Should_Delete_S3Object()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        using (var session = documentStore.OpenSession())
        {
          var s3Object = new S3Object
          {
            Bucket = "bucket1",
            Key = "key1",
            ContentMD5 = "md5",
            ContentType = "text",
            Content = () => new MemoryStream(new byte[] { 1, 2, 3 })
          };
          session.Advanced.DatabaseCommands.PutAttachment(s3Object.Id, null, s3Object.Content(), new RavenJObject());
          session.Store(s3Object);
          session.SaveChanges();
        }

        var storage = new RavenDBStorage(documentStore);
        storage.DeleteObject("bucket1", "key1");

        using (var documentSession = documentStore.OpenSession())
        {
          var s3Object = documentSession.Load<S3Object>("bucket1/key1");
          Assert.Null(s3Object);

          var attachment = documentSession.Advanced.DatabaseCommands.GetAttachment("bucket1/key1");
          Assert.Null(attachment);
        }
      }
    }

    [Fact]
    public void Should_List_S3Objects_By_Prefix()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(S3Object_Search).Assembly, documentStore);

        var storage = new RavenDBStorage(documentStore);

        storage.AddBucket(new Bucket { Id = "Bucket1", CreationDate = DateTime.UtcNow });

        storage.AddObject(GetS3Object("Bucket1", "Photo/1.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/2.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/3.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/January/4.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/January/5.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/February/6.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/February/7.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/March/8.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "Photo/March/9.jpg"));

        var searchResponse = storage.GetObjects(new S3ObjectSearchRequest { BucketName = "Bucket1", Prefix = "Photo/January/" });
        Assert.Equal(2, searchResponse.S3Objects.Count());
        Assert.Equal("Photo/January/4.jpg", searchResponse.S3Objects.ElementAt(0).Key);
        Assert.Equal("Photo/January/5.jpg", searchResponse.S3Objects.ElementAt(1).Key);
      }
    }

    [Fact]
    public void Should_List_S3Objects_By_MaxKeys()
    {
      using (var documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize())
      {
        Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(S3Object_Search).Assembly, documentStore);

        var storage = new RavenDBStorage(documentStore);

        storage.AddBucket(new Bucket { Id = "Bucket1", CreationDate = DateTime.UtcNow });

        storage.AddObject(GetS3Object("Bucket1", "1.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "2.jpg"));
        storage.AddObject(GetS3Object("Bucket1", "3.jpg"));

        var searchResponse = storage.GetObjects(new S3ObjectSearchRequest { BucketName = "Bucket1", MaxKeys = 2 });
        Assert.Equal(2, searchResponse.S3Objects.Count());
        Assert.Equal("1.jpg", searchResponse.S3Objects.ElementAt(0).Key);
        Assert.Equal("2.jpg", searchResponse.S3Objects.ElementAt(1).Key);
      }
    }

    private static S3Object GetS3Object(string bucket, string key)
    {
      return new S3Object
               {
                 Bucket = bucket,
                 Key = key,
                 CreationDate = DateTime.UtcNow,
                 Content = () => new MemoryStream(new byte[] { 1, 2, 3 })
               };
    }
  }
}