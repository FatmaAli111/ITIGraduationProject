using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ITIGraduationProject.Application.Features.AiChat.Commands.Models
{
    public class DifyChatRequest
    {
        [JsonPropertyName("inputs")]
        public Dictionary<string, string> Inputs { get; set; } = new();

        [JsonPropertyName("response_mode")]
        public string ResponseMode { get; set; } = "blocking";

        [JsonPropertyName("user")]
        public string User { get; set; } = "WearlyUser";
    }

    public class DifyChatResponse
    {
        [JsonPropertyName("data")]
        public DifyResponseData Data { get; set; } = new();

        public string Answer => Data?.Outputs?.Text ?? string.Empty;
    }

    public class DifyResponseData
    {
        [JsonPropertyName("outputs")]
        public DifyOutputs Outputs { get; set; } = new();
    }

    public class DifyOutputs
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}