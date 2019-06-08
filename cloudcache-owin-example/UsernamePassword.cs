using Apache.Geode.Client;


namespace cloudcache_ownin_example
{
    class UsernamePassword : IAuthInitialize
    {
        private string username;
        private string password;

        public UsernamePassword(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public void Close()
        {
        }

        public Properties<string, object> GetCredentials(Properties<string, string> props, string server)
        {
            var credentials = new Properties<string, object>();
            credentials.Insert("security-username", username);
            credentials.Insert("security-password", password);
            return credentials;
        }
    }
}
