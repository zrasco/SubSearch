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
            new GroupRefEntry() { Key = SERIES_STR, SetterAction = (o, input) =>
            {
                if (o.Series == null)
                    o.Series = new List<string>() { input };
                else
                    o.Series.Add(input);
            }
            },
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

        public TVShowValue GetTVShowInfo(SubtitleFileInfo parameter)
        {
            _logger.LogTrace($"{App.GetCaller()}() entering");

            TVShowValue retval = new TVShowValue();
            List<string> showNameCandidates = new List<string>();
            TVShowValue tvShowValue = null;

            // Try to get series name from filename first
            tvShowValue = InfoFromFilebase(parameter.Filebase);

            // Add the series name inferred from the filebase
            if (tvShowValue != null)
            {
                if (tvShowValue.Season.HasValue && !retval.Season.HasValue)
                    retval.Season = tvShowValue.Season;

                if (tvShowValue.EpisodeNbr.HasValue && !retval.EpisodeNbr.HasValue)
                    retval.EpisodeNbr = tvShowValue.EpisodeNbr;

                tvShowValue.Series.ForEach(x => showNameCandidates.Add(x));
            }

            // Try to get series name from path, two directory levels. Format: Series name\Season <number>\Series name - S<s#>E<ep#> - Title.ext or similar
            tvShowValue = InfoFromFilePathTwo(parameter.FullPath, parameter.Filebase);

            // Add the series name inferred from the two-level path
            if (tvShowValue != null)
            {
                if (tvShowValue.Season.HasValue && !retval.Season.HasValue)
                    retval.Season = tvShowValue.Season;

                if (tvShowValue.EpisodeNbr.HasValue && !retval.EpisodeNbr.HasValue)
                    retval.EpisodeNbr = tvShowValue.EpisodeNbr;

                tvShowValue.Series.ForEach(x => showNameCandidates.Add(x));
            }

            // Try to get series name from path, one directory level. Format: Series name\Series name - S<s#>E<ep#> - Title.ext or similar
            // TODO: Add InfoFromFilePathOne() function

            // Try to clean up the series candidate names
            for (int x = 0; x < showNameCandidates.Count; x++)
            {
                // Trim unneeded characters from beginning and end that may be left over from RegEx
                char[] charsToTrim = { ',', '.', ' ', '-' };
                string[] falsePositives =
                {
                    "Videos",
                    "TV Shows",
                    "Movies"
                };

                showNameCandidates[x] = showNameCandidates[x].Trim(charsToTrim);

                // Remove any tags enclosed in () or []
                if (showNameCandidates[x].Contains("(") && showNameCandidates[x].Contains(")"))
                    showNameCandidates[x] = RemoveEnclosedText(showNameCandidates[x], "(", ")");

                if (showNameCandidates[x].Contains("[") && showNameCandidates[x].Contains("]"))
                    showNameCandidates[x] = RemoveEnclosedText(showNameCandidates[x], "[", "]");

                // Remove any of the false positives above
                if (falsePositives.Contains(showNameCandidates[x]))
                    showNameCandidates[x] = "";
            }

            // Remove all nulls and empties from step above
            while (showNameCandidates.Contains(""))
                showNameCandidates.Remove("");


            if (!retval.Season.HasValue || !retval.EpisodeNbr.HasValue)
                retval = null;
            else
            {
                retval.Series = new List<string>(showNameCandidates);

                foreach (string showName in showNameCandidates)
                {
                    // Try replacing AND with &
                    if (showName.Contains(" AND ", StringComparison.OrdinalIgnoreCase))
                        retval.Series.Add(showName.Replace(" AND ", " & ", StringComparison.OrdinalIgnoreCase));

                    // Try replacing & with AND
                    if (showName.Contains(" & ", StringComparison.OrdinalIgnoreCase))
                        retval.Series.Add(showName.Replace(" & ", " and ", StringComparison.OrdinalIgnoreCase));

                    // Try replacing hyphens with a space
                    if (showName.Contains("-", StringComparison.OrdinalIgnoreCase))
                        retval.Series.Add(showName.Replace("-", " ", StringComparison.OrdinalIgnoreCase));
                }
            }

            // Remove duplicates
            retval.Series = retval.Series.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            _logger.LogTrace($"{App.GetCaller()}() exiting");

            return retval;
        }

        private string RemoveEnclosedText(string input, string leftThing, string rightThing, bool trim = true)
        // Removes all text between two "things" such as () or [], including the things themselves.
        // Result is trimmed of whitespace
        // Examples:
        // RemoveEnclosedText("Hello [world]","[","]") == "Hello"
        // RemoveEnclosedText("Hello (there) [person]
        {
            string retval = input;
            bool findingNew = true;

            while (findingNew)
            {
                int pos1 = retval.IndexOf(leftThing);
                int pos2 = retval.IndexOf(rightThing);

                if (pos1 != -1 && pos2 != -1 && pos1 < pos2)
                {
                    retval = retval.Substring(0, pos1) + retval.Substring(pos2 + 1, retval.Length - pos2 - 1);

                    // Trim extra whitespace
                    while (retval.Contains("  "))
                        retval = retval.Replace("  ", " ");
                }
                else
                    findingNew = false;
            }

            if (trim)
                retval = retval.Trim();
            
            return retval;

        }
        public TVShowValue InfoFromFilebase(string fileBase, string RegExExpression = null)
        // If RegExExpression != null, it will use that instead of the list in appSettings.json
        // Returns a null TVShowValue if unable to parse at least the series, season # and episode #
        {
            _logger.LogTrace($"{App.GetCaller()}() entering");
            TVShowValue retval = new TVShowValue();
            List<string> expressionList;
            bool success = false;

            if (RegExExpression != null)
                expressionList = new List<string>() { RegExExpression };
            else
                expressionList = _appSettings.RegExTV;

            // Determine capture groups we need
            // Mandatory capture groups
            foreach (string expression in expressionList)
            {
                Regex r = new Regex(expression);

                Match m = r.Match(fileBase);

                if (m.Groups.Count >= 3)
                {
                    GroupCollection gc = m.Groups;

                    foreach (var grpRefEntry in _groupRefEntries)
                    {
                        try
                        {
                            if (m.Groups.ContainsKey(grpRefEntry.Key))
                                grpRefEntry.SetterAction(retval, m.Groups[grpRefEntry.Key].Value.Trim());
                        }
                        catch (Exception)
                        {
                            grpRefEntry.SetterAction(retval, null);
                        }

                    }
                }

                if (m.Success &&
                    m.Groups.ContainsKey(SERIES_STR) &&
                    m.Groups.ContainsKey(SEASON_STR) &&
                    m.Groups.ContainsKey(EPISODE_STR))
                {
                    success = true;
                    // Do nothing because retval already has what we need from setter actions above
                }
            }

            if (!success)
                retval = null;

            _logger.LogTrace($"{App.GetCaller()}() exiting");
            return retval;
        }

        public TVShowValue InfoFromFilePathTwo(string filePath, string fileBase, string RegExExpression = null)
        // If RegExExpression != null, it will use that instead of the list in appSettings.json
        // Returns a null TVShowValue if unable to parse at least the series, season # and episode #
        {
            _logger.LogTrace($"{App.GetCaller()}() entering");

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

            var folderNamesArray = filePath.Split(@"\");

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
                        Match m = r.Match(fileBase);

                        // Only need to get the episode number
                        if (m.Groups.Count >= 1)
                        {
                            if (m.Groups.ContainsKey(EPISODE_STR))
                            {
                                retval.Series = new List<string>() { series };
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

            _logger.LogTrace($"{App.GetCaller()}() exiting");
            return retval;
        }
    }
}
