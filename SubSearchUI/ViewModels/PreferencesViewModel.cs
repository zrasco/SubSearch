using GalaSoft.MvvmLight;
using Microsoft.Extensions.Options;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace SubSearchUI.ViewModels
{
    public class RegExTVItem : ObservableObject
    {
        /// <summary>
        /// The <see cref="Expression" /> property's name.
        /// </summary>
        public const string PropertyName = "";

        private string _expression;

        /// <summary>
        /// Sets and gets the  property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Expression
        {
            get
            {
                return _expression;
            }

            set
            {
                if (_expression == value)
                {
                    return;
                }

                _expression = value;
                RaisePropertyChanged(PropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Series" /> property's name.
        /// </summary>
        public const string SeriesPropertyName = "Series";

        private string _series = null;

        /// <summary>
        /// Sets and gets the Series property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Series
        {
            get
            {
                return _series;
            }

            set
            {
                if (_series == value)
                {
                    return;
                }

                _series = value;
                RaisePropertyChanged(SeriesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SeasonNbr" /> property's name.
        /// </summary>
        public const string SeasonNbrPropertyName = "SeasonNbr";

        private string _seasonNbr = null;

        /// <summary>
        /// Sets and gets the SeasonNbr property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SeasonNbr
        {
            get
            {
                return _seasonNbr;
            }

            set
            {
                if (_seasonNbr == value)
                {
                    return;
                }

                _seasonNbr = value;
                RaisePropertyChanged(SeasonNbrPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EpNbr" /> property's name.
        /// </summary>
        public const string EpNbrPropertyName = "EpNbr";

        private string _epNbr = null;

        /// <summary>
        /// Sets and gets the EpNbr property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string EpNbr
        {
            get
            {
                return _epNbr;
            }

            set
            {
                if (_epNbr == value)
                {
                    return;
                }

                _epNbr = value;
                RaisePropertyChanged(EpNbrPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _title;

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Quality" /> property's name.
        /// </summary>
        public const string QualityPropertyName = "Quality";

        private string _quality = null;

        /// <summary>
        /// Sets and gets the Quality property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Quality
        {
            get
            {
                return _quality;
            }

            set
            {
                if (_quality == value)
                {
                    return;
                }

                _quality = value;
                RaisePropertyChanged(QualityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Date" /> property's name.
        /// </summary>
        public const string DatePropertyName = "Date";

        private string _date = null;

        /// <summary>
        /// Sets and gets the Date property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Date
        {
            get
            {
                return _date;
            }

            set
            {
                if (_date == value)
                {
                    return;
                }

                _date = value;
                RaisePropertyChanged(DatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BGColor" /> property's name.
        /// </summary>
        public const string BGColorPropertyName = "BGColor";

        private Brush _bgColor = Brushes.Transparent;

        /// <summary>
        /// Sets and gets the TextColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush BGColor
        {
            get
            {
                return _bgColor;
            }

            set
            {
                if (_bgColor == value)
                {
                    return;
                }

                _bgColor = value;
                RaisePropertyChanged(BGColorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TooltipText" /> property's name.
        /// </summary>
        public const string TooltipTextPropertyName = "TooltipText";

        private string _tooltipText = null;

        /// <summary>
        /// Sets and gets the TooltipText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TooltipText
        {
            get
            {
                return _tooltipText;
            }

            set
            {
                if (_tooltipText == value)
                {
                    return;
                }

                _tooltipText = value;
                RaisePropertyChanged(TooltipTextPropertyName);
            }
        }

        public void FillFrom(TVShowValue info)
        {
            Date = info.Date.ToString();
            EpNbr = info.EpisodeNbr.ToString();
            Series = info.Series;
            Title = info.Title;
            Quality = info.Quality;
            SeasonNbr = info.Season.ToString();
        }

        public bool IsComplete()
        {
            return (!string.IsNullOrEmpty(Date) &&
                !string.IsNullOrEmpty(EpNbr) &&
                !string.IsNullOrEmpty(Series) &&
                !string.IsNullOrEmpty(Title) &&
                !string.IsNullOrEmpty(Quality) &&
                !string.IsNullOrEmpty(SeasonNbr));
        }
    }
    public class PreferencesViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="RootDirectory" /> property's name.
        /// </summary>
        public const string RootDirectoryPropertyName = "RootDirectory";

        private string rootDirectory = "";

        /// <summary>
        /// Sets and gets the RootDirectory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RootDirectory
        {
            get
            {
                return rootDirectory;
            }
            set
            {
                if (rootDirectory == value)
                {
                    return;
                }

                rootDirectory = value;
                RaisePropertyChanged(RootDirectoryPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LanguageList" /> property's name.
        /// </summary>
        public const string LanguageListPropertyName = "LanguageList";

        private IList<CultureInfo> _languageList;

        /// <summary>
        /// Sets and gets the LanguageList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IList<CultureInfo> LanguageList
        {
            get
            {
                return _languageList;
            }

            set
            {
                if (_languageList == value)
                {
                    return;
                }

                _languageList = value;
                RaisePropertyChanged(LanguageListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedLanguage" /> property's name.
        /// </summary>
        public const string SelectedLanguagePropertyName = "SelectedLanguage";

        private CultureInfo _selectedLanguage;

        /// <summary>
        /// Sets and gets the SelectedLanguage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CultureInfo SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }

            set
            {
                if (_selectedLanguage == value)
                {
                    return;
                }

                _selectedLanguage = value;
                RaisePropertyChanged(SelectedLanguagePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PluginList" /> property's name.
        /// </summary>
        public const string PluginListPropertyName = "PluginList";

        private ObservableCollection<PluginStatus> _pluginList;

        /// <summary>
        /// Sets and gets the PluginList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PluginStatus> PluginList
        {
            get
            {
                return _pluginList;
            }

            set
            {
                if (_pluginList == value)
                {
                    return;
                }

                _pluginList = value;
                RaisePropertyChanged(PluginListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SampleText" /> property's name.
        /// </summary>      
        public const string SampleTextPropertyName = "SampleText";

        private string _sampleText;

        /// <summary>
        /// Sets and gets the SampleText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SampleText
        {
            get
            {
                return _sampleText;
            }

            set
            {
                if (_sampleText == value)
                {
                    return;
                }

                _sampleText = value;
                RaisePropertyChanged(SampleTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="RegExList" /> property's name.
        /// </summary>
        public const string RegExListPropertyName = "RegExList";

        private ObservableCollection<RegExTVItem> regExList = null;

        /// <summary>
        /// Sets and gets the RegExList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<RegExTVItem> RegExList
        {
            get
            {
                return regExList;
            }

            set
            {
                if (regExList == value)
                {
                    return;
                }

                regExList = value;
                RaisePropertyChanged(RegExListPropertyName);
            }
        }

        public PreferencesViewModel()
        {
        }
    }
}
