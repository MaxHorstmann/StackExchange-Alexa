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


namespace StackExchange.Alexa
{
    public partial class Service
    {
        private const string WelcomeText = "<p>Welcome to Stack Exchange!</p>";

        private const string MainMenuOptions = "<p>Please say: inbox, hot question, help, or I'm done.</p>";

        private const string HelpText = @"<p>Stack Exchange is a network of 150+ Q&A communities including Stack Overflow, 
                the preeminent site for programmers to find, ask, and answer questions about software development.
                To learn more, please go to stackexchange.com or stackoverflow.com. In order to check your
                inbox and cast upvotes and downvotes, you need to open the Amazon Alexa app on your mobile device
                and set up account linking. </p>";

        private async Task<SkillResponse> GetLaunchRequestResponse()
        {
        	return CreateResponse(WelcomeText + await GetMainMenu());
        }

        private async Task<SkillResponse> GetInboxIntentResponse()
        {
            var apiResponse = await _client.GetInbox();
            if (!apiResponse.Success) 
                return CreateResponse(@"<p>In order to check your inbox, please enable account linking.</p>
                    <p>You can enable account linking for the Stack Exchange skill, open the Amazon Alexa mobile app, 
                    or go to Alexa.amazon.com</p>");

			var sb = new StringBuilder();
	        if (apiResponse.items.Count() == 0)
	        {
	        	sb.AppendLine("<p>There are no unread items in your inbox.</p>");
	        }
	        else
	        {
	        	sb.AppendLine($"<p>There are {apiResponse.items.Count()} unread items in your inbox.</p>");
	        	var i = 0;
	        	foreach (var item in apiResponse.items)
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
        	var rand = new Random();

        	var networkUsers = await _client.GetNetworkUsers();

        	if ((networkUsers.items == null) || (!networkUsers.items.Any()))
        	{
        		return CreateResponse("<p>Looks like you do not yet have an account on any site. Please go to stackexchange.com and sign up for a few sites, then try again!</p>", true);
        	}

        	var sitesWhereUserHasAccount = networkUsers.items.Select(m => m.site_url).ToList();

        	var sites = (await _client.GetSites())
        		.items
        		.Where(m => m.site_type == "main_site" && m.site_state == "normal" && sitesWhereUserHasAccount.Contains(m.site_url))
        		.ToList();  // TODO cache this somwehere, maybe Redis

        	var randomSite = sites[rand.Next(sites.Count)];
        	_state.site = randomSite.api_site_parameter;

        	const int NumberOfHotQuestions = 5;
        	var questions = (await _client.GetHotQuestions(_state.site, NumberOfHotQuestions)).items.ToList(); 
        	var question = questions[rand.Next(questions.Count)];
        	_state.question_id = question.question_id;

        	var sb = new StringBuilder();
        	sb.Append($"<p>Here's a hot question from {randomSite.name}:</p>");
        	sb.Append($"<p>{question.title}</p>");
        	sb.Append($"<p>Say: more details, next question, or I'm done.</p>");

        	return CreateResponse(sb.ToString(), false);
        }

        private async Task<SkillResponse> GetHotQuestionDetailsIntentResponse()
        {
        	if ((_state?.site == null) || (_state?.question_id == null)) return await GetHotQuestionIntentResponse();

        	var apiResponse = await _client.GetQuestionDetails(_state.site, _state.question_id.Value);

        	if (!apiResponse.Success) return CreateResponse("Sorry. There was a technical issue. Please try again later.");
        	var question = apiResponse.items.First();

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
        				sb.AppendLine($"<p>{tag}</p>");  
        			}
        		}        		
	        	sb.AppendLine("<break time=\"1s\"/>");
        	}

        	sb.AppendLine($"<p>Here's the full question:</p>");
        	sb.AppendLine($"<p>{question.bodyNoHtml}</p>");
        	sb.AppendLine("<break time=\"2s\"/>");
        	sb.AppendLine("<p>Please say: upvote, downvote, favorite, answers, next question, or I'm done.</p>");

        	return CreateResponse(sb.ToString(), false);
        }

        enum VoteType
        {
        	Upvote = 0,
        	Downvote = 1
        }

        private async Task<SkillResponse> GetUpvoteIntentResponse()
        {
        	return await GetVoteIntentResponse(VoteType.Upvote);
        }

        private async Task<SkillResponse> GetDownvoteIntentResponse()
        {
        	return await GetVoteIntentResponse(VoteType.Downvote);
        }

        private async Task<SkillResponse> GetVoteIntentResponse(VoteType voteType)
        {
        	if ((_state?.site == null) || (_state?.question_id == null)) return await GetHotQuestionIntentResponse();

        	var response = voteType == VoteType.Downvote ? 
        		await _client.Downvote(_state.site, _state.question_id.Value) :
        		await _client.Upvote(_state.site, _state.question_id.Value);

        	if (!response.Success)
        	{
        		return CreateResponse($"<p>{response.error_message}</p>");
        	}
        	var score = response.items.First().score;
        	return CreateResponse($"<p>Ok! The question now has a score of {score}.</p>", false);
        }

        private async Task<SkillResponse> GetFavoriteIntentResponse()
        {
        	if ((_state?.site == null) || (_state?.question_id == null)) return await GetHotQuestionIntentResponse();
        	var response = await _client.Favorite(_state.site, _state.question_id.Value);
        	if (!response.Success)
        	{
        		return CreateResponse($"<p>{response.error_message}</p>");
        	}
        	return CreateResponse($"<p>Ok! The question has been added to your favorites, so you can find it again later.</p>", false);
        }

        private async Task<string> GetMainMenu()
        {
        	var sb = new StringBuilder();
    		var apiRequest = await _client.GetInbox();
    		if (apiRequest.Success)
    		{
	            var newMessages = apiRequest.items.Count();
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

    }
    
}
