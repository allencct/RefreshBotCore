using System;

namespace RefreshBot.Models
{
    public class TargetPage
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Url { get; set; }
        public ulong Channel { get; set; }
        public byte[] Content { get; set; }
    }
}
