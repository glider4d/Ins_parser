using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PuppeteerSharp;
using static System.Net.Mime.MediaTypeNames;


namespace InstagramPars.Services
{
    public class InstagramParser : IInstagramParser
    {
        private readonly string fileName = "_cookies.json";
        private string userName
        {
            get;
            set;
        } = string.Empty;
        private string password
        {
            get;
            set;
        } = string.Empty;

        private bool isInitialize
        {
            get; set;
        } = false;
        private bool isDownloadOptions
        {
            get; set;
        } = false;

        public async Task<bool> Login(IPage? page, bool saveCookies)
        {
            Console.WriteLine("Login in");
            bool result = true;
            if (isInitialize) return result;
            try
            {
                //  await page!.GoToAsync("https://instagram.com/");
                // Console.WriteLine("before Waite");


                try
                {
                    if (page != null)
                    {
                        await page?.WaitForNavigationAsync(new NavigationOptions { Timeout = 2000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } })!;
                    }
                }
                catch (Exception) { Console.WriteLine("timeout"); }
                Console.WriteLine("after waite");
                // await page.WaitForSelectorAsync("input[name=\"username\"]");
                var loginCheck = await page?.QuerySelectorAsync("input[name=\"username\"]")!;
                Console.WriteLine($"loginCheck = {loginCheck?.GetType()?.ToString()}");

                if (loginCheck != null)
                {
                    Console.WriteLine("loginCheck != null");
                    if (userName == string.Empty && password == string.Empty) throw new PrepareException("there aren't username and password");
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

                }
                else
                {
                    Console.WriteLine("loginCheck == null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login ex = {ex.Message}");
                result = false;
            }
            isInitialize = result;
            return result;
        }


        public async Task<LaunchOptions> InitDownloadOptions()
        {
            LaunchOptions _options = new LaunchOptions
            {
                Headless = true,
                Args = new string[] { "--no-sandbox" }
            };
            await DownloadChrome();

            return _options;
            //            DownloadChrome().ConfigureAwait(true).GetAwaiter().GetResult();

        }
        public async Task DownloadChrome()
        {
            BrowserFetcher browserFetcher = new BrowserFetcher();
            var folder = browserFetcher.DownloadsFolder;

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            if (System.Runtime.InteropServices.RuntimeInformation
                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                var launcher = new Launcher();
                var path = await launcher.GetExecutablePathAsync();
                Bash($"chmod 777 {path}");
            }
        }

        public void Bash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        public async Task FirstInit(string userName, string password)
        {
            Console.WriteLine("FirstInit in");
            try
            {
                this.userName = userName;
                this.password = password;
                // using var browserFetcher = new BrowserFetcher();
                // await browserFetcher.DownloadAsync();
                // isInitialize = true;
                isInitialize = false;
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);
                var page_browser = await Prepare();
                if (page_browser.Item1 != null)
                    await page_browser.Item1?.CloseAsync()!;
                if (page_browser.Item2 != null)
                   await page_browser.Item2?.CloseAsync()!;
//                await (await Prepare()).Item1?.CloseAsync();
//                await (await Prepare()).Item2?.CloseAsync()!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FirstInit ex = {ex.Message}");
            }
            Console.WriteLine("FirstInit out");
        }


        public async Task<string> Check()
        {
            IBrowser? browser = null;
            try
            {
                browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                var page = await browser.NewPageAsync();
                await page.GoToAsync("https://ipinfo.io/ip");

                var result = await page.GetContentAsync();
                await page.CloseAsync();
                await browser.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                await browser?.CloseAsync()!;
                return $"Check exception: {ex.Message}";
            }
        }
        public async Task<List<Tuple<string, string>>> GetResponseData(string url)
        {
            IBrowser? browser = null;
            IPage? page = null;
            List<Tuple<string, string>> data = new(); 
//            Dictionary<string, string> data = new();
            try
            {
                browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                page = await browser.NewPageAsync();
                
                page.Response += async (sender, e) =>
                {

                    var url = e.Response.Url;

                    try
                    {
                        var responseText = await e.Response.TextAsync();
                        if (responseText != null)
                        {
                            data.Add(new Tuple<string, string>(url, await e.Response.TextAsync()));
                            //data.Add(url, await e.Response.TextAsync());
                        }
                    } catch(Exception ex) { Console.WriteLine($"ex {ex.Message}"); }
                };
                /*
                page.Response += async (sender, e) =>
                {

                    var url = e.Response.Url;

                    if (url == "http://localhost:5235/api/Test/GetAjax?test=load")
                    {
                        Console.WriteLine("response");
                        var textAnswer = await e.Response.TextAsync();
                        Console.WriteLine(textAnswer);
                    }
                };*/

                try
                {
                    await page.GoToAsync(url, new NavigationOptions { Timeout = 16000, WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            catch (Exception e) { Console.WriteLine($"timeout: {e.Message}"); }
            if (page != null)   
                await page.CloseAsync();
            if (browser != null)
                await browser.CloseAsync();
            return data;
        }   
        public async Task<string> GetHtmlFromUrl(string url)
        {
            try
            {

//                var cookies = this.GetCookies();
                var browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                var page = await browser.NewPageAsync();

                try
                {
                    Console.WriteLine("before");
//                    await page.SetRequestInterceptionAsync(true);
                    page.Response += async (sender, e) =>
                    {

                        try
                        {
                            var url = e.Response.Url;

                            if (url == "http://localhost:5235/api/Test/GetAjax?test=load")
                            {
                                Console.WriteLine("response");
                                var textAnswer = await e.Response.TextAsync();
                                Console.WriteLine(textAnswer);
                            }
                        } catch(Exception ex)
                        {
                            Console.WriteLine($"response ex = {ex.Message}");
                        }


                    };
                    /*
                    page.Request += async (sender, e) => 
                    {
                        
                        Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
 
                    };*/
 
                    try
                    {
                        await page.GoToAsync(url, new NavigationOptions { Timeout = 16000, WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
//                    await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

                    //page.Response
                    Console.WriteLine("after");
                    // await page.GoToAsync(url, new NavigationOptions { Timeout = 3000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                    //                    await page.WaitForNavigationAsync(new NavigationOptions { Timeout = 2000, WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                }
                catch (Exception e) { Console.WriteLine($"timeout: {e.Message}"); }
                Console.WriteLine("after exception check");
                var result = await page.GetContentAsync();
                await page.CloseAsync();
                await browser.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                return $"GetHtmlFromUrl {url} {ex.Message}";
            }
        }

        public async Task<string> GetHtmlFromUrlWithWaitTag(string url, string waitTag)
        {
            try
            {
                var browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                var page = await browser.NewPageAsync();

                await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                await page.WaitForSelectorAsync(waitTag);
                var content = await page.GetContentAsync();
                await page.CloseAsync();
                await browser.CloseAsync();
                return content;
            }
            catch (Exception e)
            {
                return $"GetHtmlFromUrlWithWaitTag {url}, error: {e.Message}";

            }
        }
        //                     try{
        //             await page.WaitForNavigationAsync(new NavigationOptions {Timeout = 2000 ,WaitUntil = new[] { WaitUntilNavigation.Networkidle2}});
        //             } catch(Exception){Console.WriteLine("timeout");}
        //             Console.WriteLine("after waite");
        // // await page.WaitForSelectorAsync("input[name=\"username\"]");
        //                 var loginCheck = await page.QuerySelectorAsync("input[name=\"username\"]"); 

        public async Task<(IPage?,IBrowser?)> Prepare()
        {
            IBrowser browser;
            try
            {
                // if (!isInitialize) throw new PrepareException("isn't initialize");
                var dir = Directory.GetCurrentDirectory();
                Console.WriteLine($"dir = {dir}");
                Console.WriteLine("Prepare1");
                browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                // using var browserFetcher = new BrowserFetcher();
                // Console.WriteLine("Prepare2");
                // await browserFetcher.DownloadAsync();
                // Console.WriteLine("Prepare3");
                // var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                // {
                //     Headless = true
                // });
                Console.WriteLine("Prepare4");
                var page = await browser.NewPageAsync();
                Console.WriteLine("Prepare5");
                await SetCookies(page);
                Console.WriteLine("Prepare6");
                await page.GoToAsync("https://instagram.com/", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
//                await page.GoToAsync("https://instagram.com/");
                Console.WriteLine("Prepare7");


                await Login(page, true);
                Console.WriteLine("Prepare8");
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
                return (page, browser);
//                return Tuple<IPage?,IBrowser?>(page, browser);
  //              return page;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Prepare: {ex.Message}");
                throw new PrepareException("Prepare error!", ex);
            }
        }

        public async Task<bool> SaveCookies(IPage? page)
        {
            bool result = true;
            try
            {
                CookieParam[] results = await page?.GetCookiesAsync()!;
                Console.WriteLine($"SaveCookies: before save count = {results.Length}");

                string jsonSerialize = JsonConvert.SerializeObject(results);
                await System.IO.File.WriteAllTextAsync(fileName, jsonSerialize);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public async Task<CookieParam[]?> LoadCookies()
        {
            var readString = await System.IO.File.ReadAllTextAsync(fileName);
            var deserializeObj = JsonConvert.DeserializeObject<CookieParam[]>(readString);
            return deserializeObj;
        }

        public async Task SetCookies(IPage? page)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    var cookies = await LoadCookies();
                    await page!.SetCookieAsync(cookies);
                }
            }
            catch (Exception)
            {

            }


        }

        public async Task<List<Tuple<string, string>>> GetResponseInstagramData(string url)
        {
            (IPage?, IBrowser?) page_browser;
            List<Tuple<string, string>> data = new();
            try
            {
                page_browser = await Prepare();
                if (page_browser.Item1 != null)
                {



                    page_browser.Item1.Response += async (sender, e) =>
                    {
                        try
                        {
                            var url = e.Response.Url;


                            var responseText = await e.Response.TextAsync();
                            if (responseText != null)
                            {
                                data.Add(new Tuple<string, string>(url, await e.Response.TextAsync()));
                                //                                data.Add(url, await e.Response.TextAsync());
                            }
                            if (url == "http://localhost:5235/api/Test/GetAjax?test=load")
                            {
                                Console.WriteLine("response");
                                var textAnswer = await e.Response.TextAsync();
                                Console.WriteLine(textAnswer);
                            }
                        } catch (Exception ex)
                        {
                            Console.WriteLine($"ex {ex.Message}");
                        }
                    };
                    /*
                    page_browser.Item1.Request +=  (sender, e) =>
                    {

                        Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
 
                    };*/




                    await page_browser.Item1?.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } })!;
                    await page_browser.Item1.WaitForSelectorAsync(".x5yr21d");
 
                    await this.SaveCookies(page_browser.Item1);
                    await page_browser.Item1.CloseAsync();
                    if (page_browser.Item2 != null) {
                        await page_browser.Item2.CloseAsync();
                    }

                    return data;
                }

                if (page_browser.Item2 != null)
                    page_browser.Item2?.CloseAsync();
                throw new ContentException("GetResponseData exception");

            }
            catch (Exception ex)
            {
                throw new ContentException("GetResponseData exception", ex);
            }
        }

        public async Task<(byte[], string)> GetImgInstagramFromUrl(string url, string referer)
        {
            (IPage?, IBrowser?) page_browser;

            try
            {
                (byte[], string) result = new();
                page_browser = await Prepare();
                if (page_browser.Item1 != null)
                {


                    var tcs = new TaskCompletionSource();
                    page_browser.Item1.Response += async (sender, e) =>
                    {
                        string contentType = null;
                        string[] imageMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif", "video/mp4", "text/plain" };

                        string url = e.Response.Url;

                        if (e.Response.Headers.ContainsKey("content-type"))
                            contentType = (string)e.Response.Headers["content-type"].ToLower();

                        if (e.Response.Headers.ContainsKey("Content-Type"))
                            contentType = (string)e.Response.Headers["Content-Type"].ToLower();


                        Console.WriteLine($"contentType = {contentType}");
                        if (imageMimeTypes.Any(t => t == contentType))
                        {


                            try
                            {
                                var responseUrl = e.Response.Url;


                                result.Item1 = await e.Response.BufferAsync();
                                result.Item2 = responseUrl;
                                Console.WriteLine($"item2 = {result.Item2}");
                                Console.WriteLine("");
                                Console.WriteLine($"item1 = {result.Item1.Length}");
                                result.Item2 = contentType ?? string.Empty;
                            }
                            catch (Exception ex) { Console.WriteLine($"Response Img from url{ex.Message}"); }
                            tcs.TrySetResult();
                        }
                    };


                    await page_browser.Item1.SetExtraHttpHeadersAsync(new Dictionary<string, string>
                    {
                        ["Referer"] = referer
                    });

                    await Task.WhenAll(page_browser.Item1.GoToAsync(url), tcs.Task);

                    await page_browser.Item1.CloseAsync();
                    if (page_browser.Item2 != null)
                    {
                        await page_browser.Item2.CloseAsync();
                    }

                    return result;
                }

                if (page_browser.Item2 != null)
                    page_browser.Item2?.CloseAsync();
                throw new ContentException("GetImgInstagramFromUrl exception");

            }
            catch (Exception ex)
            {
                throw new ContentException("GetImgInstagramFromUrl exception", ex);
            }
        }


        public async Task<(byte[], string)> GetImgFromUrl(string url, string referer)
        {


            IBrowser? browser = null;
            try
            {
                (byte[], string) result = new();
                //                Image image = Image.FromFile(Path)
                browser = await Puppeteer.LaunchAsync(await InitDownloadOptions());
                var page = await browser.NewPageAsync();

                var tcs = new TaskCompletionSource();
                page.Response += async (sender, e) =>
                {
                    string contentType = null;
                    string[] imageMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

                    string url = e.Response.Url;

                    if (e.Response.Headers.ContainsKey("content-type"))
                        contentType = (string)e.Response.Headers["content-type"].ToLower();

                    if (e.Response.Headers.ContainsKey("Content-Type"))
                        contentType = (string)e.Response.Headers["Content-Type"].ToLower();

                    if (imageMimeTypes.Any(t => t == contentType))
                    {


                        try
                        {

                            result.Item1 = await e.Response.BufferAsync();
                            result.Item2 = contentType ?? string.Empty;
                        }
                        catch (Exception ex) { Console.WriteLine($"Response Img from url{ex.Message}"); }
                        tcs.TrySetResult();
                    }
                };



                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
                {
                    ["Referer"] = referer
                });

                await Task.WhenAll(page.GoToAsync(url), tcs.Task);



                await page.CloseAsync();
                await browser.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                await browser?.CloseAsync()!;
                throw new ContentException($"Check exception: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, string>> GetImgFromPage(string url)
        {
            Console.WriteLine("GetImgFromPage in");
            (IPage?, IBrowser?) page_browser;
            try
            {
                Console.WriteLine("GetImgFromPage 1");
                page_browser = await Prepare();
                //                var page = await Prepare();
                Console.WriteLine("GetImgFromPage 2");
                if (page_browser.Item1 != null)
                {
                    await page_browser.Item1?.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } })!;
                    //                await page_browser.Item1!.GoToAsync(url);
                    Console.WriteLine("GetImgFromPage 3");
                    await page_browser.Item1.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");
                    Console.WriteLine("GetImgFromPage 4");
                    var resultSrc = await page_browser.Item1.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.src);");
                    Console.WriteLine("GetImgFromPage 5");
                    var resultAlt = await page_browser.Item1.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('img')).map(a => a.alt);");
                    Console.WriteLine("GetImgFromPage 6");
                    var dictionary = resultSrc.Zip(resultAlt, (s, i) => new { s, i })
                                              .ToDictionary(item => item.s, item => item.i);
                    Console.WriteLine("GetImgFromPage 7");
                    await this.SaveCookies(page_browser.Item1);

                    await page_browser.Item1.CloseAsync();
                    Console.WriteLine("GetImgFromPage out");
                    if (page_browser.Item2 != null)
                        await page_browser.Item2.CloseAsync();
                    return dictionary;
                }

                if (page_browser.Item2 != null)
                    page_browser.Item2?.CloseAsync();
                throw new ContentException("GetImgFrompage exception");

            }
            catch (Exception ex)
            { 
                Console.WriteLine($"GetImgFromPage exception {ex.Message}");
                throw new ContentException("GetImgFrompage exception", ex);
            }
        }
        public async Task<string[]> GetHrefFromPage(string url)
        {
            try
            {
                var page_browser = await Prepare();
                //                await page_browser.Item1!.GoToAsync(url);
                if (page_browser.Item1 != null)
                {
                    await page_browser.Item1?.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } })!;
                    await page_browser.Item1?.WaitForSelectorAsync(".x5yr21d")!;//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");

                    string[] a_href = await page_browser.Item1.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('a')).map(a => a.href);");
                    await this.SaveCookies(page_browser.Item1);

                    await page_browser.Item1.CloseAsync();
                    if (page_browser.Item2 != null)
                        await page_browser.Item2?.CloseAsync()!;
                    return a_href;
                }
                throw new ContentException("GetHrefFromPage");
            }
            catch (Exception ex)
            {
                throw new ContentException("GetHrefFromPage", ex);
            }
        }
        public async Task<string> GetHtmlFromPage(string url)
        {
            try
            {
                var page_browser = await Prepare();
                //                await page_broser.Item1!.GoToAsync(url);

                if (page_browser.Item1 != null)
                {
                    await page_browser.Item1?.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } })!;

                    await page_browser.Item1.WaitForSelectorAsync(".x5yr21d");//, ".xu96u03", ".x10l6tqk", ".x13vifvy", ".x87ps6o", ".xh8yej3");
                    string result = await page_browser.Item1.GetContentAsync();
                    await this.SaveCookies(page_browser.Item1);
                    await page_browser.Item1.CloseAsync();
                    if (page_browser.Item2 != null)
                        await page_browser.Item2.CloseAsync();
                    return result;
                }
                throw new ContentException("GetHtmlFromPage");
            }
            catch (Exception ex)
            {
                throw new ContentException("GetHtmlFromPage", ex);
            }
        }

        public string GetFileCookies()
        {
            string result = string.Empty;
            // if (System.IO.File.Exists(fileName))
            {
                var dir = System.IO.Directory.GetCurrentDirectory();
                result = System.IO.Path.Combine(dir, fileName);
            }
            return result;
        }
        public string GetCookies()
        {
            string result = string.Empty;
            if (System.IO.File.Exists(fileName))
            {
                result = System.IO.File.ReadAllText(fileName);
            }
            return result;
        }

  
    }
}