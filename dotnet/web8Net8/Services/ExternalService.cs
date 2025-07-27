using System.Net.Http;
using System.Threading.Tasks;

namespace PXServiceNet8.Services
{
    public class ExternalService
    {
        private readonly HttpClient _httpClient;

        public ExternalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetStatusAsync()
        {
            var response = await _httpClient.GetAsync("/status");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
