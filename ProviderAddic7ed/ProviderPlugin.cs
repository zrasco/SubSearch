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
        const string PLUGIN_VERSION = "0.5";

        // Global variables
        private Addic7ed.Addic7edApi.Api _api;
        private List<TvShow> _tvShows;
        private ILogger<IProviderPlugin> _logger;

        public ProviderPlugin(ILogger<ProviderPlugin> logger)
        {
            // Future versions may have additional dependencies injected
            _logger = logger;

            _logger.LogTrace($"{GetCaller()}() entered");

            _logger.LogTrace($"{GetCaller()}() exiting");
        }
        public void Init()
        {
            _logger.LogTrace($"{GetCaller()}() entered");
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

                    _logger.LogDebug($"Show list read from {SHOWS_CACHE_FILENAME}");

                }
                else
                {
                    // Get the list of all TV shows
                    _tvShows = _api.GetShows().Result;

                    if (_tvShows.Any())
                    {
                        stream = new FileStream(SHOWS_CACHE_FILENAME, FileMode.Create, FileAccess.Write);
                        formatter.Serialize(stream, _tvShows);
                        stream.Close();

                        _logger.LogDebug($"Show list downloaded and written to {SHOWS_CACHE_FILENAME}");
                    }
                    else
                        _logger.LogWarning($"No TV shows on site!");



                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Exception: {GetCaller()}()");
                _logger.LogTrace($"{GetCaller()}() exiting (exception)");
                throw;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            _logger.LogTrace($"{GetCaller()}() exiting (normal)");
        }

        public void Unload()
        {
            _logger.LogTrace($"{GetCaller()}() entered");

            // No cleanup needed

            _logger.LogTrace($"{GetCaller()}() exiting");
        }

        public SearchCapabilities ProviderCapabilities()
        {
            return SearchCapabilities.TV;
        }

        public Task<IList<DownloadedSubtitle>> SearchSubtitlesByHashAsync(string fileHash, long fileSize, IList<CultureInfo> cultureInfos)
        {
            _logger.LogTrace($"{GetCaller()}() entered");

            _logger.LogTrace($"{GetCaller()}() exiting (exception)");
            throw new NotImplementedException();
        }

        public async Task<IList<DownloadedSubtitle>> SearchSubtitlesForTVAsync(IList<string> showNameCandidates, int seasonNbr, int episodeNbr, IList<CultureInfo> cultureInfos)
        {
            _logger.LogTrace($"{GetCaller()}() entered");

            List<DownloadedSubtitle> downloadedSubs = new List<DownloadedSubtitle>();

            try
            {
                if (!_tvShows.Any())
                {
                    _logger.LogInformation("No TV shows available.");
                }
                else
                {
                    // Find our target show
                    bool foundShow = false;

                    _logger.LogDebug($"Searching for subtitles for the following show names: ({String.Join<string>(", ", showNameCandidates).TrimEnd()})");

                    for (int x = 0; x < showNameCandidates.Count && !foundShow; x++)
                    {
                        string showName = showNameCandidates[x];

                        var myShow = _tvShows.FirstOrDefault(x => x.Name.Contains(showName, StringComparison.OrdinalIgnoreCase));

                        if (myShow == null)
                        {
                            _logger.LogDebug($"TV show variation ({x + 1}/{showNameCandidates.Count}) ({showName}) was not in the list of available shows.");
                        }
                        else
                        {
                            foundShow = true;

                            _logger.LogDebug($"TV show variation ({x + 1}/{showNameCandidates.Count}) ({showName}) was found in the list of available shows.");

                            // Find all subtitles for each episode in the target season
                            var eps = await _api.GetSeasonSubtitles(myShow.Id, seasonNbr);

                            if (!eps.Any())
                            {
                                _logger.LogInformation($"No episodes for season ({seasonNbr}) were available.");
                            }
                            else
                            {
                                // Find our target episode
                                var myEp = eps.Where(x => x.Number == episodeNbr).FirstOrDefault();

                                if (myEp == null)
                                {
                                    _logger.LogInformation($"No subtitles for series ({showName}) season ({seasonNbr}) episode ({episodeNbr}) were available.");
                                }
                                else
                                {
                                    foreach (CultureInfo language in cultureInfos)
                                    {
                                        // Find our target subtitle. Grab the first english one by default
                                        var found = myEp.Subtitles.FirstOrDefault(x => x.Language == language.DisplayName);

                                        // Try again using parent language
                                        if (found == null)
                                            found = myEp.Subtitles.FirstOrDefault(x => x.Language == language.Parent.DisplayName);

                                        if (found == null)
                                        {
                                            _logger.LogInformation($"Subtitles for series ({showName}) season ({seasonNbr}) episode ({episodeNbr}) were available, not in the language specified ({language})");
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"Downloading subtitles for series ({showName}) season ({seasonNbr}) episode ({episodeNbr}) in {language}");

                                            var downloadedSub = await _api.DownloadSubtitle(myShow.Id, found.DownloadUri);

                                            downloadedSubs.Add(new DownloadedSubtitle() { Contents = downloadedSub.Stream, CultureInfo = language });

                                            _logger.LogInformation($"Successfully retrieved subtitles for series ({showName}) season ({seasonNbr}) episode ({episodeNbr}) in {language}");
                                        }
                                    }

                                }
                            }
                        }
                    }

                    if (!foundShow)
                    {
                        string warning = $"Could not find show in list of available shows. Used the following show names: ({String.Join<string>(", ", showNameCandidates).TrimEnd()})";

                        _logger.LogWarning(warning);
                    }
                        
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception: {GetCaller()}()");

                _logger.LogTrace($"{GetCaller()}() exiting (exception)");
                throw;
            }

            _logger.LogTrace($"{GetCaller()}() exiting (normal)");
            return downloadedSubs;
        }

        public string Version()
        {
            return PLUGIN_VERSION;
        }

        private static string GetCaller([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName;
        }
    }
}
