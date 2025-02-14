using Refit;

namespace BlazorAppServer.Sample
{
    public record WeatherForcastModel(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary);
    public interface IWeatherForcastApi
    {
        [Get("/weatherforecast")]
        Task<ApiResponse<IEnumerable<WeatherForcastModel>>> GetWeatherForcast();
    }

    public class WeatherService
    {
        private readonly IWeatherForcastApi _weatherForcastApi;

        public WeatherService(IWeatherForcastApi weatherForcastApi)
        {
            _weatherForcastApi = weatherForcastApi;
        }

        public async Task<IEnumerable<WeatherForcastModel>?> GetWeatherForcast()
        {
            var response = await _weatherForcastApi.GetWeatherForcast();

            return response.Content;
        }
    }
}
