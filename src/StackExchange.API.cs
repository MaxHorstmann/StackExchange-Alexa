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

        public async Task<ApiResponse<Inbox>> GetInbox()
        {
        	return await GetApiResponse<Inbox>("inbox/unread", "&filter=withbody");
        }

        public async Task<ApiResponse<Questions>> GetHotQuestions(string site, int count)
        {
        	return await GetApiResponse<Questions>("questions", $"&order=desc&sort=hot&pagesize={count}&site={site}");
        }

        public async Task<ApiResponse<Questions>> GetQuestionDetails(string site, long question_id)
        {
        	return await GetApiResponse<Questions>($"questions/{question_id}", $"&site={site}&filter=withbody");
        }

        public async Task<ApiResponse<Question>> Upvote(string site, long question_id)
        {
        	return await GetApiResponse<Question>($"questions/{question_id}/upvote", $"&site={site}", true);
        }

        private async Task<ApiResponse<T>> GetApiResponse<T>(string route, string parameters, bool post = false)
        {
        	var baseUrl = $"/2.2/{route}";
        	var queryString = $"access_token={AccessToken}&key={Key}{parameters}";
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
        		var apiResponse = new ApiResponse<T>() { Success = response.IsSuccessStatusCode };
        		var responseContent = await response.Content.ReadAsStringAsync();
        		if (apiResponse.Success) 
        		{
        			apiResponse.Result = JsonConvert.DeserializeObject<T>(responseContent);
        		}
        		else
        		{
        			apiResponse.Error = JsonConvert.DeserializeObject<ApiError>(responseContent);
        		}
        		apiResponse.RawResponse = responseContent;
        		return apiResponse;
           	}
        }

	}

	public class ApiResponse<T>
	{
		public bool Success {get; set;}
		public T Result {get; set;}
		public ApiError Error {get; set;}
		public string RawResponse { get; set;} // for debugging
	}

	public class ApiError
	{
		public int error_id {get ;set;}
		public string error_name {get; set;}
		public string error_message {get; set;}
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
