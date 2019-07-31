﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubSearchUI.Services.Abstract
{
    interface IProviderPlugin
    {
        public enum SearchCapabilities { None = 0, Hash = 1, TV = 2};
        SearchCapabilities ProviderCapabilities();
        Task<IList<Stream>> SearchSubtitlesByHashAsync(string fileHash, long fileSize, IList<string> languages);
        Task<IList<Stream>> SearchSubtitlesForTVAsync(string showName, int seasonNbr, int episodeNbr, IList<string> languages);
    }
}