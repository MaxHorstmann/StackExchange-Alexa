using Amazon.Lambda.Core; 

namespace StackExchange.Alexa
{
	public class Skill 
	{
	    public string MyHandler(int count, ILambdaContext context) 
	    {
	        var logger = context.Logger;
	        logger.log("received : " + count);
	        return count.ToString();
	    }
	}

}
