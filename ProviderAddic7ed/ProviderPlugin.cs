using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProviderAddic7ed
{
    public class ProviderPlugin : IProviderPlugin
    {
        public SearchCapabilities ProviderCapabilities()
        {
            return SearchCapabilities.Hash & SearchCapabilities.TV;
        }

        public Task<IList<Stream>> SearchSubtitlesByHashAsync(string fileHash, long fileSize, IList<string> languages)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Stream>> SearchSubtitlesForTVAsync(string showName, int seasonNbr, int episodeNbr, IList<string> languages)
        {
            throw new NotImplementedException();
        }
    }
}
