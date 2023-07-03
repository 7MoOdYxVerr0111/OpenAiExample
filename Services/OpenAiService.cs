using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenAI_API.Completions;
using OpenAIApp.Configurations;
using System.Net.Http;
using System.Text.RegularExpressions;

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
                new CompletionRequest(text, model: OpenAI_API.Models.Model.CurieText, temperature: 0.1));

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


        public async Task<List<int>> MakeAImove([FromBody] string[][] gameBoard)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            string instructions = "So we are going to play a game of Tic Tac Toe, I am the X player and you are going to be O." +
                "You don't need to ask me for my move, you just respond your move with O based on the game board I send you." +
                "I will send you the game board and you are going to send me your move in this format x,y  (x is number of the row and y is the number of the column) just an answer with x,y." +
                "The answer represent the coorect coordinates of your move in order to beat me ,rules x and y are between 0 and 2 and you can only choose a move where the value on the board is [ ], not [O] or [X]. The board is: \n";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                {
                    instructions +=  "[" + (gameBoard[i][j] == "" ? " " : gameBoard[i][j]) + "]";
                }
                instructions += "\n";
            }
            instructions += "Try to win the game against X based on the rules of Tic Tac Toe game to finish a line or a diagonal on the game board with O before X finishing one of them";
            var result = await api.Completions.CreateCompletionAsync(new CompletionRequest(instructions)
            {
                Model = "text-davinci-003",
                MaxTokens = 500
            });
            List<int> response = new List<int>();
            string pattern = @"[0-9]";
            MatchCollection matches = Regex.Matches(result.Completions[0].Text, pattern);  
            for (int i = 0; i < 2 && i < matches.Count; i++)
            {
                if (int.TryParse(matches[i].Value, out int number))
                {
                    response.Add(number);
                }
            }
            while(gameBoard[response[0]][response[1]] != "" || (response[0] > 2 || response[0] < 0) || (response[1] > 2 || response[1] < 0))
            {
                result= await api.Completions.CreateCompletionAsync(new CompletionRequest(instructions)
                {
                    Model = "text-davinci-003",
                    MaxTokens = 500
                });
                matches = Regex.Matches(result.Completions[0].Text, pattern);
                for (int i = 0; i < 2 && i < matches.Count; i++)
                {
                    if (int.TryParse(matches[i].Value, out int number))
                    {
                        response[i] = number;
                    }
                }
            }
            return response;
        }
    }
}
