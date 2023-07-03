using Microsoft.AspNetCore.Mvc;

namespace OpenAIApp.Services
{
    public interface IOpenAiService
    {
        Task<string> CompleteSentence(string text);

        Task<string> CompleteSentenceAdvance(string text);

        Task<string> CheckProgrammingLanguage(string language);

        Task<string> GenerateResponseFromFile();
        Task<List<int>> MakeAImove([FromBody] string[][] gameBoard);
    }
}
