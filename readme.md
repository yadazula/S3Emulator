S3Emulator
==============
[http://github.com/yadazula/S3Emulator][0]  

S3Emulator is a lightweight server which mimics the services of Amazon S3. It can be useful for development and testing porpuses. 
By reducing network traffic, it saves both the time and the money.

Supported Operations
--------------------
- [GET Service][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTServiceGET.html]  
- [PUT Bucket][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTBucketPUT.html]  
- [DELETE Bucket][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTBucketDELETE.html]  
- [HEAD Bucket][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTBucketHEAD.html]  
- [GET Bucket(List Objects)][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTBucketGET.html]  
- [PUT Object][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTObjectPUT.html]  
- [GET Object][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTObjectGET.html]  
- [DELETE Object][http://docs.amazonwebservices.com/AmazonS3/latest/API/RESTObjectDELETE.html]  

How to use it ?
---------------
Download the application from [here][1]. Open a command promt window and just enter "S3Emulator"
When started with default options, all the requests made to "s3.amazonaws.com" will be redirected to S3Emulator.
You can see the full list of options by entering : "S3Emulator -help"

Options
-------
- Service  
  Address of s3 service that will be emulated.
  Default: s3.amazonaws.com

- Host  
  The hostname to use for the http listener.
  Default: localhost

- HostPort  
  The port to use for the http listener.
  Default: 8878

- EnableProxy  
  Proxy is used for redirecting requests to S3Emulator's http listener and supporting [subdomain style bucket names][2]. 
  If you disable proxy, you need to use Request-URI syle bucket names. 
  [FiddlerCore][3] is used for proxy operations.
  Default: true
	
- ProxyPort  
  The port to use for the proxy.
  Default: 8877

- Directory  
  The directory for the storage operations. 
  [RavenDB][4] is used for persistance.
  Default: ~\Data

- InMemory  
  If set to true, all storage operations will be in memory.
  Default: false

- MaxBPS  
  Set maximum bytes per second. Can be used for bandwidth throttling.
  Default: infinite

[0]: http://github.com/yadazula/S3Emulator "S3Emulator on Github"
[1]: http://github.com/yadazula/S3Emulator/downloads "download"
[2]: http://docs.amazonwebservices.com/AmazonS3/latest/dev/VirtualHosting.html#VirtualHostingSpecifyBucket
[3]: http://www.fiddler2.com/Fiddler/Core/
[4]: http://ravendb.net