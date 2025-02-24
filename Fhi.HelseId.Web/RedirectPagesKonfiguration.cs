using System.Linq;

namespace Fhi.HelseId.Web
{
    public interface IRedirectPagesKonfigurasjon
    {
        string Forbidden { get; }
        string LoggedOut { get; }
        string Error { get; }
        string Statuscode { get; }
    }

    /// <summary>
    /// Default redirect pages
    /// </summary>
    public class RedirectPagesKonfigurasjon : IRedirectPagesKonfigurasjon
    {
        public string Forbidden { get; set; } = "/Forbidden.html";
        public string LoggedOut { get; set; } = "/Loggedout.html";
        public string Error { get; set; } = "/Error.html";
        public string Statuscode { get; set; } = "/Statuscode.html";

        public bool KonfigurasjonErGyldig()
        {
            return
                new[]
                {
                    Forbidden,
                    LoggedOut,
                    Error,
                    Statuscode
                }.All(url => url.StartsWith("/"));
        }
    }
}
