using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ConsoleApp1
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();


        public static string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")  ?? string.Empty;
        public static string endpointURL = "https://api.openai.com/v1/chat/completions"; 
       public static string modelType = "gpt-3.5-turbo";
        public static int maxTokens = 256;


        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                var response = await GetOpenAiResponse(args[0]); // Call the GetOpenAiResponse method on the instance

                //Print the response to the console
                Console.WriteLine(response);
            }
            else{
                Console.WriteLine("No arguments provided. Going with Default Question of What is AI?");
                
                //Program program = new Program(); // Create an instance of the Program class
                var response = await GetOpenAiResponse("What is AI?"); // Call the GetOpenAiResponse method on the instance

                //Print the response to the console
                Console.WriteLine(response);
            }
        }

        public static async Task<string> GetOpenAiResponse(string query)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string prompt = $"User: {query}\nChatGPT:";
            // Create message objects for the system and user messages
            var messages = new List<object>
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = query }
            };
            // Create a JSON object for the request payload
            var requestBody = new
            {

                model = modelType,
                messages = messages,
                max_tokens = maxTokens
            };

            // Serialize the JSON object to a string
            var jsonContent = JsonConvert.SerializeObject(requestBody);

            // Use StringContent with the correct content type
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpointURL, content);


            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();

                // Parsing JSON to extract the response text
                var jsonResponse = JObject.Parse(result);
                var choices = jsonResponse["choices"];

                var firstChoice = choices?.FirstOrDefault(); // Add null check here

                var text = firstChoice?["message"]?["content"]?.ToString(); // Add null check here

                return text ?? "Unable to parse OpenAI response.";
            }
            else
            {
                Console.WriteLine($"Error communicating with OpenAI API. Status code: {response.StatusCode}");
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");

                return "Error communicating with OpenAI API.";
            }
        }
    }
}