using System;
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
    	public SkillResponse GetResponse(SkillRequest input, ILambdaContext context)
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
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "Your inbox is empty.";
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

    }
    
}
