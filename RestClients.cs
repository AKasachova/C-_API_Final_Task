using NUnit.Framework;
using RestSharp;


namespace APIAutomation
{
    public class ClientForWriteScope
    {
        private static ClientForWriteScope _instance;
        private static readonly object _lock = new object();
        private readonly RestClient _client;

        private ClientForWriteScope(string baseURL, string clientUsername, string clientPassword)
        {
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, Scope.write)
            };

            _client = new RestClient(options);
        }

        public static ClientForWriteScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Retrieve parameters from configuration
                        string baseURL = TestContext.Parameters["BaseUrl"];
                        string clientUsername = TestContext.Parameters["ClientUsername"];
                        string clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForWriteScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public RestClient GetRestClient()
        {
            return _client;
        }
    }

    public class ClientForReadScope
    {
        private static ClientForReadScope _instance;
        private static readonly object _lock = new object();
        private readonly RestClient _client;

        private ClientForReadScope(string baseURL, string clientUsername, string clientPassword)
        {
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, Scope.read)
            };

            _client = new RestClient(options);
        }

        public static ClientForReadScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Retrieve parameters from configuration
                        string baseURL = TestContext.Parameters["BaseUrl"];
                        string clientUsername = TestContext.Parameters["ClientUsername"];
                        string clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForReadScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public RestClient GetRestClient()
        {
            return _client;
        }
    }
}
