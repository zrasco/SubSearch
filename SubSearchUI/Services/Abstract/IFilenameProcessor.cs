using SubSearchUI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Services.Abstract
{
    public class TVShowValue
    {
        public string Series { get; set; }
        public int Season { get; set; }
        public int EpisodeNbr { get; set; }
        public string Title { get; set; }
        public string Quality { get; set; }
        public DateTime Date { get; set; }
    }
    public interface IFilenameProcessor
    {
        // Infer series name and season from pathname
        TVShowValue GetTVShowInfo(SubtitleFileInfo subtitleFileInfo, string RegExExpression = null);

        // String must at least contain series name and season/episode numbers
        TVShowValue GetTVShowInfo(string filename, string RegExExpression = null);
    }
}
