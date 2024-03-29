
using InstagramPars.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.IO;



namespace InstagramParser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstagramController : ControllerBase
    {


        private readonly ILogger<InstagramController> _logger;
        private readonly IInstagramParser _parser;
        public InstagramController(ILogger<InstagramController> logger, IInstagramParser parser)
        {
            _logger = logger;
            _parser = parser;
        }
        /*
        [HttpGet(nameof(login))]
        public async Task<IActionResult> login(IPage? page, bool saveCookies)
        {

            bool result = await _parser.Login(page, saveCookies);
            return result ? Ok(result) : BadRequest(result);
        }
        */
        [HttpGet(nameof(FirstInit))]
        public async Task<IActionResult> FirstInit(string username, string password)
        {
            try
            {
                await _parser.FirstInit(username, password);
                return Ok("Init is fine");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet(nameof(GetHtmlFromUrl))]
        public async Task<IActionResult> GetHtmlFromUrl(string url)
        {
            try
            {

                return Ok(await _parser.GetHtmlFromPage(url));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet(nameof(GetImgFromUrl))]
        public async Task<IActionResult> GetImgFromUrl(string url)
        {

            try
            {
                Console.WriteLine("GetImgFromUrl before ok");
                return Ok(await _parser.GetImgFromPage(url));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetImgFromUrl exception {ex.Message}");
                return BadRequest(ex);
            }
        }
        [HttpGet(nameof(GetHrefFromUrl))]
        public async Task<IActionResult> GetHrefFromUrl(string url)
        {
            try
            {
                return Ok(await _parser.GetHrefFromPage(url));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet(nameof(GetCookies))]
        public IActionResult GetCookies() => Ok(_parser.GetCookies());

        [HttpGet(nameof(GetFileCookies))]
        public IActionResult GetFileCookies() => Ok(_parser.GetFileCookies());

        [HttpGet(nameof(GetHttp))]
        public async Task<IActionResult> GetHttp()
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://yandex.ru");

            return Ok(response.Content);
        }
        [HttpGet(nameof(Check))]
        public async Task<IActionResult> Check() => Ok(await _parser.Check());

        [HttpGet(nameof(GetContent))]
        public async Task<IActionResult> GetContent(string url) => Ok(await _parser.GetHtmlFromUrl(url));

        [HttpGet(nameof(GetContentWithTag))]
        public async Task<IActionResult> GetContentWithTag(string url, string waitTag) => Ok(await _parser.GetHtmlFromUrlWithWaitTag(url, waitTag));
        [HttpGet(nameof(GetResponseData))]
        public async Task<IActionResult> GetResponseData(string url) => Ok(await _parser.GetResponseData(url));
        [HttpGet(nameof(GetResponseInstagramData))]
        public async Task<IActionResult> GetResponseInstagramData(string url) => Ok(await _parser.GetResponseInstagramData(url));


        [HttpGet(nameof(GetImage))]
        public async Task<IActionResult> GetImage(string url, string referer)
        {


            var result = await _parser.GetImgFromUrl(url, referer);

            if (result.Item1.Length > 0)
                return File(result.Item1, result.Item2);
            
            return NotFound();
        }


        [HttpGet(nameof(GetInstagramImageCurl))]
        public async Task<IActionResult> GetInstagramImageCurl(string url, string referer, string filename)
        {
            Console.WriteLine("GetInstagramimageCurl in");
            Console.WriteLine("GetInstagramVideo in");


            var client = new HttpClient();


            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");



            client.DefaultRequestHeaders.Add("authority", "scontent.cdninstagram.com");
            client.DefaultRequestHeaders.Add("accept-language", " ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.");
            client.DefaultRequestHeaders.Add("origin", "https://www.instagram.com");
            client.DefaultRequestHeaders.Add("Referer", referer);
            client.DefaultRequestHeaders.Add("'sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            // h.DefaultRequestHeaders.Add("Content-Type","application/json; charset=UTF-8");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");

            Stream stream2 = await client.GetStreamAsync(url);




            if (stream2 == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.
            Console.WriteLine("GetInstagramVideo out");

            return File(stream2, "application/octet-stream", filename.Length == 0 ? "insfile.png" : filename); // returns a FileStreamResult



            Console.WriteLine("GetInstagramImageCurl out");
        }

        [HttpGet(nameof(GetInstagramVideo))]
        public async Task<IActionResult> GetInstagramVideo(string url, string referer, string filename)
        {
            Console.WriteLine("GetInstagramVideo in");


            var client = new HttpClient();


            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            


            client.DefaultRequestHeaders.Add("authority", "scontent.cdninstagram.com");
            client.DefaultRequestHeaders.Add("accept-language", " ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.");
            client.DefaultRequestHeaders.Add("origin", "https://www.instagram.com");
            client.DefaultRequestHeaders.Add("Referer", referer);
            client.DefaultRequestHeaders.Add("'sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            // h.DefaultRequestHeaders.Add("Content-Type","application/json; charset=UTF-8");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site"); 

            Stream stream2 = await client.GetStreamAsync(url);




            if (stream2 == null)
                return NotFound(); // returns a NotFoundResult with Status404NotFound response.
            Console.WriteLine("GetInstagramVideo out");

            return File(stream2, "application/octet-stream", filename.Length==0? "insfile.mp4": filename); // returns a FileStreamResult
 
        }


        [HttpGet(nameof(GetInstagramImage))]
        public async Task<IActionResult> GetInstagramImage(string url, string referer)
        {
            var result = await _parser.GetImgInstagramFromUrl(url, referer);
            if (result.Item1.Length > 0) return File(result.Item1, result.Item2);
            

            return NotFound();
        }
        /*{
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();            
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });            
            var page = await browser.NewPageAsync();
            await page.GoToAsync("https://github.com/");
            return Ok(await page.GetContentAsync());
        }*/
    }
}
