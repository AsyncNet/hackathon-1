namespace HackathonConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var confNo = args[1];

            var dbClient = new DBClient();
            var conference = dbClient.Read(875).Single();

        }
    }
}