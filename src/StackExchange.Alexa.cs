﻿using System;
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


[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StackExchange.Alexa
{
    public class Service
    {
    	public async Task<SkillResponse> GetResponse(SkillRequest input, ILambdaContext context)
        {
            var log = context.Logger;
            try
            {
				var accessToken = input?.Session?.User?.AccessToken;

                SkillResponse response = new SkillResponse();
                response.Response = new ResponseBody();
                response.Response.ShouldEndSession = false;
                IOutputSpeech innerResponse = null;
                log.LogLine($"Skill Request Object:");
                log.LogLine(JsonConvert.SerializeObject(input));

                if (input.GetRequestType() == typeof(LaunchRequest))
                {
                    log.LogLine($"Default LaunchRequest made: 'Alexa, open Stack Exchange");
                    innerResponse = new PlainTextOutputSpeech()
                    {
                        Text = await GetMainMenu(accessToken)
                    };
                }
                else if (input.GetRequestType() == typeof(IntentRequest))
                {
                    var intentRequest = (IntentRequest)input.Request;

                    switch (intentRequest.Intent.Name)
                    {
                        case "InboxIntent":
                            log.LogLine($"Inbox intent");

                            var inbox = await GetInbox(input.Session.User.AccessToken);

                            innerResponse = new PlainTextOutputSpeech();

                            if (inbox.items.Count() == 0)
                            {
	                            (innerResponse as PlainTextOutputSpeech).Text = $"There are no unread items in your inbox.";
                            }
                            else
                            {
                            	var sb = new StringBuilder();
                            	sb.AppendLine($"There are {inbox.items.Count()} unread items in your inbox.");
                            	var i = 0;
                            	foreach (var item in inbox.items)
                            	{
                            		i++;
                            		sb.AppendLine($"Item {i}.");
                            		sb.AppendLine($"Type: {item.type}.");
                            		sb.AppendLine($"Title: {item.title}.");	
                            		sb.AppendLine($"Body: {item.body}.");
                            	}
	                            (innerResponse as PlainTextOutputSpeech).Text = sb.ToString();
                            }
                            break;
                        case "HotQuestionIntent":
                            log.LogLine($"HotQuestion intent");
                            innerResponse = new PlainTextOutputSpeech() 
                            {
                            	Text = "Support for hot questions is coming soon!"
                            };
                            break;
                        case "AMAZON.CancelIntent":
                        case "AMAZON.StopIntent":
                            log.LogLine($"AMAZON.StopIntent: send StopMessage");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "Ok, bye!";
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.HelpIntent":
                            log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = HelpText + MainMenuOptions;
                            break;
                        default:
                            log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                            innerResponse = new PlainTextOutputSpeech();
                            //(innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                            break;
                    }
                }

                response.Response.OutputSpeech = innerResponse;
                response.Version = "1.0";

                if (response.SessionAttributes == null)
                {
                    response.SessionAttributes = new System.Collections.Generic.Dictionary<string, object>();
                }
                //response.SessionAttributes.Add("foo", count++);

                log.LogLine($"Skill Response Object...");
                log.LogLine(JsonConvert.SerializeObject(response));
                return response;

            }
            catch (Exception ex)
            {
                log.LogLine("Unhandled exception:");
                log.LogLine(ex.ToString());
                log.LogLine(JsonConvert.SerializeObject(ex));
                throw;
            }
        }

        private async Task<string> GetMainMenu(string accessToken)
        {
        	var sb = new StringBuilder();
        	sb.AppendLine("Welcome to Stack Exchange!");

        	if (accessToken != null)
        	{
        		var inbox = await GetInbox(accessToken);
                var newMessages = inbox.items.Count();
                if (newMessages == 0) 
                {
                	sb.AppendLine("You have no new messages.");
                }
                else
                {
                	sb.AppendLine($"You have {newMessages} new messages.");
                }
        	}

        	sb.AppendLine(MainMenuOptions);
        	return sb.ToString();
        }

        private string MainMenuOptions => "Please say: inbox, hot question, help, or I'm done.";

        private async Task<Inbox> GetInbox(string accessToken)
        {
        	const string key = "dzqlqab4VD4bynFom)Z1Ng(("; // not a secret
        	var url = $"/2.2/inbox/unread?access_token={accessToken}&key={key}&filter=withbody";

        	using (var client = new HttpClient())
        	{
        		client.BaseAddress = new Uri("https://api.stackexchange.com");
        		var response = await client.GetStringAsync(url);
        		var inbox = JsonConvert.DeserializeObject<Inbox>(response);
        		return inbox;
           	}
        }

        private string HelpText => @"Stack Exchange is a network of 150+ Q&A communities including Stack Overflow, 
        		the preeminent site for programmers to find, ask, and answer questions about software development.
        		To learn more, please go to stackexchange.com or stackoverflow.com. In order to check your
        		inbox and cast upvotes and downvotes, you need to open the Amazon Alexa app on your mobile device
        		and set up account linking. ";


    }
    
}
