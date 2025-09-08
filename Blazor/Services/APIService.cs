using Microsoft.JSInterop;

namespace Blazor.Services
{
    public partial class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;

        public APIService(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }
    }
}
