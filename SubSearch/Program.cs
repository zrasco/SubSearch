using Serilog;
using Serilog.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SubSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerSettings settings;

            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.RollingFile("SubSearchLog.txt", retainedFileCountLimit: 30)
                            .WriteTo.Console()
                            .CreateLogger();
            try
            {
                if (args.Count() < 1)
                    Usage(args);
                else
                {
                    string successfulProvider = null;
                    string videoFile = args[0];
                    string outputFile = Path.ChangeExtension(videoFile, ".srt");

                    if (File.Exists(outputFile))
                    {
                        Console.WriteLine($"Skipping file {videoFile}, subtitle already exists...");
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

                        if (SearchAddic7ed(show, season, episodeNbr, outputFile).Result)
                        {
                            successfulProvider = "Addic7ed";
                        }

                        if (!string.IsNullOrEmpty(successfulProvider))
                            Console.WriteLine($"Succesfully retrieved subtitle file {outputFile} from {successfulProvider}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Main(): {ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }

        static void Usage(string[] args)
        {
            Console.WriteLine($"Usage: {Environment.CommandLine} [TV episode]");
        }

        async static Task<bool> SearchAddic7ed(string name, int season, int episode, string outputFile, string language = "English")
        {
            bool retval = false;

            try
            {
                var api = new Addic7ed.Addic7edApi.Api();
                var tvShows = await api.GetShows();

                var myShow = tvShows.FirstOrDefault(x => x.Name.Contains(name));

                var eps = await api.GetSeasonSubtitles(myShow.Id, season);
                var myEp = eps.Where(x => x.Number == episode).FirstOrDefault();

                var found = myEp.Subtitles.FirstOrDefault(x => x.Language == language);

                if (found != null)
                {
                    var downloadedSub = await api.DownloadSubtitle(myShow.Id, found.DownloadUri);

                    SaveFileStream(outputFile, downloadedSub.Stream);

                    retval = true;
                }

                retval = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SearchAddic7ed(): {ex.Message}");
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
                Console.WriteLine($"Exception in SaveFileStream(): {ex.Message}");
                throw;
            }
        }


    }
}
