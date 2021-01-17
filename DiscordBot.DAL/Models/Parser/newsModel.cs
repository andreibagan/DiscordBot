using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.DAL.Models.Parser
{
    public class NewsModel
    {
        public string HeaderUrl { get; set; }
        public string HeaderText { get; set; }
        public string Content { get; set; }
        public List<string> ContentUrl { get; set; }

        public NewsModel(string headerUrl, string headerText, string content, List<string> contentUrl)
        {
            HeaderUrl = headerUrl;
            HeaderText = headerText;
            Content = content;
            ContentUrl = contentUrl;
        }

        public NewsModel()
        {
            ContentUrl = new List<string>();
        }

    }
}
