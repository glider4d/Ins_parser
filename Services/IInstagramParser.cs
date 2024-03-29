using Newtonsoft.Json;
using PuppeteerSharp;

namespace InstagramPars.Services
{
    public interface IInstagramParser
    {
        Task<bool> Login(IPage? page, bool saveCookies);

        Task FirstInit(string userName, string password);
        Task<bool> SaveCookies(IPage? page);

        Task<CookieParam[]?> LoadCookies();

        Task SetCookies(IPage? page);

        Task<Dictionary<string, string>> GetImgFromPage(string url);
        Task<string[]> GetHrefFromPage(string url);
        Task<string> GetHtmlFromPage(string url);
        Task<List<Tuple<string, string>>> GetResponseInstagramData(string url);
        Task<List<Tuple<string, string>>> GetResponseData(string url);
        string GetFileCookies();
        string GetCookies();
        Task<(IPage?, IBrowser?)> Prepare();
        Task DownloadChrome();
        void Bash(string cmd);

        Task<LaunchOptions> InitDownloadOptions();
        Task<(byte[], string)> GetImgInstagramFromUrl(string url, string referer);
        Task<(byte[], string)> GetImgFromUrl(string url, string referer);
        Task<string> Check();
        Task<string> GetHtmlFromUrl(string url);
        Task<string> GetHtmlFromUrlWithWaitTag(string url, string waitTag);

    }
}