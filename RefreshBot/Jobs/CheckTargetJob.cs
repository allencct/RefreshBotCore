using Discord.WebSocket;
using GroupDocs.Comparison;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using RefreshBot.DataAccess;
using RefreshBot.Options;
using SimpleImageComparisonClassLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RefreshBot.Jobs
{
    public class CheckTargetJob
    {
        private readonly PageOptions _options;
        private readonly DiscordSocketClient _client;
        private readonly EntityContext _entityContext;

        public CheckTargetJob(IOptions<PageOptions> options, DiscordSocketClient client, EntityContext entityContext)
        {
            _options = options.Value;
            _client = client;
            _entityContext = entityContext;
        }

        public async Task ExecuteAsync()
        {
            try
            {

                var targets = await _entityContext.TargetPages.AsAsyncEnumerable().Where(t => t.IsActive).ToListAsync();
                if (!targets.Any())
                {
                    return;
                }

                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                var options = new LaunchOptions()
                {
                    Args = new[] { "--no-sandbox" },
                    Headless = true,
                };
                var browser = await Puppeteer.LaunchAsync(options);

                foreach (var target in targets)
                {
                    var page = await browser.NewPageAsync();
                    await page.SetViewportAsync(new ViewPortOptions { Width = _options.Width, Height = _options.Height });

                    await page.GoToAsync(target.Url);


                    //var content = await page.GetContentAsync();
                    //var dbStrippedContent = Regex.Replace(target.Content, "<.+?>", "");
                    //var strippedContent = Regex.Replace(content, "<.+?>", "");

                    //if (dbStrippedContent.Equals(strippedContent, StringComparison.OrdinalIgnoreCase))
                    //    continue;

                    var data = await page.ScreenshotDataAsync();
                    var stream = new MemoryStream(data);
                    var curImage = Image.FromStream(stream);
                    var dbImage = Image.FromStream(new MemoryStream(target.Content));

                    var difference = ImageTool.GetPercentageDifference(curImage, dbImage);
                    //var thresholdDifference = ImageTool.GetPercentageDifference(curImage, dbImage, 10);
                    if (difference < _options.DiffThreshold)
                        continue;
                    //using var comparer = new Comparer(stream);
                    //comparer.Compare(stream);
                    //var res = comparer.GetResultString();
                    //var changes = comparer.GetChanges();
                    //var a = new Image(stream);
                    var channel = _client.GetChannel(target.Channel) as Discord.IMessageChannel;
                    await channel.SendFileAsync(new MemoryStream(data), $"screenshot.png");
                    await channel.SendMessageAsync(target.Url);
                    await channel.SendMessageAsync(difference.ToString());
                    //target.Content = content;
                    target.Content = data;
                    await _entityContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}