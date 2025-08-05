namespace SelfHostedPXServiceCore.Mocks
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Accessors.Captcha;
    using Microsoft.Commerce.Payments.PXService.Model.HIPService;
    using Newtonsoft.Json;
    using Test.Common;

    public class HIPServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Method == HttpMethod.Get)
            {
                HttpResponseMessage response;
                string url = httpRequestMessage.RequestUri.ToString();
                if (url.Contains("audio"))
                {
                    // generate a sample audio wav file
                    // HIP service actually sends an mp3 file but to genrate a sample mp3 file some external library is needed, so I am generating a wav file instead
                    int sampleRate = 44100;
                    int numChannels = 1;
                    int bitDepth = 16;
                    int durationSeconds = 1;
                    int numSamples = sampleRate * numChannels * bitDepth / 8 * durationSeconds;

                    byte[] data = new byte[numSamples];

                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            // Write WAV header
                            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                            writer.Write(36 + numSamples);
                            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                            writer.Write(new char[] { 'f', 'm', 't', ' ' });
                            writer.Write(16);
                            writer.Write((short)1); // PCM format
                            writer.Write((short)numChannels);
                            writer.Write(sampleRate);
                            writer.Write(sampleRate * numChannels * bitDepth / 8);
                            writer.Write((short)(numChannels * bitDepth / 8));
                            writer.Write((short)bitDepth);
                            writer.Write(new char[] { 'd', 'a', 't', 'a' });
                            writer.Write(numSamples);

                            // Write data
                            writer.Write(data);
                        }

                        response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Content = new ByteArrayContent(stream.ToArray());
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                        response.Headers.Add("challenge-id", "85c7a465-884a-348a-a6c5-fef4fc52742b");
                        response.Headers.Add("azureregion", "EastUS2");
                    }
                }
                else
                {
                    Bitmap emptyBitmap = new Bitmap(1, 1);
                    emptyBitmap.SetPixel(0, 0, Color.White);
                    Bitmap fakeBitmap = new Bitmap(emptyBitmap, 1024, 1024);
                    MemoryStream memoryStream = new MemoryStream();
                    Image image = fakeBitmap;
                    image.Save(memoryStream, ImageFormat.Jpeg);
                    response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(memoryStream.ToArray());
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    response.Headers.Add("challenge-id", "96c7a465-995a-459a-a6c5-fef4fc52743d");
                    response.Headers.Add("azureregion", "EastUS2");
                }

                return await Task.FromResult(response);
            }
            else if (httpRequestMessage.Method == HttpMethod.Post)
            {
                string requestMessage = await httpRequestMessage.Content.ReadAsStringAsync();
                HIPCaptchaUserInput hipCaptchaPayload = JsonConvert.DeserializeObject<HIPCaptchaUserInput>(requestMessage);

                HIPCaptchaValidationResults response = new HIPCaptchaValidationResults()
                {
                    ChallengeId = hipCaptchaPayload.ChallengeId,
                    Solved = "True",
                    Reason = string.Empty
                };

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    JsonConvert.SerializeObject(response),
                    System.Text.Encoding.UTF8,
                    "application/json")
                });
            }

            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    string.Empty,
                    System.Text.Encoding.UTF8,
                    "application/octet-stream")
            });
        }
    }
}