
using InstagramPars.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp; 



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

        [HttpGet(nameof(login))]
        public  async Task<IActionResult> login(IPage? page, bool saveCookies)
        {

            bool result = await _parser.Login(page, saveCookies);
            return result? Ok (result) : BadRequest(result);
        }
 
        [HttpGet(nameof(FirstInit))]
        public async Task<IActionResult> FirstInit(string username, string password){
            try{
                await _parser.FirstInit(username, password);
                return Ok("Init is fine");
            } catch(Exception ex){
                return BadRequest(ex);
            }
        }
        [HttpGet(nameof(GetHtmlFromUrl))]
        public async Task<IActionResult> GetHtmlFromUrl(string url){
            try{
            
                return Ok(await _parser.GetHtmlFromPage(url));
            } catch(Exception ex){
                return BadRequest(ex);
            }
        }
         
        [HttpGet(  nameof(GetImgFromUrl))]
        public async Task<IActionResult> GetImgFromUrl(string url) 
        {

            try{
                Console.WriteLine("GetImgFromUrl before ok");
                return Ok(await _parser.GetImgFromPage(url));
            } catch(Exception ex){
                Console.WriteLine("GetImgFromUrl exception");
                return BadRequest(ex);
            }
        }  
        [HttpGet( nameof(GetHrefFromUrl))]
        public async Task<IActionResult> GetHrefFromUrl(string url)
        {
            try {
                return Ok(await _parser.GetHrefFromPage(url));
            } catch(Exception ex){
                return BadRequest(ex);
            }
        }
        [HttpGet(nameof(GetCookies))]
        public IActionResult GetCookies() => Ok(_parser.GetCookies());
        
        [HttpGet(nameof(GetFileCookies))]
        public IActionResult GetFileCookies() => Ok(_parser.GetFileCookies());

        [HttpGet(nameof(GetHttp))]
        public async Task<IActionResult> GetHttp(){
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://yandex.ru");
        
            return Ok(response.Content);
        }
       
    }
}
