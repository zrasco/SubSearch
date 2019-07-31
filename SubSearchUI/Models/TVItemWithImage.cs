using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SubSearchUI.Models
{
    public class TVItemWithImage : ItemWithImage
    {
        /// <summary>
        /// The <see cref="SubItems" /> property's name.
        /// </summary>
        public const string SubItemsPropertyName = "SubItems";

        private ObservableCollection<TVDirectoryItem> _subItems;

        /// <summary>
        /// Sets and gets the SubItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<TVDirectoryItem> SubItems
        {
            get
            {
                return _subItems;
            }

            set
            {
                if (_subItems == value)
                {
                    return;
                }

                _subItems = value;
                RaisePropertyChanged(SubItemsPropertyName);
            }
        }
    }
}
