using System.Text;
using System.Text.Json;
using backend.DTOs.Requests;

namespace backend.Services;

public class GroqService(string apiKey)
{
    private readonly HttpClient _httpClient = new();
    
    public async Task<string> GetFastLanguageModelExplanationAsync(string message)
    {
        const string url = "https://api.groq.com/openai/v1/chat/completions";
        const string model = "llama-3.3-70b-versatile";

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

