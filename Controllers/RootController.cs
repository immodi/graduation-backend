using backend.DTOs.Responses;
using backend.Helpers;

namespace backend.Controllers;

public class RootController
{
    private readonly string[] _supportedLanguages = ProgrammingLanguages.Commands.Keys.ToArray();
    public BaseResponse Index()
    {
        return new LanguagesResponse(_supportedLanguages);
    }
}
