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
    	private ILambdaContext _context;

    	// Main entry point
    	public async Task<SkillResponse> GetResponse(SkillRequest input, ILambdaContext context)
        {
            try
            {
            	_context = context;

            	// Initalize StackExchange API client
				_client = new Client(input?.Session?.User?.AccessToken);

				// Restore state from previous conversation, if any
				var site = (string)null;
				var question_id = (long?)null;
				if (input?.Session?.Attributes != null)
				{
					if (input.Session.Attributes.ContainsKey("site")) site = (string)input.Session.Attributes["site"];
					if (input.Session.Attributes.ContainsKey("question_id")) question_id = (long)(input.Session.Attributes["question_id"]);
				}

                if (input.GetRequestType() == typeof(LaunchRequest)) return await GetLaunchRequestResponse();
                if (input.GetRequestType() == typeof(IntentRequest))
                {
                	var intentRequest = (IntentRequest)input.Request;
                	if (intentRequest.Intent.Name=="InboxIntent") return await GetInboxIntentResponse();
                	if (intentRequest.Intent.Name=="HotQuestionIntent") return await GetHotQuestionIntentResponse();
                	if (intentRequest.Intent.Name=="HotQuestionDetailsIntent") 
                			return await GetHotQuestionDetailsIntentResponse(site, question_id);
                	if (intentRequest.Intent.Name=="UpvoteIntent") return await GetUpvoteIntentResponse(site, question_id);
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

        	var sb = new StringBuilder();

        	sb.Append($"<p>Here's a hot question from {site}:</p>");
        	sb.Append($"<p>{question.title}</p>");
        	sb.Append($"<p>Say: more details, next question, or I'm done.</p>");

        	var sessionAttributes = new Dictionary<string, object>();
        	sessionAttributes.Add("site", site);
        	sessionAttributes.Add("question_id", question.question_id);
        	return CreateResponse(sb.ToString(), false, sessionAttributes);
        }

        private async Task<SkillResponse> GetHotQuestionDetailsIntentResponse(string site, long? question_id)
        {
        	if ((site == null) || (question_id == null)) return await GetHotQuestionIntentResponse();

        	var question = await _client.GetQuestionDetails(site, question_id.Value);
        	var sessionAttributes = new Dictionary<string, object>();
        	sessionAttributes.Add("site", site);
        	sessionAttributes.Add("question_id", question.question_id);
        	return CreateResponse(question.bodyNoHtml, false, sessionAttributes);
        }

        private async Task<SkillResponse> GetUpvoteIntentResponse(string site, long? question_id)
        {
        	if ((site == null) || (question_id == null)) return await GetHotQuestionIntentResponse();
        	var response = await _client.Upvote(site, question_id.Value);
        	var sessionAttributes = new Dictionary<string, object>();
        	sessionAttributes.Add("site", site);
        	sessionAttributes.Add("question_id", question_id);
        	return CreateResponse("Ok, upvoted!", false, sessionAttributes);
        }

        

        private SkillResponse CreateResponse(
        		string ssml, 
        		bool shouldEndSession = false, 
        		Dictionary<string, object> sessionAttributes = null)
        {
        	return new SkillResponse()
        	{
    			Version = "1.0",
    			SessionAttributes = sessionAttributes,
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
            	sb.AppendLine("<p>You have no unread messages.</p>");
            }
            else
            {
            	sb.AppendLine($"<p>You have {newMessages} unread messages.</p>");
            }

        	sb.AppendLine(MainMenuOptions);
        	return sb.ToString();
        }

        private const string WelcomeText = "<p>Welcome to Stack Exchange!</p>";

        private const string MainMenuOptions = "<p>Please say: inbox, hot question, help, or I'm done.</p>";

        private const string HelpText = @"<p>Stack Exchange is a network of 150+ Q&A communities including Stack Overflow, 
        		the preeminent site for programmers to find, ask, and answer questions about software development.
        		To learn more, please go to stackexchange.com or stackoverflow.com. In order to check your
        		inbox and cast upvotes and downvotes, you need to open the Amazon Alexa app on your mobile device
        		and set up account linking. </p>";


    }
    
}
