using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using static FredBotNETCore.WeatherDataCurrent;

namespace FredBotNETCore
{
    public class WeatherData : ModuleBase
    {
        //public async Task<String> GetWeatherAsync(string city)
        //{
        //    var httpClient = new HttpClient();
        //    string URL = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=55ff8dedf42f55e12479145a2e4fefa4";
        //    var response = await httpClient.GetAsync(URL);
        //    var result = await response.Content.ReadAsStringAsync();
        //    return result;
        //}

        //[Command("weather")]
        //[Alias("Weather")]
        //[Summary("Tells you weather on a location")]
        //public async Task WeatherAsync([Remainder] string city = null)
        //{
        //    try
        //    {
        //        if (city == null)
        //        {
        //            await ReplyAsync($"{Context.User.Mention} you need to enter a city.");
        //            return;
        //        }
        //        WeatherReportCurrent weather;
        //        weather = JsonConvert.DeserializeObject<WeatherReportCurrent>(GetWeatherAsync(city).Result);
        //        double lon = 0;
        //        try
        //        {
        //            lon = weather.Coord.Lon;
        //        }
        //        catch(NullReferenceException)
        //        {
        //            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the city `{city}` does not exist or could not be found.");
        //            return;
        //        }
        //        double lat = weather.Coord.Lat;
        //        double temp = weather.Main.Temp;
        //        int pressure = weather.Main.Pressure;
        //        int humidity = weather.Main.Humidity;
        //        double minTemp = weather.Main.TempMin;
        //        double maxTemp = weather.Main.TempMax;
        //        double speed = weather.Wind.Speed;
        //        double deg = weather.Wind.Deg;
        //        var directions = new string[]
        //        {
        //            "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest", "North"
        //        };
        //        int index = Convert.ToInt32(Math.Round((deg + 23) / 45));
        //        string direction = directions[index];
        //        int all = weather.Clouds.All;
        //        //int type = weather.Sys.Type;
        //        //int id = weather.Sys.Id;
        //        //double message = weather.Sys.Message;
        //        string country = weather.Sys.Country;
        //        int sunrise = weather.Sys.Sunrise;
        //        int sunset = weather.Sys.Sunset;
        //        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //        DateTime sunriseDate = start.AddSeconds(sunrise).ToLocalTime();
        //        DateTime sunsetDate = start.AddSeconds(sunset).ToLocalTime();
        //        EmbedBuilder embed = new EmbedBuilder()
        //        {
        //            Color = new Color(235, 66, 244)
        //        };
        //        EmbedFooterBuilder footer = new EmbedFooterBuilder()
        //        {
        //            IconUrl = Context.User.GetAvatarUrl(),
        //            Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
        //        };
        //        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
        //        {
        //            Name = $"{city}, {country}"
        //        };
        //        embed.WithFooter(footer);
        //        embed.WithAuthor(author);
        //        embed.WithCurrentTimestamp();
        //        embed.AddField(y =>
        //        {
        //            y.Name = "Coordinates";
        //            y.Value = $"Longitude: **{lon}**\nLatitude: **{lat}**";
        //            y.IsInline = true;
        //        });
        //        embed.AddField(y =>
        //        {
        //            y.Name = "Wind";
        //            y.Value = $"Speed: **{speed} m/s**\nDirection: **{direction}({deg}°)**";
        //            y.IsInline = true;
        //        });
        //        embed.AddField(y =>
        //        {
        //            y.Name = "Main";
        //            y.Value = $"Temperature: **{Math.Round(temp - 273.15)}°C/{Math.Round(temp * 9 / 5 - 459.67)}°F**\nPressure: **{pressure} hpa**\nHumidity: **{humidity}%**\nMinimum Temperature: **{Math.Round(minTemp - 273.15)}°C/{Math.Round(minTemp * 9 / 5 - 459.67)}°F**\nMaximum Temperature: **{Math.Round(maxTemp - 273.15)}°C/{Math.Round(maxTemp * 9 / 5 - 459.67)}°F**";
        //            y.IsInline = true;
        //        });
        //        embed.AddField(y =>
        //        {
        //            y.Name = "Cloudiness";
        //            y.Value = $"Clouds: **{all}%**";
        //            y.IsInline = true;
        //        });
        //        embed.AddField(y =>
        //        {
        //            y.Name = "Extra";
        //            y.Value = $"Country: **{country}**\nSunrise: **{sunriseDate.TimeOfDay.ToString().Substring(0, sunriseDate.TimeOfDay.ToString().Length - 3)}**\nSunset: **{sunsetDate.TimeOfDay.ToString().Substring(0, sunsetDate.TimeOfDay.ToString().Length - 3)}**";
        //            y.IsInline = true;
        //        });
        //        await Context.Channel.SendMessageAsync("", false, embed.Build());
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e.Message + e.StackTrace);
        //    }
        //}
    }
}
