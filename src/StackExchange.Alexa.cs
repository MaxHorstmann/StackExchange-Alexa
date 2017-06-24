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
                        Text = "Welcome to Stack Exchange!"
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
                            	foreach (var item in inbox.items)
                            	{
                            		sb.AppendLine($"Type: {item.type}.");
                            		sb.AppendLine($"Title: {item.title}.");	
                            		sb.AppendLine($"Body: {item.body}.");
                            	}
	                            (innerResponse as PlainTextOutputSpeech).Text = sb.ToString();
                            }
                            break;
                        case "AMAZON.CancelIntent":
                            log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                            innerResponse = new PlainTextOutputSpeech();
                            //(innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.StopIntent":
                            log.LogLine($"AMAZON.StopIntent: send StopMessage");
                            innerResponse = new PlainTextOutputSpeech();
                            //(innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.HelpIntent":
                            log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                            innerResponse = new PlainTextOutputSpeech();
                            //(innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
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

        private async Task<Inbox> GetInbox(string accessToken)
        {
        	const string key = "dzqlqab4VD4bynFom)Z1Ng(("; // not a secret
        	var url = $"/2.2/inbox?access_token={accessToken}&key={key}&filter=withbody";

        	using (var client = new HttpClient())
        	{
        		client.BaseAddress = new Uri("https://api.stackexchange.com");
        		var response = await client.GetStringAsync(url);
        		var inbox = JsonConvert.DeserializeObject<Inbox>(response);
        		return inbox;
           	}
        }





    }
    
}
