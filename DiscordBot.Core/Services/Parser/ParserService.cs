using DiscordBot.DAL.Models.Parser;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Parser
{
    public interface IParserService
    {
        Task<List<NewsModel>> GetNews(string url);
    }

    public class ParserService : IParserService
    {
        public ParserService()
        {

        }

        public async Task<List<NewsModel>> GetNews(string url)
        {
            List<NewsModel> newsModel = new List<NewsModel>();

            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    using (HttpClient httpClient = new HttpClient(httpClientHandler))
                    {
                        using (HttpResponseMessage resp = await httpClient.GetAsync(url))
                        {
                            if (resp.IsSuccessStatusCode)
                            {
                                var html = await resp.Content.ReadAsStringAsync();

                                File.WriteAllText("vk.html", html);

                                if (!string.IsNullOrEmpty(html))
                                {
                                    HtmlDocument htmlDocument = new HtmlDocument();
                                    htmlDocument.LoadHtml(html);

                                    var content = htmlDocument.DocumentNode.SelectNodes(".//div[@class='wall_posts upanel bl_cont  mark_top ']//div[@class='wall_item']");

                                    if (content != null && content.Count > 0)
                                    {
                                        foreach (var item in content)
                                        {
                                            var headerContent = item.SelectSingleNode(".//div[@class='wi_head']//div[@class='wi_cont']//div[@class='wi_author']//a[@class='pi_author']");
                                            var urlContent = item.SelectSingleNode(".//div[@class='wi_head']//a[@data-post-click-type='post_owner_img']//img[@class='wi_img']");
                                            var textContent = item.SelectSingleNode(".//div[@class='wi_body']//div[@class='pi_text']");
                                            var imageContent = item.SelectNodes(".//div[@class]" +
                                                "//div[@class]" +
                                                "//div[@class='medias_thumbs medias_thumbs_map']" +
                                                "//div[@class='thumbs_map_wrap']" +
                                                "//div[@class='thumbs_map_helper']" +
                                                "//div[@class='thumbs_map fill']" +
                                                "//a[@class]" +
                                                "//div[@class]");

                                            NewsModel news = new NewsModel();

                                            if (headerContent != null)
                                            {
                                                news.HeaderText = headerContent.InnerText.Length < 256 ? headerContent.InnerText : headerContent.InnerText.Substring(0, 256);
                                            }

                                            if (urlContent != null)
                                            {
                                                news.HeaderUrl = (urlContent.Attributes["src"].Value);
                                            }

                                            if (textContent != null)
                                            {
                                                news.Content = textContent.InnerText.Length < 2048 ? textContent.InnerText : textContent.InnerText.Substring(0, 2048);
                                            }

                                            if (imageContent != null && imageContent.Count > 0)
                                            {
                                                foreach (var imageItem in imageContent)
                                                {
                                                    news.ContentUrl.Add(imageItem.Attributes["style"].Value.Split(new[] { "url(", ");" }, StringSplitOptions.None)[1].Replace("amp;",""));
                                                }
                                            }

                                            newsModel.Add(news);
                                        }

                                        return newsModel;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return new List<NewsModel>();
        }
    }
}
