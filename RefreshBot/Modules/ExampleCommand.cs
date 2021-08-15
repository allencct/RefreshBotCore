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

namespace RefreshBotCore.Modules
{
    public class ExampleCommands : ModuleBase
    {
        private readonly EntityContext _entityContext;

        //public ExampleCommands(EntityContext entityContext)
        //{
        //    _entityContext = entityContext;
        //}
        //private readonly EntityContext _entityContext;

        public ExampleCommands(IServiceProvider serviceProvider)
        {
            _entityContext = serviceProvider.GetRequiredService<EntityContext>();
        }

        [Command("refresh")]
        public async Task RefreshCommand()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var options = new LaunchOptions()
            {
                Args = new[] { "--no-sandbox" },
                Headless = true,
                //ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
            };
            var browser = await Puppeteer.LaunchAsync(options);
            var page = await browser.NewPageAsync();
            var url = "https://old.reddit.com/r/buildapcsales/new/";
            //await page.GoToAsync("https://old.reddit.com/r/buildapc/search/?q=ssd&sort=new&restrict_sr=on&t=all");
            await page.GoToAsync(url);
            var stream = await page.ScreenshotStreamAsync();
            var content = await page.GetContentAsync();
            //Console.WriteLine(content);
            //page.
            //var image = new Image(stream);
            //page.ScreenshotAsync
            //new EmbedImage(image)
            //var embed = new EmbedBuilder()
            //{ Fields = }
            //await page.ScreenshotAsync(outputFile);
            //await ReplyAsync(embed: new EmbedImage();
            //Context.Channel.SendMessageAsync
            var targetPage = new TargetPage { Active = true, Url = url };
            try
            {
                await _entityContext.TargetPages.AddAsync(targetPage);
                await _entityContext.SaveChangesAsync();
            }
            catch (Exception e)
            {

            }


            await Context.Channel.SendFileAsync(stream, "screenshot.png");
        }

        [Command("show")]
        public async Task ShowCommand()
        {
            try
            {
                var targets = await _entityContext.TargetPages.ToListAsync();
                await ReplyAsync(targets.FirstOrDefault().Url);

            }
            catch (Exception e)
            {

            }
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