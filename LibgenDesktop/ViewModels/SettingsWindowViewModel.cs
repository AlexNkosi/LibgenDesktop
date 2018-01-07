﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibgenDesktop.Infrastructure;
using LibgenDesktop.Models;
using LibgenDesktop.Models.Settings;

namespace LibgenDesktop.ViewModels
{
    internal class SettingsWindowViewModel : ViewModel, INotifyDataErrorInfo
    {
        private readonly MainModel mainModel;
        private readonly Dictionary<string, string> errors;
        private bool isNetworkTabSelected;
        private bool isSearchTabSelected;
        private bool networkIsOfflineModeOn;
        private ObservableCollection<string> networkMirrors;
        private string networkSelectedMirror;
        private bool searchIsLimitResultsOn;
        private ObservableCollection<string> searchMaximumResultCountDefaultValues;
        private string searchMaximumResultCount;
        private bool searchIsOpenInModalWindowSelected;
        private bool searchIsOpenInNonModalWindowSelected;
        private bool searchIsOpenInNewTabSelected;
        private bool isOkButtonEnabled;

        public SettingsWindowViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;
            errors = new Dictionary<string, string>();
            OkCommand = new Command(OkButtonClick);
            CancelCommand = new Command(CancelButtonClick);
            Initialize();
        }

        public bool IsNetworkTabSelected
        {
            get
            {
                return isNetworkTabSelected;
            }
            set
            {
                isNetworkTabSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsSearchTabSelected
        {
            get
            {
                return isSearchTabSelected;
            }
            set
            {
                isSearchTabSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool NetworkIsOfflineModeOn
        {
            get
            {
                return networkIsOfflineModeOn;
            }
            set
            {
                networkIsOfflineModeOn = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> NetworkMirrors
        {
            get
            {
                return networkMirrors;
            }
            set
            {
                networkMirrors = value;
                NotifyPropertyChanged();
            }
        }

        public string NetworkSelectedMirror
        {
            get
            {
                return networkSelectedMirror;
            }
            set
            {
                networkSelectedMirror = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchIsLimitResultsOn
        {
            get
            {
                return searchIsLimitResultsOn;
            }
            set
            {
                searchIsLimitResultsOn = value;
                NotifyPropertyChanged();
                Validate();
            }
        }

        public ObservableCollection<string> SearchMaximumResultCountDefaultValues
        {
            get
            {
                return searchMaximumResultCountDefaultValues;
            }
            set
            {
                searchMaximumResultCountDefaultValues = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchMaximumResultCount
        {
            get
            {
                return searchMaximumResultCount;
            }
            set
            {
                searchMaximumResultCount = value;
                NotifyPropertyChanged();
                Validate();
            }
        }

        public bool SearchIsOpenInModalWindowSelected
        {
            get
            {
                return searchIsOpenInModalWindowSelected;
            }
            set
            {
                searchIsOpenInModalWindowSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchIsOpenInNonModalWindowSelected
        {
            get
            {
                return searchIsOpenInNonModalWindowSelected;
            }
            set
            {
                searchIsOpenInNonModalWindowSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchIsOpenInNewTabSelected
        {
            get
            {
                return searchIsOpenInNewTabSelected;
            }
            set
            {
                searchIsOpenInNewTabSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsOkButtonEnabled
        {
            get
            {
                return isOkButtonEnabled;
            }
            set
            {
                isOkButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasErrors => errors.Any();

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        private int? SearchMaximumResultCountValue
        {
            get
            {
                if (Int32.TryParse(SearchMaximumResultCount, out int value))
                {
                    if (value > 0)
                    {
                        return value;
                    }
                }
                return null;
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (errors.TryGetValue(propertyName, out string error))
            {
                yield return error;
            }
        }

        private void Initialize()
        {
            AppSettings appSettings = mainModel.AppSettings;
            isNetworkTabSelected = true;
            isSearchTabSelected = false;
            networkIsOfflineModeOn = appSettings.Network.OfflineMode;
            networkMirrors = new ObservableCollection<string>(mainModel.Mirrors.Keys);
            networkSelectedMirror = appSettings.Network.MirrorName;
            searchMaximumResultCountDefaultValues = new ObservableCollection<string> { "100", "250", "500", "1000", "2500", "5000", "10000", "25000", "50000", "100000", "250000", "500000", "1000000" };
            searchIsLimitResultsOn = appSettings.Search.LimitResults;
            searchMaximumResultCount = appSettings.Search.MaximumResultCount.ToString();
            searchIsOpenInModalWindowSelected = false;
            searchIsOpenInNonModalWindowSelected = false;
            searchIsOpenInNewTabSelected = false;
            switch (appSettings.Search.OpenDetailsMode)
            {
                case AppSettings.SearchSettings.DetailsMode.NEW_MODAL_WINDOW:
                    searchIsOpenInModalWindowSelected = true;
                    break;
                case AppSettings.SearchSettings.DetailsMode.NEW_NON_MODAL_WINDOW:
                    searchIsOpenInNonModalWindowSelected = true;
                    break;
                case AppSettings.SearchSettings.DetailsMode.NEW_TAB:
                    searchIsOpenInNewTabSelected = true;
                    break;
            }
            Validate();
        }

        private void Validate()
        {
            bool isValid = true;
            if (SearchIsLimitResultsOn && SearchMaximumResultCountValue == null)
            {
                isValid = false;
            }
            string propertyName = nameof(SearchMaximumResultCount);
            if (isValid && errors.Any())
            {
                errors.Clear();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            else if (!isValid && !errors.Any())
            {
                errors.Add(propertyName, "Только положительные числа");
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            IsOkButtonEnabled = isValid;
        }

        private void OkButtonClick()
        {
            mainModel.AppSettings.Network = new AppSettings.NetworkSettings
            {
                OfflineMode = NetworkIsOfflineModeOn,
                MirrorName = NetworkSelectedMirror
            };
            mainModel.AppSettings.Search = new AppSettings.SearchSettings
            {
                LimitResults = SearchIsLimitResultsOn,
                MaximumResultCount = SearchMaximumResultCountValue.Value
            };
            if (SearchIsOpenInModalWindowSelected)
            {
                mainModel.AppSettings.Search.OpenDetailsMode = AppSettings.SearchSettings.DetailsMode.NEW_MODAL_WINDOW;
            }
            else if (SearchIsOpenInNonModalWindowSelected)
            {
                mainModel.AppSettings.Search.OpenDetailsMode = AppSettings.SearchSettings.DetailsMode.NEW_NON_MODAL_WINDOW;
            }
            else if (SearchIsOpenInNewTabSelected)
            {
                mainModel.AppSettings.Search.OpenDetailsMode = AppSettings.SearchSettings.DetailsMode.NEW_TAB;
            }
            mainModel.SaveSettings();
            CurrentWindowContext.CloseDialog(true);
        }

        private void CancelButtonClick()
        {
            CurrentWindowContext.CloseDialog(false);
        }
    }
}