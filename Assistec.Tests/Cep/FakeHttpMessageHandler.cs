// AssisTec.Tests/Cep/FakeHttpMessageHandler.cs
using System.Net;

namespace AssisTec.Tests.Cep;

// Substitui o transporte real do HttpClient por uma função configurável.
// Não usa NSubstitute porque HttpMessageHandler.SendAsync é protected —
// mockar um método protected exige herança de qualquer forma, então um
// handler fake dedicado é mais direto que forçar NSubstitute nesse caso.
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(responder(request));

    public static HttpResponseMessage RespostaJson(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
    };
}