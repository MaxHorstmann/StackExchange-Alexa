using Amazon.Lambda.Core; 

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StackExchange.Alexa
{
	public class Skill 
	{
	    public string MyHandler(int count) 
	    {
	        return "Hello!! " + count.ToString();
	    }
	}

}
