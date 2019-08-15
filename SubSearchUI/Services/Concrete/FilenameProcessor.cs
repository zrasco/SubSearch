using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
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
        const string SERIES_STR = "series";
        const string SEASON_STR = "season";
        const string EPISODE_STR = "episode";
        const string TITLE_STR = "title";
        const string QUALITY_STR = "quality";
        const string DATE_STR = "date";

        GroupRefEntry[] groupRefEntries = new GroupRefEntry [] {
            new GroupRefEntry() { Key = SERIES_STR, SetterAction = (o, input) => o.Series = input },
            new GroupRefEntry() { Key = SEASON_STR, SetterAction = (o, input) => o.Season = Int32.Parse(input) },
            new GroupRefEntry() { Key = EPISODE_STR, SetterAction = (o, input) => o.EpisodeNbr = Int32.Parse(input) },
            new GroupRefEntry() { Key = TITLE_STR, SetterAction = (o, input) => o.Title = input },
            new GroupRefEntry() { Key = QUALITY_STR, SetterAction = (o, input) => o.Quality = input },
            new GroupRefEntry() { Key = DATE_STR, SetterAction = (o, input) => o.Date = Convert.ToDateTime(input) },
        };

        private readonly AppSettings _appSettings;

        public FilenameProcessor(IWritableOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

        }
        public TVShowValue GetTVShowInfo(string filename, string RegExExpression = null)
        {
            TVShowValue retval = new TVShowValue();
            List<string> expressionList;

            if (RegExExpression != null)
                expressionList = new List<string>() { RegExExpression };
            else
                expressionList = _appSettings.RegExTV;

            foreach (string expression in expressionList)
            {
                Regex r = new Regex(expression);

                Match m = r.Match(filename);

                if (m.Groups.Count >= groupRefEntries.Length)
                {
                    GroupCollection gc = m.Groups;

                    foreach (var grpRefEntry in groupRefEntries)
                    {
                        if (m.Groups.ContainsKey(grpRefEntry.Key))
                            grpRefEntry.SetterAction(retval, m.Groups[grpRefEntry.Key].Value.Trim());
                    }
                }
            }

            return retval;
        }
    }
}
