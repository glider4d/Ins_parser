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
            Console.WriteLine("Login in");
            bool result = true;
            if (isInitialize) return result;
            try
            {
                 await page!.GoToAsync("https://instagram.com/");
                Console.WriteLine("before Waite");


            try{
            await page.WaitForNavigationAsync(new NavigationOptions {Timeout = 2000 ,WaitUntil = new[] { WaitUntilNavigation.Networkidle2}});
            } catch(Exception){Console.WriteLine("timeout");}
            Console.WriteLine("after waite");
// await page.WaitForSelectorAsync("input[name=\"username\"]");
                var loginCheck = await page.QuerySelectorAsync("input[name=\"username\"]"); 
                Console.WriteLine($"loginCheck = {loginCheck?.GetType()?.ToString()}");

                if (loginCheck != null)
                {
                    Console.WriteLine("loginCheck != null");
                    if ( userName == string.Empty && password == string.Empty) throw new PrepareException("there aren't username and password");
                    Console.WriteLine($"username = {userName}");
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
                    
                } else {
                    Console.WriteLine("loginCheck == null");
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"Login ex = {ex.Message}");
                result = false;
            }
            isInitialize = result;
            return result;
        }

        public async Task FirstInit(string userName, string password){
            try{
                this.userName = userName;
                this.password = password;
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                // isInitialize = true;
                isInitialize = false;
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);
                await (await Prepare())?.CloseAsync()!;
            } catch(Exception ex){
                Console.WriteLine($"FirstInit ex = {ex.Message}");
            }
        }

         

        public async Task<IPage?> Prepare(){
            try
            {
                // if (!isInitialize) throw new PrepareException("isn't initialize");
                var dir = Directory.GetCurrentDirectory();
                Console.WriteLine($"dir = {dir}");
                Console.WriteLine("Prepare1");
                using var browserFetcher = new BrowserFetcher();
                Console.WriteLine("Prepare2");
                await browserFetcher.DownloadAsync();
                Console.WriteLine("Prepare3");
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    args = "--no-sandbox"

                });
                Console.WriteLine("Prepare4");
                var page = await browser.NewPageAsync();
                Console.WriteLine("Prepare5");
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
            Console.WriteLine("GetImgFromPage in");
            try
            {
                Console.WriteLine("GetImgFromPage 1");
                var page = await Prepare();
                Console.WriteLine("GetImgFromPage 2");
                await page!.GoToAsync(url);
                Console.WriteLine("GetImgFromPage 3");
                await page.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");
                Console.WriteLine("GetImgFromPage 4");
                var resultSrc = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.src);");
                Console.WriteLine("GetImgFromPage 5");
                var resultAlt = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.alt);");
                Console.WriteLine("GetImgFromPage 6");
                var dictionary = resultSrc.Zip(resultAlt, (s, i) => new { s, i })
                                          .ToDictionary(item => item.s, item => item.i);
                Console.WriteLine("GetImgFromPage 7");
                await page.CloseAsync();
                Console.WriteLine("GetImgFromPage out");
                return dictionary;
       

            } catch(Exception ex)
            {
                Console.WriteLine("GetImgFromPage exception");
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

        public string GetFileCookies(){
            string result = string.Empty;
            if (System.IO.File.Exists(fileName)){
                var dir = System.IO.Directory.GetCurrentDirectory();
                result = System.IO.Path.Combine(dir, fileName);
            }
            return result;
        }
        public string GetCookies(){
            string result = string.Empty;
            if (System.IO.File.Exists(fileName)){
                result = System.IO.File.ReadAllText(fileName);
            }
            return result;
        }
    }
}