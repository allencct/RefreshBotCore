using Hangfire.Console;
using Hangfire.Server;
using PuppeteerSharp;
using RefreshWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefreshWeb.Jobs
{
    public class CheckTargetJob
    {
        private readonly DataService _dataService;

        public CheckTargetJob(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task ExecuteAsync(PerformContext performContext)
        {
            var targets = await _dataService.GetActiveTargetPagesAsync();
            if (!targets.Any())
            {
                performContext.WriteLine("No active target pages.");
                return;
            }

            //foreach(var target in targets)
            //{
            //    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            //    var options = new LaunchOptions()
            //    {
            //        Args = new[] { "--no-sandbox" },
            //        Headless = true,
            //        //ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
            //    };
            //    var browser = await Puppeteer.LaunchAsync(options);
            //    var page = await browser.NewPageAsync();
            //    //await page.GoToAsync("https://old.reddit.com/r/buildapc/search/?q=ssd&sort=new&restrict_sr=on&t=all");
            //    await page.GoToAsync(url);
            //    var stream = await page.ScreenshotStreamAsync();
            //    var content = await page.GetContentAsync();
            //}

          
        }
    }
}
