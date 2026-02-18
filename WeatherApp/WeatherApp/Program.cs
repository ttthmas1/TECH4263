using System;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;

string[] cities = { "Madrid", "Seoul", "Sydney" };


string[] usCities = { "Dallas", "Denver", "Miami", "Phoenix", "Boston" };

var httpClient = new HttpClient();

string GetWeatherDescription(int code)
{
    return code switch
    {
        0 => "Clear sky",
        1 or 2 => "Mainly clear / Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        80 or 81 or 82 => "Rain showers",
        95 => "Thunderstorm",
        _ => $"Unknown (code {code})"
    };
}

async Task PrintWeatherForCity(string city)
{
    Console.WriteLine($"Looking up: {city}...");

    try
    {
        // Step 1: Geocoding API
        string encodedCity = UrlEncoder.Default.Encode(city);

        string geoUrl =
            $"https://geocoding-api.open-meteo.com/v1/search?name={encodedCity}&count=1&language=en&format=json";

        var geoResponse = await httpClient.GetAsync(geoUrl);

        if (!geoResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"  [!] Geocoding failed: HTTP {(int)geoResponse.StatusCode}");
            Console.WriteLine();
            return;
        }

        string geoJson = await geoResponse.Content.ReadAsStringAsync();

        using var geoDoc = JsonDocument.Parse(geoJson);
        var root = geoDoc.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
        {
            Console.WriteLine("  [!] No results found (check spelling).");
            Console.WriteLine();
            return;
        }

        var location = results[0];

        string foundName = location.GetProperty("name").GetString() ?? city;
        string country = location.GetProperty("country").GetString() ?? "Unknown";
        double lat = location.GetProperty("latitude").GetDouble();
        double lon = location.GetProperty("longitude").GetDouble();

        Console.WriteLine($"  Found   : {foundName}, {country} ({lat:0.00}°, {lon:0.00}°)");

        // Step 2: Weather API
        string weatherUrl =
            $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

        var weatherResponse = await httpClient.GetAsync(weatherUrl);

        if (!weatherResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"  [!] Weather failed: HTTP {(int)weatherResponse.StatusCode}");
            Console.WriteLine();
            return;
        }

        string weatherJson = await weatherResponse.Content.ReadAsStringAsync();

        using var weatherDoc = JsonDocument.Parse(weatherJson);
        var weatherRoot = weatherDoc.RootElement;

        var current = weatherRoot.GetProperty("current_weather");

        double temp = current.GetProperty("temperature").GetDouble();
        double wind = current.GetProperty("windspeed").GetDouble();
        int code = current.GetProperty("weathercode").GetInt32();

        string desc = GetWeatherDescription(code);

        Console.WriteLine($"  Weather : Temp {temp:0.0}°C | Wind {wind:0.0} km/h | {desc} (code {code})");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [!] Error: {ex.Message}");
        Console.WriteLine();
    }
}

Console.WriteLine("========================================");
Console.WriteLine("         CITY WEATHER LOOKUP");
Console.WriteLine("========================================");
Console.WriteLine();

foreach (var city in cities)
{
    await PrintWeatherForCity(city);
}

Console.WriteLine("========================================");
Console.WriteLine("           US CITY WEATHER");
Console.WriteLine("========================================");
Console.WriteLine();

foreach (var city in usCities)
{
    await PrintWeatherForCity(city);
}

Console.WriteLine("========================================");
Console.WriteLine("Done!");
