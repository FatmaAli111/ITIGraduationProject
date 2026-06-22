using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Features.AiChat.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IAiService;


namespace ITIGraduationProject.Service.AI
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskGeminiAsync(string userQuestion)
        {
            try
            {
                var apiKey = _configuration["DifyAI:ApiKey"];
                var baseUrl = _configuration["DifyAI:BaseUrl"];

                var requestData = new DifyChatRequest();
                requestData.Inputs.Add("user_question", userQuestion);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}workflows/run");
                request.Headers.Add("Authorization", apiKey);
                request.Content = JsonContent.Create(requestData);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return "Sorry, I am having trouble connecting to the AI assistant right now.";
                }

                var result = await response.Content.ReadFromJsonAsync<DifyChatResponse>();
                return result?.Answer ?? "I could not process the response from the assistant.";
            }
            catch (Exception)
            {
                return "Sorry, I am having trouble connecting to the AI assistant right now.";
            }
        }
    }
}