using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TestTasks.WeatherFromAPI.Models;

namespace TestTasks.WeatherFromAPI
{
    public class WeatherManager
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "";
        private const string GeoCodingUrl = "http://api.openweathermap.org/geo/1.0/direct?q={0}&limit=1&appid={1}";
        private const string OneCallUrl = "https://api.openweathermap.org/data/3.0/onecall/timemachine?lat={0}&lon={1}&dt={2}&appid={3}";

        public WeatherManager()
            :this(new HttpClient())
        {
        }
        public WeatherManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<WeatherComparisonResult> CompareWeather(string cityA, string cityB, int dayCount)
        {
            if (string.IsNullOrWhiteSpace(cityA) || string.IsNullOrWhiteSpace(cityB))
                throw new ArgumentException("City names must be specified.");

            if (dayCount is < 1 or > 5)
                throw new ArgumentException("dayCount must be between 1 and 5.");

            var coordA = await GetCoordinates(cityA);
            var coordB = await GetCoordinates(cityB);

            var now = DateTimeOffset.UtcNow;
            var warmerDays = 0;
            var rainierDays = 0;

            for (var i = 0; i < dayCount; i++)
            {
                var timestamp = now.AddDays(-i).ToUnixTimeSeconds();

                var weatherA = await GetHistoricalWeather(coordA.Lat, coordA.Lon, timestamp);
                var weatherB = await GetHistoricalWeather(coordB.Lat, coordB.Lon, timestamp);
                
                List<HourlyWeather> datasetA;
                List<HourlyWeather> datasetB;
                
                if (weatherA.Hourly.Count > 0 && weatherB.Hourly.Count > 0)
                {
                    datasetA = weatherA.Hourly;
                    datasetB = weatherB.Hourly;
                }
                else
                {
                    datasetA = weatherA.Data;
                    datasetB = weatherB.Data;
                }
                
                var avgTempA = datasetA.Average(h => h.Temp);
                var avgTempB = datasetB.Average(h => h.Temp);
                var totalRainA = datasetA.Sum(h => h.Rain?.Volume ?? 0);
                var totalRainB = datasetB.Sum(h => h.Rain?.Volume ?? 0);

                if (avgTempA > avgTempB) warmerDays++;
                if (totalRainA > totalRainB) rainierDays++;
            }

            return new WeatherComparisonResult(cityA, cityB, warmerDays, rainierDays);
        }

        private async Task<Coordinates> GetCoordinates(string city)
        {
            var url = string.Format(GeoCodingUrl, city, ApiKey);
            var response = await _httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<List<GeocodingResponse>>();

            if (result == null || result.Count == 0)
                throw new ArgumentException($"City '{city}' not found.");

            return new Coordinates(result[0].Lat, result[0].Lon);
        }

        private async Task<WeatherResponse> GetHistoricalWeather(double lat, double lon, long timestamp)
        {
            var url = string.Format(OneCallUrl, lat, lon, timestamp, ApiKey);
            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadFromJsonAsync<WeatherResponse>();
            return responseString ?? throw new Exception("Invalid weather data received.");
        }
    }
}
