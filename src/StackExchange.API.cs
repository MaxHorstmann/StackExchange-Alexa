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
      	const string ApiBaseAddress = "https://api.stackexchange.com";

      	private readonly string _accessToken;
      	private readonly string _key;

      	public Client(string key, string accessToken)
      	{
      		_key = key;
      		_accessToken = accessToken;
      	}

        public async Task<ApiResponse<InboxItem>> GetInbox()
        {
        	return await GetApiResponse<InboxItem>("inbox/unread", "&filter=withbody");
        }

        public async Task<ApiResponse<Question>> GetHotQuestions(string site, int count)
        {
        	return await GetApiResponse<Question>("questions", $"&order=desc&sort=hot&pagesize={count}&site={site}");
        }

        public async Task<ApiResponse<Question>> GetQuestionDetails(string site, long question_id)
        {
        	return await GetApiResponse<Question>($"questions/{question_id}", $"&site={site}&filter=withbody");
        }

        public async Task<ApiResponse<Site>> GetSites()
        {
        	return await GetApiResponse<Site>($"sites", $"&pagesize=300");
        }

        public async Task<ApiResponse<Question>> Upvote(string site, long question_id)
        {
        	return await GetApiResponse<Question>($"questions/{question_id}/upvote", $"&site={site}", true);
        }

        public async Task<ApiResponse<Question>> Downvote(string site, long question_id)
        {
        	return await GetApiResponse<Question>($"questions/{question_id}/downvote", $"&site={site}", true);
        }

        public async Task<ApiResponse<Question>> Favorite(string site, long question_id)
        {
        	return await GetApiResponse<Question>($"questions/{question_id}/favorite", $"&site={site}", true);
        }

        private async Task<ApiResponse<T>> GetApiResponse<T>(string route, string parameters, bool post = false)
        {
        	var baseUrl = $"/2.2/{route}";
        	var queryString = $"access_token={_accessToken}&key={_key}{parameters}";
        	using (var httpClient = new HttpClient())
        	{
        		httpClient.BaseAddress = new Uri(ApiBaseAddress);
        		HttpResponseMessage response;
        		if (post)
        		{
        			var requestContent = new StringContent(queryString, Encoding.UTF8, "application/x-www-form-urlencoded");
        			response = await httpClient.PostAsync(baseUrl, requestContent);
        		}
        		else
        		{
        			var url = $"{baseUrl}?{queryString}";
        			response = await httpClient.GetAsync(url);
        		}
        		return JsonConvert.DeserializeObject<ApiResponse<T>>(await response.Content.ReadAsStringAsync());
           	}
        }

	}

	public class ApiResponse<T>
	{
		public IEnumerable<T> items {get; set;}
		public long? error_id {get ;set;}
		public string error_name {get; set;}
		public string error_message {get; set;}

		public bool Success => error_id == null;
	}

	public class Question
	{
		public long question_id {get; set;}
		public string title {get; set;}
		public string body { get; set;}
		public IEnumerable<string> tags {get; set;}
		public long score {get; set;}

		public string bodyNoHtml => Regex.Replace(body ?? string.Empty, "<.*?>", String.Empty);
	}

	public class InboxItem
	{
		public string item_type { get; set;}
		public string type => item_type == "chat_message" ? "chat message" : item_type;
		public string title { get; set;}
		public string body { get; set;}
	}

	public class Site
	{
		public string api_site_parameter {get; set;}
		public string name {get; set;}
		public string site_url {get; set;}
		public string site_type {get; set;}
		public string site_state {get; set;}
	}
}
