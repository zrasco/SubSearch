using Addic7ed.Addic7edApi.Models;
using Microsoft.Extensions.Logging;
using ProviderPluginTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace ProviderAddic7ed
{
    public class ProviderPlugin : IProviderPlugin
    {
        const string SHOWS_CACHE_FILENAME = "shows.tmp";
        // Global variables
        private Addic7ed.Addic7edApi.Api _api;
        private List<TvShow> _tvShows;
        private ILogger<IProviderPlugin> _logger;

        public ProviderPlugin(ILogger<ProviderPlugin> logger)
        {
            // Future versions may have additional dependencies injected
            _logger = logger;
        }
        public void Init()
        {
            Stream stream = null;

            try
            {
                // Create an API instance
                _api = new Addic7ed.Addic7edApi.Api();

                IFormatter formatter = new BinaryFormatter();

                if (File.Exists(SHOWS_CACHE_FILENAME))
                {
                    stream = new FileStream(SHOWS_CACHE_FILENAME, FileMode.Open, FileAccess.Read);

                    _tvShows = (List<TvShow>)formatter.Deserialize(stream);

                    stream.Close();

                    _logger.LogDebug($"SearchAddic7ed - Show list read from {SHOWS_CACHE_FILENAME}");

                }
                else
                {
                    // Get the list of all TV shows
                    _tvShows = _api.GetShows().Result;

                    stream = new FileStream(SHOWS_CACHE_FILENAME, FileMode.Create, FileAccess.Write);
                    formatter.Serialize(stream, _tvShows);
                    stream.Close();

                    _logger.LogDebug($"SearchAddic7ed - Show list downloaded and written to {SHOWS_CACHE_FILENAME}");
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Exception in SearchAddic7ed - Init()");
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        public void Unload()
        {
            // No cleanup needed
        }

        public SearchCapabilities ProviderCapabilities()
        {
            return SearchCapabilities.TV;
        }

        public Task<IList<DownloadedSubtitle>> SearchSubtitlesByHashAsync(string fileHash, long fileSize, IList<CultureInfo> cultureInfos)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<DownloadedSubtitle>> SearchSubtitlesForTVAsync(string showName, int seasonNbr, int episodeNbr, IList<CultureInfo> cultureInfos)
        {
            List<DownloadedSubtitle> downloadedSubs = new List<DownloadedSubtitle>();

            try
            {
                if (!_tvShows.Any())
                {
                    _logger.LogInformation("SearchAddic7ed(): No TV shows available.");
                }
                else
                {
                    // Find our target show
                    var myShow = _tvShows.FirstOrDefault(x => x.Name.Contains(showName));

                    if (myShow == null)
                    {
                        _logger.LogInformation($"SearchAddic7ed(): TV show specified ({showName}) was not in the list of available shows.");
                    }
                    else
                    {
                        // Find all subtitles for each episode in the target season
                        var eps = await _api.GetSeasonSubtitles(myShow.Id, seasonNbr);

                        if (!eps.Any())
                        {
                            _logger.LogInformation($"SearchAddic7ed(): No episodes for season ({seasonNbr}) were available.");
                        }
                        else
                        {
                            // Find our target episode
                            var myEp = eps.Where(x => x.Number == episodeNbr).FirstOrDefault();

                            if (myEp == null)
                            {
                                _logger.LogInformation($"SearchAddic7ed(): No subtitles for season ({seasonNbr}) episode ({episodeNbr}) were available.");
                            }
                            else
                            {
                                foreach (CultureInfo language in cultureInfos)
                                {
                                    // Find our target subtitle. Grab the first english one by default
                                    var found = myEp.Subtitles.FirstOrDefault(x => x.Language == language.DisplayName);

                                    if (found == null)
                                    {
                                        _logger.LogInformation($"SearchAddic7ed(): Subtitles for season ({seasonNbr}) episode ({episodeNbr}) were available, not in the language specified ({language})");
                                    }
                                    else
                                    {
                                        var downloadedSub = await _api.DownloadSubtitle(myShow.Id, found.DownloadUri);

                                        downloadedSubs.Add(new DownloadedSubtitle() { Contents = downloadedSub.Stream, CultureInfo = language });

                                        _logger.LogInformation($"SearchAddic7ed(): Successfully retrieved subtitles for season ({seasonNbr}) episode ({episodeNbr}) in {language}");
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in SearchAddic7ed(): SearchSubtitlesForTVAsync()");
                throw;
            }

            return downloadedSubs;
        }

        public string Version()
        {
            return "0.5";
        }
    }
}
