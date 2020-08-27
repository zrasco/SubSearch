using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Addic7ed.Addic7edApi
{
    internal class HttpClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClient()
        {
            var baseAddress = new Uri("http://www.addic7ed.com/", UriKind.Absolute);
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };

            cookieContainer.Add(baseAddress, new Cookie("wikisubtitlesuser", "954584"));
            cookieContainer.Add(baseAddress, new Cookie("wikisubtitlespass", "4175b653dd76b03dd97fc3670ff4f048"));

            _httpClient = new System.Net.Http.HttpClient(handler)
            {
                BaseAddress = baseAddress
            };


        }

        public async Task<string> Login(string username, string password)
        {
            var html = await _httpClient.GetStringAsync(new Uri($"login.php"));

            return html;
        }

        public async Task<string> GetShows()
        {
            var html = await _httpClient.GetStringAsync("http://web.archive.org/web/20190810000426/https://www.addic7ed.com/ajax_getShows.php");
            //var html = await _httpClient.GetStringAsync(new Uri("/ajax_getShows.php", UriKind.Relative));
            return html;
        }

        public async Task<string> GetNumberOfSeasons(int addictedShowId)
        {
            return
                await
                    _httpClient.GetStringAsync(new Uri($"/ajax_getSeasons.php?showID={addictedShowId}",
                        UriKind.Relative));
        }

        public async Task<HttpContent> DownloadSubtitle(int addictedShowId, Uri downloadUri)
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Referrer = new Uri(_httpClient.BaseAddress, $"/show/{addictedShowId}");
            requestMessage.Method = HttpMethod.Get;
            requestMessage.RequestUri = downloadUri;
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            return responseMessage.Content;
        }

        public async Task<string> GetSeasonSubtitles(int showId, int seasonNumber)
        {
            var uri = new Uri($"/ajax_loadShow.php?show={showId}&season={seasonNumber}", UriKind.Relative);

            var html = await _httpClient.GetStringAsync(uri);
            return html;
        }
    }
}