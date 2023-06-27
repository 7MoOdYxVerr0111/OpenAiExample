using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAIApp.Configurations;
using System.Net.Http;

namespace OpenAIApp.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiConfig _openAiConfig;
        private readonly HttpClient httpClient;
        public OpenAiService(
            IOptionsMonitor<OpenAiConfig> optionsMonitor
        )
        {
            _openAiConfig = optionsMonitor.CurrentValue;
            httpClient = new HttpClient();
        }

        public async Task<string> CheckProgrammingLanguage(string language)
        {

            //api instance
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var chat = api.Chat.CreateConversation();
            chat.AppendSystemMessage("You are a teacher who help new programmers understand things are pgrogramming language or not." +
                " If the user tells you a programming language respond with yes, if a user tells you somehting which is not a programming language respond with no." +
                " You will only respond wiht yes or no. You do not say anything else.");

            chat.AppendUserInput(language);

            var response = await chat.GetResponseFromChatbotAsync();
            return response;
        }

        public async Task<string> CompleteSentence(string text)
        {
            //api instance
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var result = await api.Completions.GetCompletion(text);
            return result;
        }

        public async Task<string> CompleteSentenceAdvance(string text)
        {
            //api instance
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);

            var result = await api.Completions.CreateCompletionAsync(
                new CompletionRequest(text, model: Model.CurieText, temperature: 0.1));

            return result.Completions[0].Text;
        }

        public async Task<string> GenerateResponseFromFile()
        {
            try
            {
                string instructions = await ReadFileContents("Controllers/payload.txt");
                var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
                var result = await api.Completions.CreateCompletionAsync(new CompletionRequest(instructions)
                {
                    Model = "text-davinci-003",
                    MaxTokens = 500
                });
                string response = result.Completions[0].Text;
                return response;
            }
            catch (Exception ex)
            {
                return($"Error generating response: {ex.Message}");
            }
        }

        public async Task<string> ReadFileContents(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }

    }
}
