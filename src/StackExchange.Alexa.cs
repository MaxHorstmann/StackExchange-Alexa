using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
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
    	// Main entry point
    	public async Task<SkillResponse> GetResponse(SkillRequest input, ILambdaContext context)
        {
            try
            {
				var accessToken = input?.Session?.User?.AccessToken;
                if (input.GetRequestType() == typeof(LaunchRequest)) return await GetLaunchRequestResponse(accessToken);
                if (input.GetRequestType() == typeof(IntentRequest))
                {
                	var intentRequest = (IntentRequest)input.Request;
                	if (intentRequest.Intent.Name=="InboxIntent") return await GetInboxIntentResponse(accessToken);
                	if (intentRequest.Intent.Name=="HotQuestionIntent") return await GetHotQuestionIntentResponse(accessToken);
                	if (intentRequest.Intent.Name=="AMAZON.HelpIntent") return CreatePlainTextResponse(HelpText + MainMenuOptions);
                	if (intentRequest.Intent.Name=="AMAZON.CancelIntent") return await GetLaunchRequestResponse(accessToken);
                	if (intentRequest.Intent.Name=="AMAZON.StopIntent") return CreatePlainTextResponse("Ok, bye!", true);
                }
                return CreatePlainTextResponse("Sorry, not sure what you're saying.");
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

        private async Task<SkillResponse> GetLaunchRequestResponse(string accessToken)
        {
        	return CreatePlainTextResponse(await GetMainMenu(accessToken));
        }

        private async Task<SkillResponse> GetInboxIntentResponse(string accessToken)
        {
			var sb = new StringBuilder();
	       	sb.Append("<speak>");
	        var inbox = await Client.GetInbox(accessToken);
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
        	sb.Append("</speak>");
        	return CreateSsmlResponse(sb.ToString());
        }

        private async Task<SkillResponse> GetHotQuestionIntentResponse(string accessToken)
        {
        	//var site = "scifi"; // TODO randomize

        	// https://api.stackexchange.com/2.2/questions?order=desc&sort=hot&site=scifi&pagesize=3


        	return CreatePlainTextResponse("Coming soon!");
        }

        private SkillResponse CreatePlainTextResponse(string text, bool shouldEndSession = false)
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
        			OutputSpeech = new PlainTextOutputSpeech()
		            {
		                Text = text
		            }
        		}
        	};
        }

        private SkillResponse CreateSsmlResponse(string ssml, bool shouldEndSession = false)
        {
        	return new SkillResponse()
        	{
    			Version = "1.0",
        		Response = new ResponseBody()
        		{
        			ShouldEndSession = shouldEndSession,
        			OutputSpeech = new SsmlOutputSpeech()
		            {
		            	Ssml = ssml
		            }
        		}
        	};
        }

        private async Task<string> GetMainMenu(string accessToken)
        {
        	var sb = new StringBuilder();
        	sb.AppendLine("Welcome to Stack Exchange!");

        	if (accessToken != null)
        	{
        		var inbox = await Client.GetInbox(accessToken);
                var newMessages = inbox.items.Count();
                if (newMessages == 0) 
                {
                	sb.AppendLine("You have no unread messages.");
                }
                else
                {
                	sb.AppendLine($"You have {newMessages} unread messages.");
                }
        	}

        	sb.AppendLine(MainMenuOptions);
        	return sb.ToString();
        }


        private string MainMenuOptions => "Please say: inbox, hot question, help, or I'm done.";

        private string HelpText => @"Stack Exchange is a network of 150+ Q&A communities including Stack Overflow, 
        		the preeminent site for programmers to find, ask, and answer questions about software development.
        		To learn more, please go to stackexchange.com or stackoverflow.com. In order to check your
        		inbox and cast upvotes and downvotes, you need to open the Amazon Alexa app on your mobile device
        		and set up account linking. ";


    }
    
}
