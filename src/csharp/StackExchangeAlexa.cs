using Amazon.Lambda.Core; 

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
