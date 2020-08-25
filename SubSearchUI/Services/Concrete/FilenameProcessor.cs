using Microsoft.Extensions.Logging;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SubSearchUI.Services.Concrete
{
    class GroupRefEntry
    {
        public string Key { get; set; }
        public Action<TVShowValue, string> SetterAction { get; set; }
    }
    public class FilenameProcessor : IFilenameProcessor
    {
        // Capture group names
        const string SERIES_STR = "series";
        const string SEASON_STR = "season";
        const string EPISODE_STR = "episode";
        const string TITLE_STR = "title";
        const string QUALITY_STR = "quality";
        const string DATE_STR = "date";

        GroupRefEntry[] _groupRefEntries = new GroupRefEntry[] {
            // Series name, season and episode number are mandatory (usually) 
            new GroupRefEntry() { Key = SERIES_STR, SetterAction = (o, input) => o.Series = input },
            new GroupRefEntry() { Key = SEASON_STR, SetterAction = (o, input) => o.Season = Int32.Parse(input) },
            new GroupRefEntry() { Key = EPISODE_STR, SetterAction = (o, input) => o.EpisodeNbr = Int32.Parse(input) },

            // Optional
            new GroupRefEntry() { Key = TITLE_STR, SetterAction = (o, input) => o.Title = input },
            new GroupRefEntry() { Key = QUALITY_STR, SetterAction = (o, input) => o.Quality = input },
            new GroupRefEntry() { Key = DATE_STR, SetterAction = (o, input) => {
                if (DateTime.TryParse(input, out DateTime result))
                    o.Date = result;
            } },
        };

        private readonly AppSettings _appSettings;
        private ILogger<FilenameProcessor> _logger;

        public FilenameProcessor(IWritableOptions<AppSettings> appSettings,
                                    ILogger<FilenameProcessor> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }
        public TVShowValue GetTVShowInfo(SubtitleFileInfo subtitleFileInfo, string RegExExpression = null)
        {
            _logger.LogTrace("Entering GetTVShowInfo(SubtitleFileInfo, RegExExpression)");

            TVShowValue retval = new TVShowValue();
            List<string> expressionList;

            if (RegExExpression != null)
                expressionList = new List<string>() { RegExExpression };
            else
                expressionList = _appSettings.RegExTVInferred;

            // Try to infer the series name and season # from the folder structure, using the following:
            // Series Name\Season <season nbr>\Episode filename.extension
            //
            // Hence (assuming folderNamesArray.Length >= 3):
            // Episode filename.extension == folderNamesArray[folderNamesArray.Length - 1]
            // Season <season nbr> == folderNamesArray[folderNamesArray.Length - 2]
            // Series Name == folderNamesArray[folderNamesArray.Length - 3]

            var folderNamesArray = subtitleFileInfo.FullPath.Split(@"\");

            if (folderNamesArray.Length >= 3)
            {
                string episode = folderNamesArray[folderNamesArray.Length - 1];
                bool gotSeason = Int32.TryParse(folderNamesArray[folderNamesArray.Length - 2].Replace("Season", "").Trim(), out int season);

                if (gotSeason)
                {
                    bool exitLoop = false;
                    string series = folderNamesArray[folderNamesArray.Length - 3];

                    _logger.LogDebug($"Series=={series}, Season=={season}, Episode=={episode}");

                    for (int x = 0; x < expressionList.Count && !exitLoop; x++)
                    {
                        string expression = expressionList[x];
                        Regex r = new Regex(expression);
                        Match m = r.Match(subtitleFileInfo.Filebase);

                        // Only need to get the episode number
                        if (m.Groups.Count >= 1)
                        {
                            if (m.Groups.ContainsKey(EPISODE_STR))
                            {
                                retval.Series = series;
                                retval.Season = season;

                                _groupRefEntries.Where(x => x.Key == EPISODE_STR).FirstOrDefault().SetterAction(retval, m.Groups[EPISODE_STR].Value);

                                exitLoop = true;
                            }
                        }
                    }

                }
            }
            else
                _logger.LogError("Unable to infer series/season from pathname, please check folder requirements");

            // Determine capture groups we need
            // Mandatory capture groups
            /*
            foreach (string expression in expressionList)
            {
                Regex r = new Regex(expression);

                Match m = r.Match(subtitleFileInfo.Filebase);

                if (m.Groups.Count >= _groupRefEntries.Length)
                {
                    GroupCollection gc = m.Groups;

                    foreach (var grpRefEntry in _groupRefEntries)
                    {
                        try
                        {
                            if (m.Groups.ContainsKey(grpRefEntry.Key))
                                grpRefEntry.SetterAction(retval, m.Groups[grpRefEntry.Key].Value.Trim());
                        }
                        catch (Exception ex)
                        {
                            grpRefEntry.SetterAction(retval, null);
                        }

                    }
                }
            }
            */

            _logger.LogTrace("Leaving GetTVShowInfo(SubtitleFileInfo, RegExExpression)");
            return retval;
        }

        public TVShowValue GetTVShowInfo(string filename, string RegExExpression = null)
        {
            // TODO: For now this is just a quick copy of the method above. Remove code duplication once RegEx processing is fleshed out.

            TVShowValue retval = new TVShowValue();
            List<string> expressionList;

            if (RegExExpression != null)
                expressionList = new List<string>() { RegExExpression };
            else
                expressionList = _appSettings.RegExTV;

            // Determine capture groups we need
            // Mandatory capture groups

            foreach (string expression in expressionList)
            {
                Regex r = new Regex(expression);

                Match m = r.Match(filename);

                if (m.Groups.Count >= _groupRefEntries.Length)
                {
                    GroupCollection gc = m.Groups;

                    foreach (var grpRefEntry in _groupRefEntries)
                    {
                        try
                        {
                            if (m.Groups.ContainsKey(grpRefEntry.Key))
                                grpRefEntry.SetterAction(retval, m.Groups[grpRefEntry.Key].Value.Trim());
                        }
                        catch (Exception ex)
                        {
                            grpRefEntry.SetterAction(retval, null);
                        }

                    }
                }
            }

            return retval;
        }
    }
}
