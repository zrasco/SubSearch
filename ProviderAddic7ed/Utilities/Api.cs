using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Addic7ed.Addic7edApi.Models;

namespace Addic7ed.Addic7edApi
{
    public class Api
    {
        private readonly HttpClient _httpClient;

        public Api()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<TvShow>> GetShows()
        {
            var html = await _httpClient.GetShows();
            return await new Parser().GetShows(html);
        }

        public async Task<int> GetNumberOfSeasons(int addictedShowId)
        {
            var html = await _httpClient.GetNumberOfSeasons(addictedShowId);
            return await new Parser().GetNumberOfSeasons(html);
        }

        public async Task<DownloadSubtitleResult> DownloadSubtitle(int addictedShowId, Uri downloadUri)
        {
            var httpContent = await _httpClient.DownloadSubtitle(addictedShowId, downloadUri);

            // Bugfixes by Zeb Rasco
            //
            // 1) Sometimes the Content type is null because the charset is blank as well, so we'll default to "text/srt" in case that happens
            //
            // 2) Sometimes ContentDisposition is blank, most likely because you've exceeded your daily download limit.
            const string DEFAULT_MEDIA_TYPE = "text/srt";

            var downloadSubtitleResult = new DownloadSubtitleResult
            {
                Stream = await httpContent.ReadAsStreamAsync(),
            };

            if (httpContent.Headers.ContentType == null)
                downloadSubtitleResult.Mediatype = DEFAULT_MEDIA_TYPE;
            else
                downloadSubtitleResult.Mediatype = httpContent.Headers.ContentType.MediaType;

            if (httpContent.Headers.ContentDisposition == null)
            {
                var content = await httpContent.ReadAsStringAsync();

                // TODO: Make this more informative
                string exMessage = "Unknown error";

                if (content.Contains("Daily Download count exceeded"))
                    exMessage = "Daily download count exceeded";

                throw new Exception(exMessage);
            }
            else
                downloadSubtitleResult.Filename = httpContent.Headers.ContentDisposition.FileName.Trim('"');

            return downloadSubtitleResult;
        }

        public async Task<Episode> GetEpisodeSubtitles(int addictedShowId, int seasonNumber, int episodeNumber)
        {
            var episodes = await GetSeasonSubtitles(addictedShowId, seasonNumber);
            return episodes.FirstOrDefault(e => e.Number == episodeNumber);
        }

        public async Task<List<Episode>> GetSeasonSubtitles(int showId, int seasonNumber)
        {
            var html = await _httpClient.GetSeasonSubtitles(showId, seasonNumber);
            return await new Parser().GetSeasonSubtitles(html);
        }
    }
}