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

        string GetFileCookies();
        string GetCookies();

        Task<IPage?> Prepare();
    }
}