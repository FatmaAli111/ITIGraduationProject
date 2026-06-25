using ITIGraduationProject.Service.AI;
using Moq.Protected;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ITIGraduationProject.Test.Features.AiChatAssisstant
{

    [TestFixture]
    public class AiServiceTests
    {
        private Mock<IConfiguration> _configuration;
        private Mock<HttpMessageHandler> _handler;

        private AiService _service;

        [SetUp]
        public void Setup()
        {
            _configuration = new Mock<IConfiguration>();
            _handler = new Mock<HttpMessageHandler>();

            _configuration
                .Setup(x => x["DifyAI:ApiKey"])
                .Returns("test-api-key");

            _configuration
                .Setup(x => x["DifyAI:BaseUrl"])
                .Returns("https://fake-api.com/");

            var httpClient = new HttpClient(_handler.Object);

            _service = new AiService(
                httpClient,
                _configuration.Object);
        }

        [Test]
        public async Task AskGeminiAsync_Should_Return_Answer_When_Request_Succeeds()
        {
            var json =
                """
        {
            "data":
            {
                "outputs":
                {
                    "text":"Hello Fatma"
                }
            }
        }
        """;

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json")
                    });

            var result =
                await _service.AskGeminiAsync("Hello");

            Assert.That(
                result,
                Is.EqualTo("Hello Fatma"));
        }
        [Test]
        public async Task AskGeminiAsync_Should_Return_Error_Message_When_Api_Fails()
        {
            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });

            var result =
                await _service.AskGeminiAsync("Hello");

            Assert.That(
                result,
                Is.EqualTo(
                    "Sorry, I am having trouble connecting to the AI assistant right now."));
        }

        [Test]
        public async Task AskGeminiAsync_Should_Return_Error_Message_When_Exception_Occurs()
        {
            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception());

            var result =
                await _service.AskGeminiAsync("Hello");

            Assert.That(
                result,
                Is.EqualTo(
                    "Sorry, I am having trouble connecting to the AI assistant right now."));
        }

        [Test]
        public async Task AskGeminiAsync_Should_Return_Default_Message_When_Answer_Is_Null()
        {
            var json =
                """
                {
                }
                """;

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>
                (
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json")
                    });

            var result =
                await _service.AskGeminiAsync("Hello");

            Assert.That(
                result,
                Is.EqualTo(
                    "I could not process the response from the assistant."));
        }
    }
}
