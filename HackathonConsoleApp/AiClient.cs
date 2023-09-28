using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;


namespace HackathonConsoleApp
{

    internal class AiClient
    {
        public async Task Execute()
        {
            // Azure OpenAI setup
            var apiBase = "https://hackaton2023.openai.azure.com/"; // Add your endpoint here
            var apiKey = "f19a3b5827434135be6838f0b7fb1907"; //Environment.GetEnvironmentVariable("OPENAI_API_KEY"); // Add your OpenAI API key here
            var deploymentId = "gpt-4_0613"; // Add your deployment ID here

            // Azure Cognitive Search setup
            var searchEndpoint = "https://hackaton2023.openai.azure.com/openai/deployments/gpt-4_0613/extensions/chat/completions?api-version=2023-07-01-preview"; // Add your Azure Cognitive Search endpoint here
            var searchKey = "idxIp839RL9yFsUIRcaq14pxKxwOmaUkGVMRIhkiV4AzSeBKKk8K"; // Environment.GetEnvironmentVariable("SEARCH_KEY"); // Add your Azure Cognitive Search admin key here
            var searchIndexName = "confindex3"; // Add your Azure Cognitive Search index name here
            var client = new OpenAIClient(new Uri(apiBase), new AzureKeyCredential(apiKey!));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
            {
                new ChatMessage(ChatRole.User, "Find closest conference where conference is located in Tokyo. Conference sectors are: Automobile Components, Automobiles, Chemicals, Energy Equipment & Services. Conference presenters are:  DAIZ INC., estie.")
            },
                // The addition of AzureChatExtensionsOptions enables the use of Azure OpenAI capabilities that add to
                // the behavior of Chat Completions, here the "using your own data" feature to supplement the context
                // with information from an Azure Cognitive Search resource with documents that have been indexed.
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                {
                    new AzureCognitiveSearchChatExtensionConfiguration()
                    {
                        SearchEndpoint = new Uri(searchEndpoint),
                        IndexName = searchIndexName,
                        SearchKey = new AzureKeyCredential(searchKey!),
                    }
                }
                }
            };

            var response = await client.GetChatCompletionsAsync(
                deploymentId,
                chatCompletionsOptions);

            var message = response.Value.Choices[0].Message;
            // The final, data-informed response still appears in the ChatMessages as usual
            Console.WriteLine($"{message.Role}: {message.Content}");
            // Responses that used extensions will also have Context information that includes special Tool messages
            // to explain extension activity and provide supplemental information like citations.
            Console.WriteLine($"Citations and other information:");
            foreach (var contextMessage in message.AzureExtensionsContext.Messages)
            {
                // Note: citations and other extension payloads from the "tool" role are often encoded JSON documents
                // and need to be parsed as such; that step is omitted here for brevity.
                Console.WriteLine($"{contextMessage.Role}: {contextMessage.Content}");
            }

        }
    }
}
