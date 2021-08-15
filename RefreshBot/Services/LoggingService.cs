using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RefreshBot.Services
{

    public class ScheduleService
    {
        private readonly DiscordSocketClient _client;
        private readonly Timer _timer;

        private List<ScheduleData> _queue;
        private ConcurrentDictionary<SocketUser, UserStamp> _bouncer;

        private DateTime startTime;
        private object busy = new object();
        private StringBuilder SBuilder;
        private string streamChannelmask = "bot";
        private SocketTextChannel streamChannel;

        public int Count => _queue.Count;

        public ScheduleService(DiscordSocketClient client)
        {
            //Client stuff
            _client = client;
            _client.Ready += OnClientReady;
            // Initializing collections
            _queue = new List<ScheduleData>();
            _bouncer = new ConcurrentDictionary<SocketUser, UserStamp>();
            startTime = DateTime.Now;
            SBuilder = new StringBuilder(); // lol optimization
                                            // setting up Timer, currently hardcoded
            _timer = new Timer();
            _timer.Interval = 30000;
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimedEvent;


        }

        //private async Task FindChannel()
        //{
        //    foreach (var guild in _client.Guilds)
        //    {
        //        if (guild.Name.ContainsIC("iconoclasts"))
        //        {
        //            foreach (var channel in guild.TextChannels)
        //            {
        //                if (channel.Name.ContainsIC(streamChannelmask)) { streamChannel = channel; break; }
        //            }
        //        }
        //    }
        //    //Logger.LogConsoleInfo($"Default Scheduler channel: {streamChannelmask}");
        //}

        // Initializing some post-constructor stuff when client is ready and connected
        private async Task OnClientReady()
        {
            Start("OnReady event");
            //FindChannel();
        }

        // Timer Event fired every XXms that does most of thw work - going through list
        // finding stuff that needs to be announced, etc
        // using soft lock because timer resolution is big enough to allow any new requests fall through
        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (System.Threading.Monitor.TryEnter(busy, 1000))
            {
                try
                {
                    if (_queue.Count > 0)
                    {
                        if (_client.ConnectionState == ConnectionState.Connected)
                        {
                            if (streamChannel != null)
                            {
                                StringBuilder announce = new StringBuilder();
                                int count = -1;
                                for (int i = 0; i < _queue.Count; i++)
                                {
                                    if ((_queue[i].date - DateTime.Now).TotalSeconds < 5)
                                    {
                                        announce.Append($"Scheduled stream starting: {_queue[0].URL} (added by {_queue[0].user.Username})\n");
                                        count++;
                                    }
                                    else { break; }
                                }
                                if (count > -1)
                                {
                                    await streamChannel.SendMessageAsync(announce.ToString());
                                    _queue.RemoveRange(0, count + 1);
                                    //Logger.LogConsoleInfo($"Scheduler pass finished, removed entries {count + 1}");
                                }
                            }
                        }
                        else
                        {
                            int count = -1;
                            for (int i = 0; i < _queue.Count; i++)
                            {
                                if ((_queue[i].date - DateTime.Now).TotalSeconds < 5) { count++; }
                                else { break; }
                            }
                            if (count > -1)
                            {
                                _queue.RemoveRange(0, count + 1);
                                //Logger.LogConsoleInfo($"Timer check passed but not connected. Removed expired entries {count + 1}");
                            }

                        }
                    }
                    else { Stop("Nothing in queue"); }
                }
                finally { System.Threading.Monitor.Exit(busy); }
            }
            // 24hrs check, gonna need to figure out something else for this
            if ((DateTime.Now - startTime).TotalHours > 23)
            {
                startTime = DateTime.Now;
                ResetUsers();
            }
        }

        //Synchronous start of the timer
        public void Stop(string reason)
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                //Logger.LogConsoleInfo($"Scheduler has been stopped: {reason}");
            }
        }

        //Synchronous stop of the timer
        public void Start(string reason)
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
                //Logger.LogConsoleInfo($"Scheduler has been restarted: {reason}");
            }
        }

        //Adding new entry to the queue List
        // using explicit lock because every request needs to be processed
        public async Task AddEntry(string url, DateTime time, SocketUser user)
        {
            UpdateUser(user);
            lock (busy)
            {
                int count = -1;
                for (int i = 0; i < _queue.Count; i++)
                {
                    if (_queue[i].date < time) { count++; }
                    else { break; }
                }
                if (count > -1)
                {
                    _queue.Insert(count + 1, new ScheduleData(url, time, user));
                }
                else
                {
                    _queue.Add(new ScheduleData(url, time, user));
                }
                Start("New entry added to empty queue");
            }
        }

        //Removing entry from the queue List
        // using explicit lock because every request needs to be processed
        public async Task<string> RemoveEntry(string url, SocketUser user, bool owner)
        {
            lock (busy)
            {
                var data = new ScheduleData(url, DateTime.Now, null);
                int indx = -1;
                indx = _queue.IndexOf(data);
                if (indx > -1)
                {
                    var usr = _queue[indx].user.Username;
                    if (usr == user.Username || owner)
                    {
                        _queue.RemoveAt(indx);
                        return $"Removed entry {url} (by {usr})";
                    }
                    else
                    {
                        return $"Entry {url} exist, but it must be removed by the original user: {usr}";
                    }
                }
                else
                {
                    busy = false;
                    return $"Entry {url} not found in queue.";
                }
            }
        }

        // Checking if specific entry exist in the List, using first result
        // Comparer only cares about URL so dummy SheduleData made
        public async Task<ScheduleData> CheckEntry(string url)
        {
            // Need to fix the Socketuser null part since it is bad way to check if no results returned
            // SU can be null if user left server?
            lock (busy)
            {
                var data = new ScheduleData(url, DateTime.Now, null);
                int indx = -1;
                indx = _queue.IndexOf(data);
                if (indx > -1) { return _queue[indx]; }
                else { return data; }
            }

        }


        // using soft lock because this request can be ignored if collection is busy
        public async Task<string> GetList()
        {
            if (System.Threading.Monitor.TryEnter(busy, 1000))
            {
                try
                {
                    SBuilder.Clear();
                    foreach (var item in _queue)
                    {
                        SBuilder.Append($"[{item.URL}] [{item.date}] [{item.user}]\n");
                    }
                    return SBuilder.ToString();
                }
                finally { System.Threading.Monitor.Exit(busy); }
            }
            return "Queue is busy";
        }

        public async Task<string> Status()
        {
            return $"Entries: {Count} | Timer enabled: {_timer.Enabled} | Resolution {_timer.Interval}ms";
        }

        //
        // User-bouncer checks
        // Using ConcurrentDic for this so dont care for locks and stuff

        public async Task<bool> CanAdd(SocketUser user)
        {
            return !_bouncer.ContainsKey(user) || _bouncer[user].CanAdd();
        }

        public async Task UpdateUser(SocketUser user)
        {
            if (!_bouncer.ContainsKey(user))
            {
                var result = _bouncer.TryAdd(user, new UserStamp(DateTime.Now, 1));
                //if (!result) { Logger.LogConsoleInfo("Unable to add user to timestamp DB"); }
            }
            else
            {
                _bouncer[user].lastTimestamp = DateTime.Now;
                _bouncer[user].count = _bouncer[user].count + 1;
            }
        }

        public async Task ResetUsers() { _bouncer.Clear(); }


    }

    // Struct for List of queue
    public struct ScheduleData : IEquatable<ScheduleData>
    {
        public string URL;
        public DateTime date;
        public SocketUser user;

        public ScheduleData(string url, DateTime DT, SocketUser usr)
        {
            URL = url;
            date = DT;
            user = usr;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ScheduleData))
                return false;

            ScheduleData SD = (ScheduleData)obj;
            return this.Equals(SD);
        }

        public bool Equals(ScheduleData other)
        {
            return URL.Equals(other.URL);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * 23 + URL.GetHashCode();
            }
        }
    }

    public class UserStamp
    {
        public DateTime lastTimestamp;
        public int count;

        public UserStamp(DateTime timestamp, int Count)
        {
            lastTimestamp = timestamp;
            count = Count;
        }

        public bool CanAdd() { return count < 5 && (DateTime.Now - lastTimestamp).TotalSeconds > 600; }

    }
}
