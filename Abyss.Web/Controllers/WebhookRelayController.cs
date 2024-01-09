using Abyss.Web.Data.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Abyss.Web.Controllers;

[ApiController]
[Route("api/webhookrelay")]
public class WebhookRelayController : Controller
{
    private readonly WebhookRelayOptions _options;
    private readonly HttpClient _client;

    public WebhookRelayController(IOptions<WebhookRelayOptions> options, HttpClient client)
    {
        _options = options.Value;
        _client = client;
    }

    [HttpPost("{name}/{key}")]
    public async Task<IActionResult> Relay([FromRoute] string name, [FromRoute] string key)
    {
        var relay = _options.Relays.FirstOrDefault(x => x.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        if (relay.Equals(default(KeyValuePair<string, WebhookRelayOptions.WebhookRelay>)))
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(relay.Value.Key) || key != relay.Value.Key)
        {
            return Unauthorized();
        }

        var statusCode = HttpStatusCode.Accepted;
        var resp = string.Empty;
        foreach (var url in relay.Value.Urls)
        {
            using var req = new HttpRequestMessage(new HttpMethod(Request.Method), url);
            foreach (var header in Request.Headers.Where(x => x.Key != "Host"))
            {
                req.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable());
            };
            using var requestBodyStream = new MemoryStream();
            await Request.Body.CopyToAsync(requestBodyStream);
            Request.Body.Seek(0, SeekOrigin.Begin);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            req.Content = new StreamContent(requestBodyStream);
            if (Request.Headers.ContainsKey("Content-Type"))
            {
                req.Content.Headers.Add("Content-Type", Request.ContentType);
            }
            var res = await _client.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                statusCode = res.StatusCode;
                resp = await res.Content.ReadAsStringAsync();
            }
        }

        return StatusCode((int)statusCode, resp);
    }
}
