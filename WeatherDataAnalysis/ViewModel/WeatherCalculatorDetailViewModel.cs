﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using WeatherDataAnalysis.DataTier;
using WeatherDataAnalysis.Extension;
using WeatherDataAnalysis.Model;
using WeatherDataAnalysis.Utility;
using WeatherDataAnalysis.View;

namespace WeatherDataAnalysis.ViewModel
{
    /// <summary>
    ///     The view model for the weather calculator
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class WeatherCalculatorDetailViewModel : INotifyPropertyChanged
    {
        #region Data members

        private WeatherCalculator weatherCalculator;

        private DateTime date;

        private int highTemperature;

        private int lowTemperature;

        private double precipitation;

        private DailyStats selectedDay;

        private ObservableCollection<DailyStats> days;

        private DuplicateDayResult duplicateBehavior;

        private string report;

        private WeatherDataParser parser;
        #endregion

        #region Properties





        private string dateForDisplay;

        public string DateForDisplay
        {
            get => this.dateForDisplay;
            set
            {
                this.dateForDisplay = value;
                this.OnPropertyChanged();
            }
        }







        public string Report
        {
            get => this.report; 

            set
            {
                this.report = value;
                this.OnPropertyChanged();
                this.SummaryCommand.OnCanExecuteChanged();
            }
        }


        /// <summary>
        ///     Gets or sets the remove command.
        /// </summary>
        /// <value>
        ///     The remove command.
        /// </value>
        public RelayCommand RemoveCommand { get; set; }

        /// <summary>
        ///     Gets or sets the add command.
        /// </summary>
        /// <value>
        ///     The add command.
        /// </value>
        public RelayCommand AddCommand { get; set; }

        /// <summary>
        ///     Gets or sets the edit command.
        /// </summary>
        /// <value>
        ///     The edit command.
        /// </value>
        public RelayCommand EditCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear data command.
        /// </summary>
        /// <value>
        ///     The clear data command.
        /// </value>
        public RelayCommand ClearDataCommand { get; set; }

        /// <summary>
        /// Gets or sets the summary command.
        /// </summary>
        /// <value>
        /// The summary command.
        /// </value>
        public RelayCommand SummaryCommand { get; set; }

        /// <summary>
        ///     Gets or sets the date.
        /// </summary>
        /// <value>
        ///     The date.
        /// </value>
        public DateTime Date
        {
            get => this.date;
            set
            {
                this.date = value;
                this.OnPropertyChanged();
                this.AddCommand.OnCanExecuteChanged();
                this.EditCommand.OnCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the high temperature.
        /// </summary>
        /// <value>
        ///     The high temperature.
        /// </value>
        public int HighTemperature
        {
            get => this.highTemperature;
            set
            {
                this.highTemperature = value;
                this.OnPropertyChanged();
                this.AddCommand.OnCanExecuteChanged();
                this.EditCommand.OnCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the low temperature.
        /// </summary>
        /// <value>
        ///     The low temperature.
        /// </value>
        public int LowTemperature
        {
            get => this.lowTemperature;
            set
            {
                this.lowTemperature = value;
                this.OnPropertyChanged();
                this.AddCommand.OnCanExecuteChanged();
                this.EditCommand.OnCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the precipitation.
        /// </summary>
        /// <value>
        ///     The precipitation.
        /// </value>
        public double Precipitation
        {
            get => this.precipitation;
            set
            {
                this.precipitation = value;
                this.OnPropertyChanged();
                this.AddCommand.OnCanExecuteChanged();
                this.EditCommand.OnCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the selected day.
        /// </summary>
        /// <value>
        ///     The selected day.
        /// </value>
        public DailyStats SelectedDay
        {
            get => this.selectedDay;
            set
            {
                this.selectedDay = value;
                this.OnPropertyChanged();
                this.RemoveCommand.OnCanExecuteChanged();
                this.EditCommand.OnCanExecuteChanged();
                this.Date = this.selectedDay.Date;
                this.HighTemperature = this.selectedDay.HighTemperature;
                this.LowTemperature = this.selectedDay.LowTemperature;
                this.Precipitation = this.selectedDay.Precipitation;
            }
        }

        /// <summary>
        ///     Gets or sets the days.
        /// </summary>
        /// <value>
        ///     The days.
        /// </value>
        public ObservableCollection<DailyStats> Days
        {
            get => this.days;
            set
            {
                this.days = value;
                this.OnPropertyChanged();
                this.ClearDataCommand?.OnCanExecuteChanged();
                this.SummaryCommand?.OnCanExecuteChanged();
            }
        }

        public MergeOrReplaceResult MergeOrReplace { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeatherCalculatorDetailViewModel" /> class.
        /// </summary>
        public WeatherCalculatorDetailViewModel()
        {
            this.date = DateTime.Now;
            this.parser = new WeatherDataParser();
            this.weatherCalculator = new WeatherCalculator(new List<DailyStats>());
            this.Days = this.weatherCalculator.Days.ToObservableCollection();
            this.initializeCommands();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        /// <returns> the event</returns>
        public event PropertyChangedEventHandler PropertyChanged;

        private void initializeCommands()
        {
            this.RemoveCommand = new RelayCommand(this.deleteDay, this.canDeleteDay);
            this.AddCommand = new RelayCommand(this.addDay, this.canAddDay);
            this.EditCommand = new RelayCommand(this.editDay, this.canEditDay);
            this.ClearDataCommand = new RelayCommand(this.clearData, this.canClearData);
            this.SummaryCommand = new RelayCommand(this.createSummary, this.canCreateSummary);
        }

        private bool canCreateSummary(object obj)
        {
            return this.Days.Count > 0;
        }

        private void createSummary(object obj)
        {
            this.weatherCalculator.HighTemperatureThreshold = 20;
            this.weatherCalculator.LowTemperatureThreshold = 20;
            this.weatherCalculator.HistogramBucketSize = 5;
            var reportBuilder = new ReportBuilder(this.weatherCalculator);
            reportBuilder.CompileReport();
            this.Report = reportBuilder.Report;
        }

        private bool canClearData(object obj)
        {
            return this.Days.Count > 0;
        }

        private void clearData(object obj)
        {
            this.weatherCalculator.Days.Clear();
            this.UpdateDays();
        }

        private bool canEditDay(object obj)
        {
            return this.HighTemperature > this.LowTemperature && this.SelectedDay != null && this.SelectedDay.Date == this.Date;
        }

        private void editDay(object obj)
        {
            var index = this.weatherCalculator.Days.IndexOf(this.SelectedDay);
            this.weatherCalculator.Days[index].HighTemperature = this.HighTemperature;
            this.weatherCalculator.Days[index].LowTemperature = this.LowTemperature;
            this.weatherCalculator.Days[index].Precipitation = this.Precipitation;

            this.Days = this.weatherCalculator.Days.ToObservableCollection();
        }

        private bool canAddDay(object obj)
        {
            return this.HighTemperature > this.LowTemperature && this.weatherCalculator.FindDayWithDate(this.Date) == null;
        }

        private void addDay(object obj)
        {
            var dayToAdd = new DailyStats(this.Date, this.HighTemperature, this.LowTemperature,
                this.Precipitation);
            this.weatherCalculator.Add(dayToAdd);
            this.weatherCalculator.Days = this.weatherCalculator.Days.OrderBy(day => day.Date).ToList();
            this.UpdateDays();
        }

        private bool canDeleteDay(object obj)
        {
            return this.SelectedDay != null;
        }

        private void deleteDay(object obj)
        {
            this.weatherCalculator.Remove(this.SelectedDay);
            this.UpdateDays();
        }

        /// <summary>
        ///     Reads the file.
        /// </summary>
        /// <param name="file">The file to be read.</param>
        public async void ReadFile(StorageFile file)
        {
            this.weatherCalculator = new WeatherCalculator(await this.parser.LoadFile(file));
            this.UpdateDays();

        }

        public async Task ReadNewFile(StorageFile file)
        {
            this.weatherCalculator =  new WeatherCalculator(this.weatherCalculator, await this.parser.LoadFile(file));
            this.UpdateDays();
 
        }

        public void SaveFile(StorageFile file)
        {
            var fileSaver = new WeatherDataFileSaver();
            fileSaver.SaveFile(this.weatherCalculator.Days, file);
        }

        public ICollection<IGrouping<int, DailyStats>> FindDuplicateDays()
        {
            return this.weatherCalculator.ConflictingDays;
        }

        public void UpdateDays()
        {
            this.weatherCalculator.Days = this.weatherCalculator.Days.OrderBy(day => day.Date).ToList();
            this.Days = this.weatherCalculator.Days.ToObservableCollection();
        }

        public ICollection<DailyStats> FindNextConflictingDays()
        {
            return this.weatherCalculator.FindNextConflictingDays();
        }

        public void Merge(bool action)
        {
            this.weatherCalculator.Merge(action);
        }


        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}