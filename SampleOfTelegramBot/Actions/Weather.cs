using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            return await GetWeatherFromPogodaByAsync();
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
    }
}
