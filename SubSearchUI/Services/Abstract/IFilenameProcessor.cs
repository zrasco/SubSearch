using SubSearchUI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Services.Abstract
{
    public class TVShowValue
    {
        public List<string> Series { get; set; }
        public int? Season { get; set; }
        public int? EpisodeNbr { get; set; }
        public string Title { get; set; }
        public string Quality { get; set; }
        public DateTime Date { get; set; }
    }
    public interface IFilenameProcessor
    {
        TVShowValue InfoFromFilebase(string fileBase, string RegExExpression);
        TVShowValue InfoFromFilePathTwo(string filePath, string fileBase, string RegExExpression);
        TVShowValue GetTVShowInfo(SubtitleFileInfo parameter);
    }
}
