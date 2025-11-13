namespace MauiApp1
{


    using Duende.IdentityModel.OidcClient.Browser;
    using Microsoft.Maui.Authentication;

    public class MauiWebBrowser : IBrowser
    {
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken ct = default)
        {
            try
            {
                // O WebAuthenticator faz o trabalho pesado
                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(options.StartUrl),
                    new Uri(options.EndUrl));

                // Retorna os dados que o OidcClient espera
                return new BrowserResult
                {
                    Response = result.Properties.ToQueryString(),
                    ResultType = BrowserResultType.Success
                };
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UserCancel,
                    ErrorDescription = "Login cancelado pelo usuário."
                };
            }
        }
    }

    // Classe auxiliar para converter o resultado
    internal static class WebAuthenticatorExtensions
    {
        public static string ToQueryString(this IDictionary<string, string> properties)
        {
            return string.Join("&", properties.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }
}