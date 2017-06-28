using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using StackExchange.API;


[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StackExchange.Alexa
{
    public class Service
    {
    	private Client _client;

    	// Main entry point
    	public async Task<SkillResponse> GetResponse(SkillRequest input, ILambdaContext context)
        {
            try
            {
				_client = new Client(input?.Session?.User?.AccessToken);

                if (input.GetRequestType() == typeof(LaunchRequest)) return await GetLaunchRequestResponse();
                if (input.GetRequestType() == typeof(IntentRequest))
                {
                	var intentRequest = (IntentRequest)input.Request;
                	if (intentRequest.Intent.Name=="InboxIntent") return await GetInboxIntentResponse();
                	if (intentRequest.Intent.Name=="HotQuestionIntent") return await GetHotQuestionIntentResponse();
                	if (intentRequest.Intent.Name=="AMAZON.HelpIntent") return CreateResponse(HelpText + MainMenuOptions);
                	if (intentRequest.Intent.Name=="AMAZON.CancelIntent") return await GetLaunchRequestResponse();
                	if (intentRequest.Intent.Name=="AMAZON.StopIntent") return CreateResponse("Ok, bye!", true);
                }
                return CreateResponse("Sorry, not sure what you're saying.");
            }
            catch (Exception ex)
            {
	            var log = context.Logger;
                log.LogLine("Unhandled exception:");
                log.LogLine(ex.ToString());
                log.LogLine(JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        private async Task<SkillResponse> GetLaunchRequestResponse()
        {
        	return CreateResponse(WelcomeText + await GetMainMenu());
        }

        private async Task<SkillResponse> GetInboxIntentResponse()
        {
			var sb = new StringBuilder();
	        var inbox = await _client.GetInbox();
	        if (inbox.items.Count() == 0)
	        {
	        	sb.AppendLine("<p>There are no unread items in your inbox.</p>");
	        }
	        else
	        {
	        	sb.AppendLine($"<p>There are {inbox.items.Count()} unread items in your inbox.</p>");
	        	var i = 0;
	        	foreach (var item in inbox.items)
	        	{
	        		i++;
	        		sb.Append("<p>");
	        		sb.AppendLine($"<s><say-as interpret-as=\"ordinal\">{i}</say-as> message</s>");
	        		sb.AppendLine($"Type: {item.type}.");
	        		sb.AppendLine($"Title: {item.title}.");	
	        		sb.AppendLine($"Body: {item.body}.");
	        		sb.Append("</p><p></p><p></p>");
	        	}
	        }
        	return CreateResponse(sb.ToString());
        }

        private async Task<SkillResponse> GetHotQuestionIntentResponse()
        {
        	var site = "scifi"; // TODO randomize
        	var questions = await _client.GetHotQuestions(site, 1);
        	var question = questions.items.First();   // TODO pick random from top 5 or so
        	return CreateResponse(question.title);
        }

        private SkillResponse CreateResponse(string ssml, bool shouldEndSession = false)
        {
            // if (response.SessionAttributes == null)
            // {
            //     response.SessionAttributes = new System.Collections.Generic.Dictionary<string, object>();
            // }
            // response.SessionAttributes.Add("foo", count++);

        	return new SkillResponse()
        	{
    			Version = "1.0",
        		Response = new ResponseBody()
        		{
        			ShouldEndSession = shouldEndSession,
        			OutputSpeech = new SsmlOutputSpeech()
		            {
		           		Ssml = "<speak>" + ssml + "</speak>"
		            }
        		}
        	};
        }

        private async Task<string> GetMainMenu()
        {
        	var sb = new StringBuilder();
    		var inbox = await _client.GetInbox();
            var newMessages = inbox.items.Count();
            if (newMessages == 0) 
            {
            	sb.AppendLine("You have no unread messages.");
            }
            else
            {
            	sb.AppendLine($"You have {newMessages} unread messages.");
            }

        	sb.AppendLine(MainMenuOptions);
        	return sb.ToString();
        }

        private const string WelcomeText = "Welcome to Stack Exchange!";

        private const string MainMenuOptions = "Please say: inbox, hot question, help, or I'm done.";

        private const string HelpText = @"Stack Exchange is a network of 150+ Q&A communities including Stack Overflow, 
        		the preeminent site for programmers to find, ask, and answer questions about software development.
        		To learn more, please go to stackexchange.com or stackoverflow.com. In order to check your
        		inbox and cast upvotes and downvotes, you need to open the Amazon Alexa app on your mobile device
        		and set up account linking. ";


    }
    
}
