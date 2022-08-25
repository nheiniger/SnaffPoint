using PnP.Core.Auth;
using System;
using System.Threading.Tasks;

namespace GetBearerToken
{
    class Program
    {
        private static string url = "";
        private static string _adalToken = null;
        private static InteractiveAuthenticationProvider _interactiveProvider;

        // shameless rip off https://github.com/pnp/PnP-Tools/blob/master/Solutions/SharePoint.Search.QueryTool/SearchQueryTool/MainWindow.xaml.cs
        private static async Task<string> AdalLogin(string url)
        {
            var spUri = new Uri(url);

            var resourceUri = new Uri(spUri.Scheme + "://" + spUri.Authority);
            const string clientId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
            const string redirectUri = "https://oauth.spops.microsoft.com/";

            string tenant = spUri.Host.Replace("sharepoint", "onmicrosoft")
                    .Replace("-df", "")
                    .Replace("-admin", "");

            if (_interactiveProvider == null || _interactiveProvider.TenantId != tenant)
            {
                _interactiveProvider = new InteractiveAuthenticationProvider(clientId, tenant, new Uri(redirectUri));
            }

            _adalToken = await _interactiveProvider.GetAccessTokenAsync(resourceUri);
            return "Bearer " + _adalToken;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                System.Console.WriteLine("Please enter your SharePoint online URL.");
                return 1;
            }
            url = args[0];

            try
            {
                var token = AdalLogin(url);
                token.Wait();
                Console.WriteLine(_adalToken);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went horribly wrong, I'm so sorry: " + e.Message);
                return 1;
            }
            return 0;
        }
    }
}
