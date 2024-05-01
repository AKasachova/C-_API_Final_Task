using RestSharp;
using RestSharp.Authenticators;

namespace APIAutomation
{
    public enum Scope
    {
        read,
        write
    }
    public class BasicAuthenticator : AuthenticatorBase
    {
        readonly string _baseUrl;
        readonly string _clientUsername;
        readonly string _clientPassword;
        readonly Scope _scope;

        public BasicAuthenticator(string baseUrl, string clientUsername, string clientPassword, Scope scope) : base("")
        {
            _baseUrl = baseUrl;
            _clientUsername = clientUsername;
            _clientPassword = clientPassword;
            _scope = scope;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        public async Task<string> GetToken()
        {
            var options = new RestClientOptions(_baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_clientUsername, _clientPassword),
            };
            using var client = new RestClient(options);

            var request = new RestRequest("oauth/token").AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", _scope);
            var response = await client.PostAsync<TokenResponse>(request);
            return $"{response!.TokenType} {response!.AccessToken}";
        }
    }
}
