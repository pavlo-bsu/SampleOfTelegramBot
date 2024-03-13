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

        /// <summary>
        /// Content of btn that represent Minsk weather
        /// </summary>
        public string BtnMinskName
        { get; set; }

        /// <summary>
        /// Callback data for 'Minsk weather' btn
        /// </summary>
        public string BtnMinskCallbackData
        { get; set; }

        /// <summary>
        /// Content of btn that represent Abu Dhabi weather
        /// </summary>
        public string BtnAbuDhabiName
        { get; set; }

        /// <summary>
        /// Callback data for 'Abu Dhabi weather' btn
        /// </summary>
        public string BtnAbuDhabiCallbackData
        { get; set; }


        public string AnswerCallbackQuery
        { get; set; }

        public Weather()
        {
            BotOnKeywords = new List<string> {"погода", "weather" };
            BtnMinskName = "Minsk weather \ud83c\udf07";
            BtnMinskCallbackData = "minsk";
            AnswerCallbackQuery = "See the current weather";
            BtnAbuDhabiName = "Abu Dhabi weather \ud83c\udfdc\ufe0f";
            BtnAbuDhabiCallbackData = "abudhabi";
        }

        /*public async Task<string> GetCurrentWeatherAsync()
        {
            return await GetWeatherFromPogodaByAsync_v_11_2021();
        }*/

        /// <summary>
        /// Get current weather from "pogoda.by", i.e. from minsk weather station
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
        /// Get current weather from "pogoda.by" (after redesign 11.2021), i.e. from minsk weather station (Minsk-Uruchie). 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetWeatherFromPogodaByAsync_v_11_2021()
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

        /// <summary>
        /// Get current weather from "https://www.avmet.ae/", i.e. from AbuDhabi weather station (Zayed International Airport). 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAbuDhabiWeatherFromAvmetAe_v_03_2024_Async()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = await wc.DownloadStringTaskAsync("https://www.avmet.ae/omaa.aspx");

            //cut garbage
            string strBeginning = "Last Updated:";
            string strEnd = "Visibility";
            var indexBeginning = webData.IndexOf(strBeginning);
            var indexEnd = webData.IndexOf(strEnd, indexBeginning);
            string webWeather = webData.Substring(indexBeginning, indexEnd - indexBeginning);


            //get local AbuDhabi time
            string dateBeginning = " title=\"";
            string dateEnd = "\">";
            string dateStr = GetSubStringInBetween(webWeather, dateBeginning, dateEnd);
            DateTime date = DateTime.Parse(dateStr, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            // Convert to UAE local time
            TimeSpan uaeOffset = TimeSpan.FromHours(4);// UAE time zone offset is UTC+4
            date = date + uaeOffset;
            string datetimeOut = date.ToString("dd.MM.yyyy HH:mm");

            //get temperature
            string tempBeginning = "title=\"TAINS: ";
            string tempEnd = "\" class";
            var tempStr = GetSubStringInBetween(webWeather, tempBeginning, tempEnd);
            double temperature = double.Parse(tempStr, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            string temperatureOut = temperature.ToString("+0°C;-0°C");


            // get humidity
            string humBeginning = "title=\"RHINS:";
            string humEnd = "\" class";
            string humStr = GetSubStringInBetween(webWeather, humBeginning, humEnd);
            double humidity = double.Parse(humStr, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            string humidityOut = humidity.ToString("0")+"%";

            // get wind
            string windBeginning = "title=\"WIND10A:";
            string windEnd = "\" class";

            string windStrTmp = GetSubStringInBetween(webWeather, windBeginning, windEnd);
            var windStr = windStrTmp.Split('/')[1];
            //velocity [Knot]
            double windKnot = double.Parse(windStr, System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            //velocity [m/s]
            double windMPS = windKnot * 0.514;
            string windOut = windMPS.ToString("0m/s");

            //total string
            string result = $"Abu Dhabi weather\n\r{datetimeOut}\n\rTemperature: {temperatureOut}\r\nHumidity: {humidityOut}\r\nWind: {windOut}";

            return result;
        }

        /// <summary>
        /// get substring excluding the beginning and the ending
        /// </summary>
        /// <param name="totalStr">source string</param>
        /// <param name="subStrBeginning">beginning</param>
        /// <param name="subStrEnd">ending</param>
        /// <returns>source string without the beginning and the ending</returns>
        private string GetSubStringInBetween(string totalStr, string subStrBeginning, string subStrEnd)
        {
            var tempIndexBeginning = totalStr.IndexOf(subStrBeginning);
            var tempIndexEnd = totalStr.IndexOf(subStrEnd, tempIndexBeginning);
            var tempStartShift = subStrBeginning.Length;
            var tempStr = totalStr.Substring(tempIndexBeginning + tempStartShift, tempIndexEnd - tempIndexBeginning - tempStartShift);
            return tempStr;
        }
    }
}
