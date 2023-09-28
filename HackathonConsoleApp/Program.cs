namespace HackathonConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // - read conference id
            //var confNo = args[1];

            // - get data from target (new) conference
            var dbClient = new DBClient();
            var conference = dbClient.Read(875).Single();

            // - send data to get closest conference
            var aiClient = new AiClient();
            await aiClient.Execute();
            // - parse response and read conference name

            // - send another request to AI with conference name
            
            // - parse response to extract emails

            // - add targets to target conference (only emails without creating a reference to PORTFOLIO table) - check if email exists

        }
    }
}