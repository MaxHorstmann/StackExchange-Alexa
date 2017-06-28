using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace StackExchange.API
{
	public class Client
	{
        public static async Task<Inbox> GetInbox(string accessToken)
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

	public class Inbox
	{
		public IEnumerable<InboxItem> items { get; set; }
	}

	public class InboxItem
	{
		public string item_type { get; set;}
		public string type => item_type == "chat_message" ? "chat message" : item_type;
		public string title { get; set;}
		public string body { get; set;}
	}
}
