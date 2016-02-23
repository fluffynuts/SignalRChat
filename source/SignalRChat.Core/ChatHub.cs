using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR;

namespace SignalRChat.Core
{
    public class MessageWrapper
    {
        public ClientRegistration Sender { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
    }

    public class ClientRegistration
    {
        private static int _guestIndex = 1;
        public string Name { get; set; }
        public Guid Identifier { get; private set; }
        public ClientRegistration()
        {
            Identifier = Guid.NewGuid();
            Name = "guest_" + (_guestIndex++);
        }
    }

    public class ClientRegistrationRepository
    {
        private ConcurrentDictionary<Guid, ClientRegistration> _clients = new ConcurrentDictionary<Guid, ClientRegistration>(); 
        public ClientRegistration Create()
        {
            var clientRegistration = new ClientRegistration();
            _clients[clientRegistration.Identifier] = clientRegistration;
            return clientRegistration;
        }

        public ClientRegistration Find(Guid id)
        {
            ClientRegistration existing;
            return _clients.TryGetValue(id, out existing) ? existing: null;
        }

        public static ClientRegistrationRepository Instance { get; } = new ClientRegistrationRepository();
    }


    public class ChatHub: Hub
    {
        public ChatHub()
        {
            System.Diagnostics.Debug.WriteLine($"Creating new instance of {GetType().Name}");
        }
        public void SendMessage(MessageWrapper messageWrapper)
        {
            Clients.All.sendMessage(messageWrapper);
        }

        public ClientRegistration Register()
        {
            var clientRegistration = ClientRegistrationRepository.Instance.Create();
            Clients.All.addClient(clientRegistration);
            return clientRegistration;
        }

        public ClientRegistration ValidateRegistration(ClientRegistration registration)
        {
            return ClientRegistrationRepository.Instance.Find(registration.Identifier);
        }

        public void SetName(ClientRegistration registration)
        {
            var existing = ClientRegistrationRepository.Instance.Find(registration.Identifier);
            if (existing == null)
                return;
            existing.Name = registration.Name;
        }
    }
}
