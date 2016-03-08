using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using SignalRChat.Core;

namespace SignalRChat.Console
{
    class Program
    {
        static IHubProxy _hub;
        private static ClientRegistration _registrationInfo;

        static void Main(string[] args)
        {
            StartListener();
            AttemptToRegister();
            HandleUserInput();
        }

        private static void AttemptToRegister()
        {
            Task.Run(async () =>
            {
                _registrationInfo = await _hub.Invoke<ClientRegistration>("Register");
            }).Wait();
        }

        private static void HandleUserInput()
        {
            string userInput;
            do
            {
                userInput = (System.Console.ReadLine() ?? "").Trim();
                System.Console.Write($"\r{SpacesFor(userInput)}\r");
                System.Console.Out.Flush();
                if (ProcessedCommandFrom(userInput))
                    continue;
                _hub.Invoke("SendMessage", new MessageWrapper()
                {
                    Sender = _registrationInfo,
                    Message = userInput
                });
            } while (userInput != "/quit");
        }

        private static string SpacesFor(string userInput)
        {
            return new string(' ', userInput.Length);
        }

        private static bool ProcessedCommandFrom(string userInput)
        {
            if (!userInput.StartsWith("/"))
                return false;
            return true;
        }

        private static void StartListener()
        {
            Task.Run(async () =>
            {
                var host = "http://ch4tz.azurewebsites.net";
                var connection = new HubConnection(host);
                var hub = connection.CreateHubProxy("ChatHub");
                await connection.Start();
                _hub = hub;
                hub.On<MessageWrapper>("SendMessage", ReceiveMessage);
            }).Wait();
        }

        static void ReceiveMessage(MessageWrapper data)
        {
            var sender = data.Sender?.Name ?? "(unknown)";
            var message = data.Message ?? "";
            System.Console.WriteLine($"{sender} says: {message}");
        }
    }
}
