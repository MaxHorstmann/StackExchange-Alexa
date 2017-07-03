using System;
using Xunit;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace StackExchange.Alexa.Tests
{
    public class AllTests
    {
        [Fact]
        public async Task TestUpvoteIntent()
        {
        	var input = @"
{
  ""session"": {
    ""sessionId"": ""SessionId.777c70f4-33ec-4b74-a1bb-b6c35d916307"",
    ""application"": {
      ""applicationId"": ""amzn1.ask.skill.03d9d112-9807-48d0-bbea-4715817a5448""
    },
    ""attributes"": {
      ""site"": ""scifi"",
      ""question_id"": 162694
    },
    ""user"": {
      ""userId"": ""amzn1.ask.account.AE7FDDQ443BXMTB23PLKDBVHXPBPYOVCTES56QDWBN6XXSLFBTR72ZTSUUARWS4V2ITIHO6EMXE6GQT6PVHNODA5EDRLHFB3QPTVMCALMYYO5RVGWSFWXUFT4NBXVPRBM2DVA3HFU5TTVVU4CTWX7FFJT7XM7H2E33766S3FSC55YXND2PTJGCTQR44BAVEMYNCJI6PNBT2YQSI"",
      ""accessToken"": ""AccessToken1234""
    },
    ""new"": false
  },
  ""request"": {
    ""type"": ""IntentRequest"",
    ""requestId"": ""EdwRequestId.3d8b4179-64ff-4cf8-ace6-8875acbda28c"",
    ""locale"": ""en-US"",
    ""timestamp"": ""2017-07-01T19:22:21Z"",
    ""intent"": {
      ""name"": ""UpvoteIntent"",
      ""slots"": {}
    }
  },
  ""version"": ""1.0""
}
        	";
			var service = new StackExchange.Alexa.Service();
			var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(input);
			var result = await service.GetResponse(skillRequest, new MockLambdaContext());

        }

        [Fact(Skip = "Not now")]
        public async Task TestQuestionDetailsIntent()
        {
        	var input = @"
					{
					  ""session"": {
					    ""sessionId"": ""SessionId.12345"",
					    ""application"": {
					      ""applicationId"": ""amzn1.ask.skill.12345""
					    },
					    ""attributes"": {
					      ""site"": ""scifi"",
					      ""question_id"": 162523
					    },
					    ""user"": {
					      ""userId"": ""amzn1.ask.account.12345"",
					      ""accessToken"": ""myToken12345""
					    },
					    ""new"": false
					  },
					  ""request"": {
					    ""type"": ""IntentRequest"",
					    ""requestId"": ""EdwRequestId.12345"",
					    ""locale"": ""en-US"",
					    ""timestamp"": ""2017-06-29T00:16:07Z"",
					    ""intent"": {
					      ""name"": ""HotQuestionDetailsIntent"",
					      ""slots"": {}
					    }
					  },
					  ""version"": ""1.0""
					}
        	";
			var service = new StackExchange.Alexa.Service();
			var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(input);
			var result = await service.GetResponse(skillRequest, new MockLambdaContext());

        }
    }

  	public class MockLambdaContext : ILambdaContext
	{
		public string AwsRequestId => null;
        public IClientContext ClientContext => null;
        public string FunctionName => null;
        public string FunctionVersion => null;
        public ICognitoIdentity Identity => null;
        public string InvokedFunctionArn => null;
        public ILambdaLogger Logger => new ConsoleLambdaLogger();
        public string LogGroupName => null;
        public string LogStreamName => null;
        public int MemoryLimitInMB => 0;
        public TimeSpan RemainingTime => TimeSpan.Zero;
	}

	public class ConsoleLambdaLogger : ILambdaLogger
	{
		public void Log(string message) 
		{
			Console.Write(message);
		}

        public void LogLine(string message)
        {
        	Console.WriteLine(message);
        }
	}

}
