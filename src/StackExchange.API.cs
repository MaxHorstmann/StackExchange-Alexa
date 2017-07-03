using System;
using System.Text;
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
        	var response = await GetApiResponse("inbox/unread", "&filter=withbody");
        	if (!response.IsSuccessStatusCode) return null;
        	return JsonConvert.DeserializeObject<Inbox>(await response.Content.ReadAsStringAsync());
        }

        public async Task<Questions> GetHotQuestions(string site, int count)
        {
        	var response = await GetApiResponse("questions", $"&order=desc&sort=hot&pagesize={count}&site={site}");
        	if (!response.IsSuccessStatusCode) return null;
    		return JsonConvert.DeserializeObject<Questions>(await response.Content.ReadAsStringAsync());
        }

        public async Task<Question> GetQuestionDetails(string site, long question_id)
        {
        	var response = await GetApiResponse($"questions/{question_id}", $"&site={site}&filter=withbody");
        	if (!response.IsSuccessStatusCode) return null;
    		return JsonConvert.DeserializeObject<Questions>(await response.Content.ReadAsStringAsync()).items.First();
        }

        public async Task<Question> Upvote(string site, long question_id)
        {
        	var response = await GetApiResponse($"questions/{question_id}/upvote", $"&site={site}", true);
        	if (!response.IsSuccessStatusCode) 
        	{
        		Console.WriteLine(await response.Content.ReadAsStringAsync());
        		return null; 
        	}
    		return JsonConvert.DeserializeObject<Question>(await response.Content.ReadAsStringAsync());
        }

        private async Task<HttpResponseMessage> GetApiResponse(string route, string parameters, bool post = false)
        {
        	var baseUrl = $"/2.2/{route}";
        	var queryString = $"access_token={AccessToken}&key={Key}{parameters}";
        	using (var httpClient = new HttpClient())
        	{
        		httpClient.BaseAddress = new Uri(ApiBaseAddress);
        		if (post)
        		{
        			var requestContent = new StringContent(queryString, Encoding.UTF8, "application/x-www-form-urlencoded");
        			return await httpClient.PostAsync(baseUrl, requestContent);
        		}
        		else
        		{
        			var url = $"{baseUrl}?{queryString}";
	        		return await httpClient.GetAsync(url);
        		}
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

		public string bodyNoHtml => Regex.Replace(body ?? string.Empty, "<.*?>", String.Empty);
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
