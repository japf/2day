using System;

namespace Chartreuse.Today.Exchange.Ews
{
    public class EwsRequestSettings
    {
        public string Email { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public string ServerUri { get; private set; }

        public EwsRequestSettings(string email, string username, string password, string serverUri)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (serverUri == null)
                throw new ArgumentNullException(nameof(serverUri));

            this.Email = email;
            this.Username = username;
            this.Password = password;
            this.ServerUri = serverUri;
        }
    }
}