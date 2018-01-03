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
using Mixpanel;


[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StackExchange.Alexa
{
    public partial class Service
    {
    	private class State
    	{
    		public long? question_id {get; set;}
    		public string site {get; set;}
    	}

    	private Client _client;
    	private ILambdaContext _context;
    	private State _state;
        private MixpanelClient _mixpanel;

    	// Main entry point
    	public async Task<SkillResponse> GetResponse(SkillRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(JsonConvert.SerializeObject(request));
            MixpanelConfig.Global.SerializeJsonFn = JsonConvert.SerializeObject;            
            _mixpanel = new MixpanelClient(MixPanelToken);
            
            //await _mixpanel.TrackAsync("GetResponse.Begin", new {});

            try
            {
            	_context = context;
            	_state = RestoreState(request);
				_client = new Client(StackApiKey, request?.Session?.User?.AccessToken); // StackExchange API client
				var response = await RouteRequest(request);
	            context.Logger.LogLine(JsonConvert.SerializeObject(response));
                //await _mixpanel.TrackAsync("GetResponse.End", new {});
				return response;
            }
            catch (Exception ex)
            {
                await _mixpanel.TrackAsync("GetResponse.Exception", new {});
                context.Logger.LogLine("Unhandled exception:");
                context.Logger.LogLine(ex.ToString());
                context.Logger.LogLine(JsonConvert.SerializeObject(ex));
                return CreateResponse($"Sorry, there was a technical problem. Please try again later.");
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
            if (input.GetRequestType() == typeof(LaunchRequest)) 
            {
                await _mixpanel.TrackAsync("Launch", new {});
                return await GetLaunchRequestResponse();
            }
            if (input.GetRequestType() == typeof(IntentRequest))
            {
            	var intentRequest = (IntentRequest)input.Request;
                await _mixpanel.TrackAsync("Intent", new { Name = intentRequest.Intent.Name });
            	if (intentRequest.Intent.Name=="InboxIntent") return await GetInboxIntentResponse();
            	if (intentRequest.Intent.Name=="HotQuestionIntent") return await GetHotQuestionIntentResponse();
            	if (intentRequest.Intent.Name=="HotQuestionDetailsIntent") return await GetHotQuestionDetailsIntentResponse();
                if (intentRequest.Intent.Name=="HotQuestionAnswersIntent") return await GetHotQuestionAnswersIntentResponse();
            	if (intentRequest.Intent.Name=="UpvoteIntent") return await GetUpvoteIntentResponse();
            	if (intentRequest.Intent.Name=="DownvoteIntent") return await GetDownvoteIntentResponse();
            	if (intentRequest.Intent.Name=="FavoriteIntent") return await GetFavoriteIntentResponse();
            	if (intentRequest.Intent.Name=="AMAZON.HelpIntent") return CreateResponse(HelpText + MainMenuOptions);
            	if (intentRequest.Intent.Name=="AMAZON.CancelIntent") return CreateResponse("Ok, bye!", true);
            	if (intentRequest.Intent.Name=="AMAZON.StopIntent") return CreateResponse("Ok, bye!", true);
            }
            return CreateResponse("Sorry, not sure what you're saying.");
    	}

        private SkillResponse CreateResponse(
        		string ssml, 
        		bool shouldEndSession = false,
        		bool displayLinkAccountCard = false)
        {
        	var sessionAttributes = new Dictionary<string, object>();
        	sessionAttributes.Add("site", _state?.site);
        	sessionAttributes.Add("question_id", _state?.question_id);


        	var card = displayLinkAccountCard ? new LinkAccountCard() : null;

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
		            },
		            Card = card
        		}
        	};
        }

    }
    
}
