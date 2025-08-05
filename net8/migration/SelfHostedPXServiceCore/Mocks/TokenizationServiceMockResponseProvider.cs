namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class TokenizationServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage)
        {
            // Token expiration date is set to 12/31/2099.
            string responseContent = "{\"keys\":[{\"kid\":\"epan-20240117\",\"kty\":\"RSA\",\"key_ops\":[\"decrypt\",\"encrypt\"],\"n\":\"o95rs_bzk_jRalI0onqcK_qD6p3_nzJBNDDvhl8Ca5dW5DMguqYmismTURWDVj5usxL4kFksOoUJX1sZlGjedZ8-oFBAN71ge5avAqJGyzGlnT9uJptkqsWdVEKRC3gQP57wRV6iIsp9BWQ7iIoEOOSnk4Tjf5qj7xVDSckGDdLbiSqrndpss20kuGQgW0shj9-4HfaTTVo-ZfKReaahYo_pK9nTJGgPLvL0IgwlNZ3PUklhbSycOahx-mFaBoVgLoWUh4TdmBzkPHHsJrqP-14RQD24wsoxbZXMc3qbTVsmb0ImvTl5YIMHvAZb7anA24uzitoIt_td-ZNS-bWnPweW5qPmrX3qEJh9fZZZf8CgB-R2k4TCefJf16O7e9C7PAd-_4sHMc8ZxG00qeKYavMSE-71iY-BVAgINwKKP6LVhfOOnyz-2GjxEoRX-QqCl-8vZmxjOPzD6MZIoRiQdJ_MQFBLATXLqL8Q_DgDiAnNvx9TzHZPZctNbVyqjypV\",\"e\":\"AQAB\",\"iat\":1705539114,\"exp\":4102387200,\"nbf\":1705363200,\"use\":\"enc\",\"alg\":\"RSA-OAEP-256\"},{\"kid\":\"epan-20240108\",\"kty\":\"RSA\",\"key_ops\":[\"decrypt\",\"encrypt\"],\"n\":\"q-7vx8gBw3T4key9m8yz1IiztVPQR_Zb8XLOsSFuPoxCH0i6KNsEy47-kgCFNXfEWpCeeIEBIs7YOnpzKohowbgTnS8Wj6RT5QK_KqThYC93IrrKWmgoit307ivq6yvn0FSnE3ryfHBf5zVZIYa5eDVGUxpHjfq9EuvwvIeuMcOBxS3q8SWEi5GBGKT7Uf47vLRqTIQROgiw680HKERNj1XgHPdpWBH5pBy541QFrKlK6E-2wRvOcJEfkzgE2Nq5XtOIPddHoxlGNhsW2a55v-dwvDJZrWPc75lRIyXFIFYdQLJhW3eWEdo9p5bXnTBmr4MGVrURoV7yirU4cS9nSnNxjgPuN6-lyMuYpRZmH0Dg3nLyExWY7k1DpVOORJOi-6KqllsO_UP-k9O7U219HPjJMZe3MofN7bhcMEyhF9go6bwCCcwWxDTrlB1G20a1GaD4HZF9vJ57K7AGJthO3455ZPUHM9Ngj7dFo2wwYdvlmPp4UDDIiY-eCdXZpcp5\",\"e\":\"AQAB\",\"iat\":1704935775,\"exp\":1736553600,\"nbf\":1704931200,\"use\":\"enc\",\"alg\":\"RSA-OAEP-256\"}]}";
            HttpStatusCode statusCode = HttpStatusCode.OK;

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
