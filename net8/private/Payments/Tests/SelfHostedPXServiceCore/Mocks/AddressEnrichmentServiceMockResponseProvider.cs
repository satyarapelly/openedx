namespace SelfHostedPXServiceCore.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class AddressEnrichmentServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.ToString().Contains("autocomplete"))
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                if (requestContent.Contains("98052"))
                {
                    responseContent = "{\"suggested_addresses\":[{\"mailability_score\":\"2\",\"result_percentage\":\"100.00\",\"address_type\":\"S\",\"address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"6051 137th Ave NE\",\"address_line2\":\"Apt 318\",\"postal_code\":\"98052\"}}, {\"mailability_score\":\"2\",\"result_percentage\":\"100.00\", \"address_type\":\"S\",\"address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"6347 137th Ave NE\",\"address_line2\":\"Apt 262\",\"postal_code\":\"98052\"}}, {\"mailability_score\":\"2\",\"result_percentage\":\"100.00\",\"address_type\":\"S\",\"address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"6355 137th Ave NE\",\"address_line2\":\"Apt 286\",\"postal_code\":\"98052\"}}],\"status\":\"None\"}";
                }
                else if (requestContent.Contains("98012"))
                {
                    responseContent = "{\"suggested_addresses\": [{\"mailability_score\":\"1\",\"result_percentage\":\"100.00\",\"address_type\":\"S\",\"address\": {\"country\":\"US\",\"region\":\"WA\",\"city\":\"Bothell\",\"postal_code\":\"98012\"}}, {\"mailability_score\":\"1\",\"result_percentage\":\"100.00\",\"address_type\":\"S\",\"address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Mill Creek\",\"postal_code\":\"98012\"}}],\"status\":\"None\"}";
                }
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

            return await Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}