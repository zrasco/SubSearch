﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProviderPluginTypes
{
    public enum SearchCapabilities { None = 0, Hash = 1, TV = 2 };

    public class DownloadedSubtitle
    {
        public Stream Contents { get; set; }
        public CultureInfo CultureInfo { get; set; }
    }

    public class ProviderSignal
    {
        public enum SignalType { ChangeDetails, ChangePercentage };
        public SignalType Type { get; set; }
        public double DoubleValue { get; set; }
        public string TextValue { get; set; }
    }

    public interface IProviderPlugin
    {
        // Called when the plugin is loaded into the program
        Task InitAsync(ConcurrentQueue<ProviderSignal> cQueue);
        // Perform any cleanup here
        Task UnloadAsync(ConcurrentQueue<ProviderSignal> cQueue);
        string Version();
        // Reports the capabilities of the provider (can search by hash, etc...). Use a bitwise OR with the SearchCapabilities above
        SearchCapabilities ProviderCapabilities();
        // Search by file hash
        Task<IList<DownloadedSubtitle>> SearchSubtitlesByHashAsync(string fileHash,
                                                                    long fileSize,
                                                                    IList<CultureInfo> cultureInfos,
                                                                    ConcurrentQueue<ProviderSignal> cQueue);
        // Search by show, season and episode #
        Task<IList<DownloadedSubtitle>> SearchSubtitlesForTVAsync(IList<string> showNameCandidates,
                                                                    int seasonNbr,
                                                                    int episodeNbr,
                                                                    IList<CultureInfo> cultureInfos,
                                                                    ConcurrentQueue<ProviderSignal> cQueue);
        
        
    }
}