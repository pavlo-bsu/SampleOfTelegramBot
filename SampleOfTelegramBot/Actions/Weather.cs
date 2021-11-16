using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pavlo.SampleOfTelegramBot.Actions
{
    public class Weather
    {
        public List<string> BotOnKeywords
        { get; set; }

        public string BtnName
        { get; set; }

        public string BtnCallbackData
        { get; set; }

        public string AnswerCallbackQuery
        { get; set; }

        public Weather()
        {
            BotOnKeywords = new List<string> {"погода", "weather" };
            BtnName = "Weather \ud83c\udf24\ufe0f";
            BtnCallbackData = "weather";
            AnswerCallbackQuery = "See the current weather";
        }

        public async Task<string> GetCurrentWeatherAsync()
        {
            return await GetWeatherFromPogodaByAsync_v_11_2021();
        }

        /// <summary>
        /// Get current temperature from "pogoda.by", i.e. from minsk weather station
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetWeatherFromPogodaByAsync()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.GetEncoding(1251);
            string webData = await wc.DownloadStringTaskAsync("http://pda.pogoda.by/");

            //cut garbage
            string strBeginning = "Сейчас <strong>в Минске</strong>";
            string strEnd = "<br>Давление ";
            var indexBeginning = webData.IndexOf(strBeginning);
            var indexEnd = webData.IndexOf(strEnd, indexBeginning);

            string webWeather = webData.Substring(indexBeginning, indexEnd - indexBeginning);

            //formatting time: UTC to local Minsk time
            string timeBeginning = "Погода за ";
            string timeEnd = ":";
            var timeIndexBeginning = webWeather.IndexOf(timeBeginning);
            var timeIndexEnd = webWeather.IndexOf(timeEnd, timeIndexBeginning);
            int timeStartShift = timeBeginning.Length;

            string hours = webWeather.Substring(timeIndexBeginning + timeStartShift, timeIndexEnd - timeIndexBeginning - timeStartShift);

            int hoursInBYTimeZone = int.Parse(hours) + 3;
            if (hoursInBYTimeZone >= 24)
                hoursInBYTimeZone -= 24;

            webWeather = webWeather.Remove(timeIndexBeginning + timeStartShift, timeIndexEnd - timeIndexBeginning - timeStartShift);
            webWeather = webWeather.Insert(timeIndexBeginning + timeStartShift, hoursInBYTimeZone.ToString());

            string utc = "UTC";
            webWeather = webWeather.Remove(webWeather.IndexOf(utc), utc.Length);

            //cut html symbols
            webWeather = webWeather.Replace("<br>", "\n").Replace("<strong>", string.Empty).Replace("</strong> ", string.Empty).Replace("&deg;", " °");

            return webWeather;
        }
        /// <summary>
        /// Get current temperature from "pogoda.by" (after redesign 11.2021), i.e. from minsk weather station (Minsk-Uruchie). 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetWeatherFromPogodaByAsync_v_11_2021()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            string webData = wc.DownloadString("https://pogoda.by/rss/weather?station=26850");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(webData);

            XmlElement xRoot = xmlDoc.DocumentElement;

            //formatting time: UTC to local Minsk time
            XmlElement weatherDate = xRoot["channel"]["item"];
            string s = weatherDate["pubDate"].InnerText;
            var sArray = s.Split();
            string date = sArray[1] + " " + sArray[2] + " " + sArray[3];

            DateTime d = DateTime.Parse(date, new System.Globalization.CultureInfo("en-US"));

            string time = sArray[4];
            DateTime t = DateTime.Parse(time, new System.Globalization.CultureInfo("en-US"));

            string dateTime = date + " " + time;
            DateTime dt = DateTime.Parse(dateTime, new System.Globalization.CultureInfo("en-US"));

            dt = dt.AddHours(3);

            string datetimeOut = (dt.ToShortDateString() + " " + dt.ToLongTimeString());

            //formatting weather
            string[] weather = weatherDate["description"].InnerText.Split(" | ", StringSplitOptions.None);
            weather = weather.Where(val => !val.ToLowerInvariant().StartsWith("давление")).ToArray();
            var weatherOut = string.Join("\n", weather);

            string outString = "Погода\n"+datetimeOut+"\n"+weatherOut;

            return outString;
        }
    }
}
