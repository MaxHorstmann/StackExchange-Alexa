using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StackExchange.API
{
	public class Client
	{
      	const string Key = "dzqlqab4VD4bynFom)Z1Ng(("; // not a secret
      	const string ApiBaseAddress = "https://api.stackexchange.com";

      	private readonly string AccessToken;
      	public Client(string accessToken)
      	{
      		AccessToken = accessToken;
      	}

        public async Task<Inbox> GetInbox()
        {
    		return JsonConvert.DeserializeObject<Inbox>(await GetApiResponse("inbox","&filter=withbody"));
        }

        public async Task<Questions> GetHotQuestions(string site, int count)
        {
    		return JsonConvert.DeserializeObject<Questions>(await GetApiResponse("questions", 
    			$"&order=desc&sort=hot&pagesize={count}&site={site}"));
        }

        public async Task<Question> GetQuestionDetails(string site, long question_id)
        {
    		return JsonConvert.DeserializeObject<Questions>(await GetApiResponse($"questions/{question_id}", $"&site={site}&filter=withbody")).items.First();
        }

        public async Task Upvote(string site, long question_id)
        {
    		await GetApiResponse($"questions/{question_id}/upvote", $"&site={site}", true);
        }

        private async Task<string> GetApiResponse(string route, string parameters, bool post = false)
        {
        	var url = $"/2.2/{route}?access_token={AccessToken}&key={Key}{parameters}";
        	using (var httpClient = new HttpClient())
        	{
        		httpClient.BaseAddress = new Uri(ApiBaseAddress);
        		if (post)
        		{
        			var result = await httpClient.PostAsync(url, null);
        			return await result.Content.ReadAsStringAsync();
        		}
        		return await httpClient.GetStringAsync(url);
           	}
        }

	}

	public class Questions
	{
		public IEnumerable<Question> items {get; set;}
	}

	public class Question
	{
		public int question_id {get; set;}
		public string title {get; set;}
		public string body { get; set;}
		public IEnumerable<string> tags {get; set;}

		public string bodyNoHtml => Regex.Replace(body, "<.*?>", String.Empty);
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
