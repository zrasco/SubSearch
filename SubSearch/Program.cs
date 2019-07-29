using OSDBnet;
using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SubSearch
{
    class Program
    {
        const string DEFAULT_LANGUAGE = "English (United States)";
        private static CultureInfo[] _cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
        static ILogger _logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.RollingFile("SubSearchLog.txt", retainedFileCountLimit: 30)
                            .WriteTo.Console()
                            .CreateLogger();

        static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 1)
                    Usage(args);
                else
                {
                    string successfulProvider = null;
                    string videoFile = args[0];
                    string defaultOutputFile = Path.GetDirectoryName(videoFile) + Path.GetFileNameWithoutExtension(videoFile) + ".srt";
                    string langOutputFile = Path.GetDirectoryName(videoFile) + Path.GetFileNameWithoutExtension(videoFile) + "." + _cultureInfos.Where(x => x.DisplayName == DEFAULT_LANGUAGE).FirstOrDefault().TwoLetterISOLanguageName + ".srt";
                    string langCountryOutputFile = Path.GetDirectoryName(videoFile) + Path.GetFileNameWithoutExtension(videoFile) + "." + _cultureInfos.Where(x => x.DisplayName == DEFAULT_LANGUAGE).FirstOrDefault().Name + ".srt";


                    // Attempt to find subtitles with correct country info

                    if (File.Exists(defaultOutputFile) || File.Exists(langOutputFile) || File.Exists(langCountryOutputFile) )
                    {
                        _logger.Information($"Skipping file {videoFile}, subtitle already exists...");
                    }
                    else
                    {
                        // Get the show, season and episode number
                        // Episode must be in this format:
                        // Aqua Teen Hunger Force - S01E01 - Rabbot [LOW] [2000-12-30]
                        var pieces = Path.GetFileName(videoFile).Split("-");

                        string show = pieces[0].Trim();
                        string [] seasonEpStr = pieces[1].Trim().ToUpper().Split("E");

                        int season = Int32.Parse(seasonEpStr[0].Substring(1, seasonEpStr[0].Length - 1));
                        int episodeNbr = Int32.Parse(seasonEpStr[1]);

                        if (SearchAddic7ed(show, season, episodeNbr, langCountryOutputFile).Result)
                        {
                            successfulProvider = "Addic7ed";
                        }

                        if (!string.IsNullOrEmpty(successfulProvider))
                            _logger.Information($"Succesfully retrieved subtitle file {langCountryOutputFile} from {successfulProvider}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception in Main(): {ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }

        static void Usage(string[] args)
        {
            Console.WriteLine($"Usage: {Environment.CommandLine} [TV episode]");
        }

        async static Task<bool> SearchOpenSubtitles(string fileHash, long fileSize, string languages = DEFAULT_LANGUAGE)
        {
            bool retval = false;

            var client = Osdb.Create("SubSearch");
            List<Subtitle> targetSubList = (await client.SearchSubtitlesFromHash(languages, fileHash, fileSize)).ToList();

            return retval;
        }

        async static Task<bool> SearchAddic7ed(string name, int season, int episode, string outputFile, string language = DEFAULT_LANGUAGE)
        {
            bool retval = false;

            try
            {
                var api = new Addic7ed.Addic7edApi.Api();
                // Get the list of all TV shows
                var tvShows = await api.GetShows();

                if (!tvShows.Any())
                {
                    _logger.Information("SearchAddic7ed(): No TV shows available.");
                }
                else
                {
                    // Find our target show
                    var myShow = tvShows.FirstOrDefault(x => x.Name.Contains(name));

                    if (myShow == null)
                    {
                        _logger.Information($"SearchAddic7ed(): TV show specified ({name}) was not in the list of available shows.");
                    }
                    else
                    {
                        // Find all subtitles for each episode in the target season
                        var eps = await api.GetSeasonSubtitles(myShow.Id, season);

                        if (!eps.Any())
                        {
                            _logger.Information($"SearchAddic7ed(): No episodes for season ({season}) were available.");
                        }
                        else
                        {
                            // Find our target episode
                            var myEp = eps.Where(x => x.Number == episode).FirstOrDefault();

                            if (myEp == null)
                            {
                                _logger.Information($"SearchAddic7ed(): No subtitles for season ({season}) episode ({episode}) were available.");
                            }
                            else
                            {
                                // Find our target subtitle. Grab the first english one by default
                                var found = myEp.Subtitles.FirstOrDefault(x => x.Language == language);

                                if (found == null)
                                {
                                    _logger.Information($"SearchAddic7ed(): Subtitles for season ({season}) episode ({episode}) were available, not in the language specified ({language})");
                                }
                                else
                                {
                                    var downloadedSub = await api.DownloadSubtitle(myShow.Id, found.DownloadUri);

                                    SaveFileStream(outputFile, downloadedSub.Stream);

                                    _logger.Information($"SearchAddic7ed(): Successfully retrieved subtitles for season ({season}) episode ({episode}) in {language}");
                                    retval = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception in SearchAddic7ed(): {ex.Message}");
                throw;
            }

            return retval;
        }



        private static void SaveFileStream(String path, Stream stream)
        {
            try
            {
                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                stream.CopyTo(fileStream);
                fileStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception in SaveFileStream(): {ex.Message}");
                throw;
            }
        }


    }
}
