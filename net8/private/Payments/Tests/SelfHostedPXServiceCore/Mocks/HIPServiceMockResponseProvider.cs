namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.Versioning;
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
                var url = httpRequestMessage.RequestUri!.ToString();

                if (url.Contains("audio", StringComparison.OrdinalIgnoreCase))
                {
                    // 1 second of silent WAV (44.1kHz, mono, 16-bit) – cross-platform
                    var wavBytes = CreateSilentWav(seconds: 1, sampleRate: 44100, channels: 1, bitDepth: 16);
                    var audio = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(wavBytes)
                    };
                    audio.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                    audio.Headers.Add("challenge-id", "85c7a465-884a-348a-a6c5-fef4fc52742b");
                    audio.Headers.Add("azureregion", "EastUS2");
                    return audio;
                }
                else
                {
                    // Image response: Windows uses System.Drawing; others use embedded JPEG
                    byte[] jpeg = OperatingSystem.IsWindows()
                        ? CreateWhiteJpeg1024_Windows()
                        : LoadEmbeddedJpeg("SelfHostedPXServiceCore.Mocks.Images.blank1024.jpg");

                    var img = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(jpeg)
                    };
                    img.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    img.Headers.Add("challenge-id", "96c7a465-995a-459a-a6c5-fef4fc52743d");
                    img.Headers.Add("azureregion", "EastUS2");
                    return img;
                }
            }
            else if (httpRequestMessage.Method == HttpMethod.Post)
            {
                var requestMessage = await httpRequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var hipCaptchaPayload = JsonConvert.DeserializeObject<HIPCaptchaUserInput>(requestMessage)!;

                var responsePayload = new HIPCaptchaValidationResults
                {
                    ChallengeId = hipCaptchaPayload.ChallengeId,
                    Solved = "True",
                    Reason = string.Empty
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(responsePayload),
                        System.Text.Encoding.UTF8,
                        "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/octet-stream")
            };
        }

        private static byte[] CreateSilentWav(int seconds, int sampleRate, int channels, int bitDepth)
        {
            int dataBytes = sampleRate * channels * (bitDepth / 8) * seconds;
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                // RIFF header
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataBytes);
                writer.Write(new[] { 'W', 'A', 'V', 'E' });

                // fmt  chunk
                writer.Write(new[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * (bitDepth / 8));
                writer.Write((short)(channels * (bitDepth / 8)));
                writer.Write((short)bitDepth);

                // data chunk
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataBytes);
                writer.Write(new byte[dataBytes]); // silence
            }
            return stream.ToArray();
        }

        private static byte[] LoadEmbeddedJpeg(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var s = asm.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

#if WINDOWS
        // Mark explicit Windows-only. Called only when OperatingSystem.IsWindows() is true.
        [SupportedOSPlatform("windows")]
        private static byte[] CreateWhiteJpeg1024_Windows()
        {
            using var bmp1 = new System.Drawing.Bitmap(1, 1);
            bmp1.SetPixel(0, 0, System.Drawing.Color.White);
            using var bmp = new System.Drawing.Bitmap(bmp1, 1024, 1024);
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
#else
        private static byte[] CreateWhiteJpeg1024_Windows()
            => throw new PlatformNotSupportedException("Windows-only method was invoked on a non-Windows build.");
#endif
    }
}