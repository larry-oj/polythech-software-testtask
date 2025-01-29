#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestTasks.WeatherFromAPI.Models
{
    public class WeatherResponse
    {
        public List<HourlyWeather> Data { get; set; } = new();
        public List<HourlyWeather> Hourly { get; set; } = new();
    }

    public class HourlyWeather
    {
        public double Temp { get; set; }
        public RainVolume? Rain { get; set; }
    }

    public class RainVolume
    {
        [JsonPropertyName("1h")]
        public double Volume { get; set; }
    }
}