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
    	private class State
    	{
    		public long? question_id {get; set;}
    		public string site {get; set;}
    	}

    	private Client _client;
    	private ILambdaContext _context;
    	private State _state;

    	// Main entry point
    	public async Task<SkillResponse> GetResponse(SkillRequest input, ILambdaContext context)
        {
            try
            {
            	_context = context;
            	_state = RestoreState(input);
				_client = new Client(input?.Session?.User?.AccessToken); // StackExchange API client
				return await RouteRequest(input);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine("Unhandled exception:");
                context.Logger.LogLine(ex.ToString());
                context.Logger.LogLine(JsonConvert.SerializeObject(ex));
                return CreateResponse(ex.ToString());
                //throw;
            }
        }

        private static State RestoreState(SkillRequest input)
        {
        	var state = new State();
			if (input?.Session?.Attributes != null)
			{
				if (input.Session.Attributes.ContainsKey("site")) state.site = (string)input.Session.Attributes["site"];
				if (input.Session.Attributes.ContainsKey("question_id")) state.question_id = (long?)(input.Session.Attributes["question_id"]);
			}
			return state;
        }

        public async Task<SkillResponse> RouteRequest(SkillRequest input)
    	{
            if (input.GetRequestType() == typeof(LaunchRequest)) return await GetLaunchRequestResponse();
            if (input.GetRequestType() == typeof(IntentRequest))
            {
            	var intentRequest = (IntentRequest)input.Request;
            	if (intentRequest.Intent.Name=="InboxIntent") return await GetInboxIntentResponse();
            	if (intentRequest.Intent.Name=="HotQuestionIntent") return await GetHotQuestionIntentResponse();
            	if (intentRequest.Intent.Name=="HotQuestionDetailsIntent") 
            			return await GetHotQuestionDetailsIntentResponse();
            	if (intentRequest.Intent.Name=="UpvoteIntent") return await GetUpvoteIntentResponse();
            	if (intentRequest.Intent.Name=="AMAZON.HelpIntent") return CreateResponse(HelpText + MainMenuOptions);
            	if (intentRequest.Intent.Name=="AMAZON.CancelIntent") return await GetLaunchRequestResponse();
            	if (intentRequest.Intent.Name=="AMAZON.StopIntent") return CreateResponse("Ok, bye!", true);
            }
            return CreateResponse("Sorry, not sure what you're saying.");
    	}

        private async Task<SkillResponse> GetLaunchRequestResponse()
        {
        	return CreateResponse(WelcomeText + await GetMainMenu());
        }

        private async Task<SkillResponse> GetInboxIntentResponse()
        {
			var sb = new StringBuilder();
			var apiResponse = await _client.GetInbox();
			if (!apiResponse.Success) return CreateResponse("Sorry. I'm having trouble reading your inbox at the moment. Please try again later.");
	        var inbox = apiResponse.Result;
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
        	_state.site = "scifi"; // TODO randomize
        	var apiResponse = await _client.GetHotQuestions(_state.site, 1); 
        	if (!apiResponse.Success) return CreateResponse("Sorry. I can't access hot questions at the moment. Please try again later.");
        	var questions = apiResponse.Result;
        	var question = questions.items.First();   // TODO pick random from top 5 or so
        	_state.question_id = question.question_id;

        	var sb = new StringBuilder();
        	sb.Append($"<p>Here's a hot question from {_state.site}:</p>");
        	sb.Append($"<p>{question.title}</p>");
        	sb.Append($"<p>Say: more details, next question, or I'm done.</p>");

        	return CreateResponse(sb.ToString(), false);
        }

        private async Task<SkillResponse> GetHotQuestionDetailsIntentResponse()
        {
        	if ((_state?.site == null) || (_state?.question_id == null)) return await GetHotQuestionIntentResponse();
        	var apiResponse = await _client.GetQuestionDetails(_state.site, _state.question_id.Value);
        	if (!apiResponse.Success) return CreateResponse("Sorry. There was a technical issue. Please try again later.");
        	var question = apiResponse.Result;

        	var sb = new StringBuilder();
        	if ((question.tags != null) && (question.tags.Any()))
        	{
        		if (question.tags.Count() == 1)
        		{
        			sb.AppendLine("<p>The question has a single tag:</p>");
        			sb.AppendLine($"<p>{question.tags.First()}</p>");
        		}
        		else
        		{
        			sb.AppendLine($"<p>The question has {question.tags.Count()} tags:</p>");
        			foreach (var tag in question.tags)
        			{
        				sb.AppendLine($"<p>tag</p>");  
        			}
        		}        		
        	}

        	sb.AppendLine($"<p>Here's the full question: {question.bodyNoHtml}</p>");
        	sb.AppendLine("<break time=\"3s\"/>");
        	sb.AppendLine("<p>Please say: upvote, downvote, answers, next question, or I'm done.</p>");

        	return CreateResponse(sb.ToString(), false);
        }

        private async Task<SkillResponse> GetUpvoteIntentResponse()
        {
        	if ((_state?.site == null) || (_state?.question_id == null)) return await GetHotQuestionIntentResponse();
        	var response = await _client.Upvote(_state.site, _state.question_id.Value);
        	var responseText = response.Success ? "Ok, upvated!" : "Sorry, could not upvote.";
        	return CreateResponse("Ok, upvoted!", false);
        }

        private SkillResponse CreateResponse(
        		string ssml, 
        		bool shouldEndSession = false)
        {
        	var sessionAttributes = new Dictionary<string, object>();
        	sessionAttributes.Add("site", _state?.site);
        	sessionAttributes.Add("question_id", _state?.question_id);

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
    		var apiRequest = await _client.GetInbox();
    		if (apiRequest.Success)
    		{
    			var inbox = apiRequest.Result;
	            var newMessages = inbox.items.Count();
	            if (newMessages == 0) 
	            {
	            	sb.AppendLine("<p>You have no unread messages.</p>");
	            }
	            else
	            {
	            	sb.AppendLine($"<p>You have {newMessages} unread messages.</p>");
	            }
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
