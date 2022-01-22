using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TestWeather.API
{
    public class ApiCaller
    {
        public static async Task<ApiResponse> Get(string url, string authId = null)
        {
            // Połączenie do OpenWeatherMap API i przekazanie odpowiedzi
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(authId))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authId);

                var request = await client.GetAsync(url);
                if (request.IsSuccessStatusCode)
                {
                    return new ApiResponse { Response = await request.Content.ReadAsStringAsync() };
                }
                else
                    return new ApiResponse { ErrorMessage = request.ReasonPhrase };
            }
        }
    }


    // Parametry klasy API Caller
    public class ApiResponse
    {
        public bool Success => ErrorMessage == null;
        public string ErrorMessage { get; set; }
        public string Response { get; set; }
    }
}
