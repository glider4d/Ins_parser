using Newtonsoft.Json;
using PuppeteerSharp; 

namespace InstagramParser.Interfaces
{
    public interface IInstagramParser
    {
        Task<bool> Login(IPage? page, bool saveCookies);

        Task Init(string userName, string password);
        Task<bool> SaveCookies(IPage? page);

        Task<CookieParam[]?> LoadCookies();
        
        Task SetCookies(IPage? page);

        Task<Dictionary<string, string>> GetImgFromPage(string url);
        Task<string[]> GetHrefFromPage(string url);
        Task<string> GetHtmlFromPage(string url);

        Task<IPage?> Prepare();
    }
}
