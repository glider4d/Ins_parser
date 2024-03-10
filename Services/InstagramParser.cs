using Newtonsoft.Json;
using PuppeteerSharp; 


namespace InstagramPars.Services
{
    public class InstagramParser : IInstagramParser
    {
        private readonly string fileName = "_cookies.json";
        private string userName{
            get;
            set;
        } = string.Empty;
        private string password{
            get;
            set;
        } = string.Empty;

        private bool isInitialize{
            get;set;
        } = false;


        public async Task<bool> Login(IPage? page, bool saveCookies){
            bool result = true;
            if (isInitialize) return result;
            try
            {
                 await page!.GoToAsync("https://instagram.com/");
  


                var loginCheck = await page.QuerySelectorAsync("input[name=\"username\"]"); 
                Console.WriteLine($"loginCheck = {loginCheck?.GetType()?.ToString()}");

                if (loginCheck != null)
                {
                    if ( userName == string.Empty && password == string.Empty) throw new PrepareException("there aren't username and password");
                    await page.WaitForSelectorAsync("input[name=\"username\"]");
                    await page.TypeAsync("input[name=\"username\"]", userName);//""
                     await page.TypeAsync("input[name=\"password\"]", password);//""
                    await page.ClickAsync("button[type=\"submit\"]");

                     await page.WaitForNavigationAsync();
                     var savebutton = page.QuerySelectorAllHandleAsync("button._acan");
                    if (savebutton != null)
                    {
                        await page.ClickAsync("button._acan");
                        await page.WaitForNavigationAsync();

                    }
                    if (saveCookies) await SaveCookies(page);
                    
                }
            } catch(Exception)
            {
                result = false;
            }
            isInitialize = result;
            return result;
        }

        public async Task FirstInit(string userName, string password){
            this.userName = userName;
            this.password = password;
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            // isInitialize = true;
            isInitialize = false;
            await (await Prepare())?.CloseAsync()!;
        }

         

        public async Task<IPage?> Prepare(){
            try
            {
                // if (!isInitialize) throw new PrepareException("isn't initialize");
                var dir = Directory.GetCurrentDirectory();
                Console.WriteLine($"dir = {dir}");
                using var browserFetcher = new BrowserFetcher();

                await browserFetcher.DownloadAsync();
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });
                var page = await browser.NewPageAsync();
                await SetCookies(page);
                await page.GoToAsync("https://instagram.com/");
            
              
                
                await Login(page, true);
                
                var checkDialogNotification = await page.QuerySelectorAsync("button._a9_1");
                if (checkDialogNotification == null)
                {
                    Console.WriteLine("==null");
                }
                else
                {
                    Console.WriteLine("!=null");
                    await page.WaitForSelectorAsync("button._a9_1");
                    await page.ClickAsync("button._a9_1");
                }
                return page;

            } catch(Exception ex)
            {
                throw new PrepareException("Prepare error!", ex);
            }
        }

        public async Task<bool> SaveCookies(IPage? page){
            bool result = true;
            try{
                CookieParam[] results = await page?.GetCookiesAsync()!;
                Console.WriteLine($"SaveCookies: before save count = {results.Length}");

                string jsonSerialize = JsonConvert.SerializeObject(results);
                await System.IO.File.WriteAllTextAsync(fileName, jsonSerialize);
            } catch(Exception){
                result = false;
            }
            return result;
        }

        public async Task<CookieParam[]?> LoadCookies(){
            var readString = await System.IO.File.ReadAllTextAsync(fileName);
            var deserializeObj = JsonConvert.DeserializeObject<CookieParam[]>(readString);
            return deserializeObj;
        }
        
        public async Task SetCookies(IPage? page){
            try{
                if (System.IO.File.Exists(fileName))
                {
                    var cookies = await LoadCookies();
                    await page!.SetCookieAsync(cookies);
                } 
            } catch(Exception){

            }


        }

        public async Task<Dictionary<string, string>> GetImgFromPage(string url){
            try
            {
                var page = await Prepare();
                
                await page!.GoToAsync(url);
                await page.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");
            
                var resultSrc = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.src);");
                var resultAlt = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.alt);");
                var dictionary = resultSrc.Zip(resultAlt, (s, i) => new { s, i })
                                          .ToDictionary(item => item.s, item => item.i);

                await page.CloseAsync();
                return dictionary;
       

            } catch(Exception ex)
            {
                throw new ContentException("GetImgFrompage exception", ex);
            }
        }
        public async Task<string[]> GetHrefFromPage(string url){
            try{
                var page = await Prepare();
                await page!.GoToAsync(url);
                await page.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");

                string[] a_href = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('a')).map(a => a.href);");
                await page.CloseAsync();
                return a_href;
            } catch(Exception ex){
                throw new ContentException("GetHrefFromPage", ex);
            }
        }
        public async Task<string> GetHtmlFromPage(string url){
            try{
                var page = await Prepare();
                await page!.GoToAsync(url);
                
                await page.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");
                string result = await page.GetContentAsync();
                await page.CloseAsync();
                return result;
            } catch(Exception ex){
                throw new ContentException("GetHtmlFromPage", ex);
            }
        }
    }
}