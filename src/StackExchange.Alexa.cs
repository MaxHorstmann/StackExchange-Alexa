using System;
using Amazon.Lambda.Core;

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StackExchange.Alexa
{
    public class Service
    {
    	// TODO 
    	public object GetResponse()
    	{
    		return new { message = "Hello from Lambda!", time = DateTime.UtcNow };
    	}
    }
    
}
