using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWeather.API;
using TestWeather.Models;
using Xamarin.Forms;


namespace TestWeather
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            GetWeatherInfo(); // wyświetla informacje o pogodzie
            getHistory(); // Pobranie z bazy danych 3 ostatnich wyszukań użytkowników
        }

        // Lista w której przechowamy ostatnie 3 wyszukania.
        private List<string> lastSearches = new List<string>();

        // Domyślna lokalizacja
        private string Location = "Poznan"; // Domyślna lokalizacja

        // Połączenie z Azure DB
        private string connStr = "Server=tcp:weather-serwer.database.windows.net,1433;Initial Catalog=WeatherAppDB;Persist Security Info=False;User ID=myadmin;Password=qaz12345!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // Połączenie się z bazą oraz pobranie ostatnich 3 wyszukań użytkowników
        private void getHistory()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select top 3 city_name from userSearchHistory order by id desc";

                    conn.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lastSearches.Add(reader.GetString(0));
                        }
                    }
                }
            }
            lastLabel1.Text = lastSearches[0];
            lastLabel2.Text = lastSearches[1];
            lastLabel3.Text = lastSearches[2];

        }

        // Obsługa przycisku "Check"
        private void WeatherClicked(object sender, EventArgs args)
        {
            Location = cityNameText.Text;
            GetWeatherInfo();
            SendUserSearchData(Location);
        }

        // Obsługa przycisków pod paskiem wyszukania które po wybraniu wyświetlają dane pogodowe ostatnio przeglądane przez użytkowników
        private void LastSearchesClicked(object sender, EventArgs args)
        {
            var button = (Button)sender;
            Location = button.Text;
            GetWeatherInfo();
        }

        // Pobiera dane pogodowego z zewnętrznego API
        private async void GetWeatherInfo()
        {
            string apiKey = "70986ba44fd7ea6c60bb98a8f731a9df";
            string apiBase = "https://api.openweathermap.org/data/2.5/weather?q=";
            string unit = "metric";
            string url = apiBase + Location + "&appid=" + apiKey + "&units=" + unit;

            var result = await ApiCaller.Get(url);

            if (result.Success)
            {
                try
                {
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);

                    // Formatowanie opisu stanu pogody
                    string weatherDescription = weatherInfo.weather[0].description;
                    weatherDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(weatherDescription);

                    // Dane miasta
                    string cityName = weatherInfo.name;
                    string cityCountry = weatherInfo.sys.country;

                    // Przeliczenie prędkości wiatru z m/s na km/h
                    float speed = weatherInfo.wind.speed;
                    speed = (speed * 18) / 5;
                    int new_speed = (int)Math.Round(speed);
                    string wind_speed = new_speed.ToString();

                    temperatureLabel.Text = weatherInfo.main.temp.ToString("0");
                    weatherImage.Source = $"icon{weatherInfo.weather[0].icon}";
                    weatherDescriptionLabel.Text = weatherDescription;
                    placeLabel.Text = $"{cityName}, {cityCountry}"; 
                    tempFeelsLikeLabel.Text = $"Feels like: {weatherInfo.main.feels_like.ToString("0")} °C";
                    windSpeedLabel.Text = $"Wind speed: {wind_speed} km/h.";
                    humidityLabel.Text = $"Humidity: {weatherInfo.main.humidity}%";

                }
                catch (Exception e)
                {
                    await DisplayAlert("Weather Info", "Unknown error!", "Ok");
                    cityNameText.Placeholder = "Write a city";
                    
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No weather information found", "Ok");
            }
        }

        // Zapisanie w bazie danych wyszukania użytkownika
        private void SendUserSearchData(string city_name)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO userSearchHistory (city_name) VALUES (@city_name)";
                    command.Parameters.AddWithValue("@city_name", city_name);

                    conn.Open();
                    command.ExecuteScalar();
                }
            }
        }

    }
}
