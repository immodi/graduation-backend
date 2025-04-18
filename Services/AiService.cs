using System.Text;
using System.Text.Json;
using backend.DTOs.Requests;

namespace backend.Services;

public class AiService(string apiKey)
{
    private readonly HttpClient _httpClient = new();
    
    public async Task<string> GetFastLanguageModelExplanationAsync(string message)
    {
        const string url = "https://api.groq.com/openai/v1/chat/completions";
        const string model = "llama-3.3-70b-versatile";

        // var models = new[]
        // {
        //     "llama-3.3-70b-versatile",
        //     "codegen-350M-mono",
        //     "gemma2-9b-it",
        //     "llama-3.1-8b-instant",
        //     "llama-guard-3-8b",
        //     "llama3-70b-8192",
        //     "llama3-8b-8192",
        //     "whisper-large-v3",
        //     "distil-whisper-large-v3-en"
        // };
        
        var requestBody = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = message
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        var groqResponse = await JsonSerializer.DeserializeAsync<GroqResponse>(responseStream);

        return groqResponse?.Choices?[0]?.Message?.Content ?? "No content returned.";
    }
}

