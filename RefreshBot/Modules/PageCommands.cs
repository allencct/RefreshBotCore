using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using RefreshBot.DataAccess;
using RefreshBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Options;
using RefreshBot.Options;

namespace RefreshBotCore.Modules
{
    public class PageCommands : ModuleBase
    {
        private readonly PageOptions _options;
        private readonly EntityContext _entityContext;


        //public ExampleCommands(EntityContext entityContext)
        //{
        //    _entityContext = entityContext;
        //}
        //private readonly EntityContext _entityContext;

        public PageCommands(IOptions<PageOptions> options, EntityContext entityContext)
        {

            _options = options.Value;
            _entityContext = entityContext;
        }

        [Command("add")]
        public async Task RefreshCommand([Remainder][Summary("The url of target to add")] string url)
        {
            url = url.Trim();
            if (string.IsNullOrEmpty(url))
            {
                await ReplyAsync("URL cannot be empty.");
                return;
            }

            var channelId = Context.Channel.Id;
            var matching = await _entityContext.TargetPages.SingleOrDefaultAsync(t => t.IsActive && t.Url == url && t.Channel == channelId);
            if (matching != null)
            {
                await ReplyAsync("Matching URL already found.");
                return;
            }

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var options = new LaunchOptions()
            {
                Args = new[] { "--no-sandbox" },
                Headless = true,
                //ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
            };
            var browser = await Puppeteer.LaunchAsync(options);
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = _options.Height, Height = _options.Width });
            //await page.setViewport({ width: 1440, height: 2880});

            //await page.GoToAsync("https://old.reddit.com/r/buildapc/search/?q=ssd&sort=new&restrict_sr=on&t=all");
            await page.GoToAsync(url);
            var data = await page.ScreenshotDataAsync();

            var targetPage = new TargetPage { IsActive = true, Url = url, Channel = Context.Channel.Id, Content = data };
            await _entityContext.TargetPages.AddAsync(targetPage);
            await _entityContext.SaveChangesAsync();

            await Context.Channel.SendFileAsync(new MemoryStream(data), "screenshot.png");
        }

        [Command("list")]
        public async Task ListCommand()
        {
            var channelId = Context.Channel.Id;
            var matching = await _entityContext.TargetPages.ToAsyncEnumerable().Where(t => t.Channel == channelId && t.IsActive == true).ToListAsync();
            if (!matching.Any())
            {
                await ReplyAsync("No urls found.");
                return;
            }

            var idLength = 3;
            var maxUrlLength = matching.Max(m => m.Url.Length);
            var border = $"+---+{"".PadRight(maxUrlLength, '-')}+\n";
            var sb = new StringBuilder();
            sb.Append("`");
            sb.Append(border);
            foreach(var match in matching)
            {
                sb.Append($"|{match.Id.ToString().PadRight(idLength)}|{match.Url.PadRight(maxUrlLength)}|\n");
                sb.Append(border);
            }
            sb.Append("`");
            await ReplyAsync(sb.ToString());
        }

        [Command("remove")]
        public async Task RemoveCommand([Summary("The ID of target to remove")] string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
            {
                await ReplyAsync("ID is required.");
                return;
            }
            if (!int.TryParse(idStr, out var id))
            {
                await ReplyAsync("ID is not valid.");
                return;
            }

            var target = await _entityContext.TargetPages.FindAsync(id);
            if(target == null)
                await ReplyAsync("Matching target not found.");

            target.IsActive = false;
            await _entityContext.SaveChangesAsync();

            await ReplyAsync($"{target.Url} has been removed.");
        }

        [Command("show")]
        public async Task ShowCommand()
        {
            var targets = await _entityContext.TargetPages.ToListAsync();
            await ReplyAsync(targets.FirstOrDefault().Url);

            await ReplyAsync("end");

            // send simple string reply
        }

        [Command("hello")]
        public async Task HelloCommand()
        {
            // initialize empty string builder for reply
            var sb = new StringBuilder();

            // get user info from the Context
            var user = Context.User;

            // build out the reply
            sb.AppendLine($"You are -> [{user.Username}]");
            sb.AppendLine("I must now say, World!");

            // send simple string reply
            await ReplyAsync(sb.ToString());
        }

        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
        => ReplyAsync(echo);

        [Command("8ball")]
        [Alias("ask")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task AskEightBall([Remainder] string args = null)
        {
            // I like using StringBuilder to build out the reply
            var sb = new StringBuilder();
            // let's use an embed for this one!
            var embed = new EmbedBuilder();

            // now to create a list of possible replies
            var replies = new List<string>();

            // add our possible replies
            replies.Add("yes");
            replies.Add("no");
            replies.Add("maybe");
            replies.Add("hazzzzy....");

            // time to add some options to the embed (like color and title)
            embed.WithColor(new Color(0, 255, 0));
            embed.Title = "Welcome to the 8-ball!";

            // we can get lots of information from the Context that is passed into the commands
            // here I'm setting up the preface with the user's name and a comma
            sb.AppendLine($"{Context.User.Username},");
            sb.AppendLine();

            // let's make sure the supplied question isn't null 
            if (args == null)
            {
                // if no question is asked (args are null), reply with the below text
                sb.AppendLine("Sorry, can't answer a question you didn't ask!");
            }
            else
            {
                // if we have a question, let's give an answer!
                // get a random number to index our list with (arrays start at zero so we subtract 1 from the count)
                var answer = replies[new Random().Next(replies.Count - 1)];

                // build out our reply with the handy StringBuilder
                sb.AppendLine($"You asked: [**{args}**]...");
                sb.AppendLine();
                sb.AppendLine($"...your answer is [**{answer}**]");

                // bonus - let's switch out the reply and change the color based on it
                switch (answer)
                {
                    case "yes":
                        {
                            embed.WithColor(new Color(0, 255, 0));
                            break;
                        }
                    case "no":
                        {
                            embed.WithColor(new Color(255, 0, 0));
                            break;
                        }
                    case "maybe":
                        {
                            embed.WithColor(new Color(255, 255, 0));
                            break;
                        }
                    case "hazzzzy....":
                        {
                            embed.WithColor(new Color(255, 0, 255));
                            break;
                        }
                }
            }

            // now we can assign the description of the embed to the contents of the StringBuilder we created
            embed.Description = sb.ToString();

            // this will reply with the embed
            await ReplyAsync(null, false, embed.Build());
        }
    }
}