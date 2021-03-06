﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeatherDataAnalysis.Model;

namespace UnitTestWeatherDataApp.Model.WeatherData
{
    //  Input ({WeatherData.Days} in WeatherData)                                            Expected output
    //  [{1/1/2015,50,15,1}]                                                                 [50]
    //  [{1/1/2015,50,15,1}, {1/2/2015,45,25,1}                                              [50,45]
    //  [{1/1/2015,50,15,1}, {1/2/2015,45,25,1}, {1/3/2015,40,20,1}, {1/4/2015,50,15,1}]     [50,45,40,50]
    //  [{1/1/2015,50,15,1}, {1/1/2016, 50, 15,1}]                                           [50]
    //  [{}]                                                                                 empty list
    [TestClass]
    public class TestFindHighTemperaturesForYear
    {
        #region Data members

        private WeatherCalculator weatherData;
        private List<DailyStats> days;
        private List<int> testList;
        private DailyStats day1;
        private DailyStats day2;
        private DailyStats day3;
        private DailyStats day4;
        private DailyStats day5;

        #endregion

        #region Methods

        [TestInitialize]
        public void TestInit()
        {
            this.days = new List<DailyStats>();
            this.testList = new List<int>();
            this.day1 = new DailyStats(new DateTime(2015, 1, 1), 50, 15, 1);
            this.day2 = new DailyStats(new DateTime(2015, 1, 2), 45, 25, 1);
            this.day3 = new DailyStats(new DateTime(2015, 1, 3), 40, 30, 1);
            this.day4 = new DailyStats(new DateTime(2015, 1, 4), 50, 15, 1);
            this.day5 = new DailyStats(new DateTime(2016, 1, 1), 50, 15, 1);
        }

        [TestMethod]
        public void TestOneDayInData()
        {
            this.days.Add(this.day1);
            this.testList.Add(50);
            this.weatherData = new WeatherCalculator(this.days);
            CollectionAssert.AreEquivalent(this.testList,
                this.weatherData.FindHighTemperaturesForYear(this.day1.Date.Year));
        }

        [TestMethod]
        public void TestTwoDays()
        {
            this.testList.Add(50);
            this.testList.Add(45);

            this.days.Add(this.day1);
            this.days.Add(this.day2);
            this.weatherData = new WeatherCalculator(this.days);
            CollectionAssert.AreEquivalent(this.testList,
                this.weatherData.FindHighTemperaturesForYear(this.day1.Date.Year));
        }

        [TestMethod]
        public void TestMultipleHighTemps()
        {
            this.testList.Add(50);
            this.testList.Add(45);
            this.testList.Add(40);
            this.testList.Add(50);

            this.days.Add(this.day1);
            this.days.Add(this.day2);
            this.days.Add(this.day3);
            this.days.Add(this.day4);
            this.weatherData = new WeatherCalculator(this.days);
            CollectionAssert.AreEquivalent(this.testList,
                this.weatherData.FindHighTemperaturesForYear(this.day1.Date.Year));
        }

        [TestMethod]
        public void TestDifferentYearNotReturned()
        {
            this.testList.Add(50);

            this.days.Add(this.day1);
            this.days.Add(this.day5);
            this.weatherData = new WeatherCalculator(this.days);
            CollectionAssert.AreEquivalent(this.testList,
                this.weatherData.FindHighTemperaturesForYear(this.day1.Date.Year));
        }

        [TestMethod]
        public void TestEmptyList()
        {
            this.weatherData = new WeatherCalculator(this.days);
            CollectionAssert.AreEquivalent(this.testList,
                this.weatherData.FindHighTemperaturesForYear(this.day1.Date.Year));
        }

        #endregion
    }
}