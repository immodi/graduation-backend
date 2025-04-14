using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class AiController(GroqService groqService)
{
    public async Task<BaseResponse> ChatWithTheAi(AiRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new ErrorResponse("The request message is empty or doesn't contain exist");
        }

        try
        {
            var response = await groqService.GetFastLanguageModelExplanationAsync(request.Message);
            return new AiResponse(response);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
}
