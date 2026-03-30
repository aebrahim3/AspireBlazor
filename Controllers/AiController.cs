using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CmsBackend.Controllers;

// Provides AI-powered grammar and spell checking using the free LanguageTool API.
[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AiController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Request model for the grammar check endpoint.
    public class GrammarCheckRequest
    {
        public string Text { get; set; } = "";
        public string Language { get; set; } = "en-US";
    }

    // Response model for individual grammar/spelling issues.
    public class GrammarIssue
    {
        public string Message { get; set; } = "";
        public int Offset { get; set; }
        public int Length { get; set; }
        public string BadText { get; set; } = "";
        public List<string> Suggestions { get; set; } = new();
        public string RuleCategory { get; set; } = "";
    }

    // Accepts article text, sends it to LanguageTool, and returns a list of issues.
    [HttpPost("check")]
    public async Task<ActionResult<List<GrammarIssue>>> CheckGrammar([FromBody] GrammarCheckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return Ok(new List<GrammarIssue>());

        // Strip HTML tags to get plain text for grammar checking.
        var plainText = Regex.Replace(request.Text, "<[^>]+>", " ");
        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

        if (string.IsNullOrWhiteSpace(plainText))
            return Ok(new List<GrammarIssue>());

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Call the free LanguageTool public API (no API key needed).
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", plainText),
                new KeyValuePair<string, string>("language", request.Language)
            });

            var response = await client.PostAsync(
                "https://api.languagetool.org/v2/check", formContent);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502, new { error = "Grammar check service unavailable." });
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var matches = doc.RootElement.GetProperty("matches");

            var issues = new List<GrammarIssue>();

            foreach (var match in matches.EnumerateArray())
            {
                var issue = new GrammarIssue
                {
                    Message = match.GetProperty("message").GetString() ?? "",
                    Offset = match.GetProperty("offset").GetInt32(),
                    Length = match.GetProperty("length").GetInt32(),
                    RuleCategory = match.TryGetProperty("rule", out var rule)
                        && rule.TryGetProperty("category", out var cat)
                        && cat.TryGetProperty("name", out var catName)
                            ? catName.GetString() ?? ""
                            : ""
                };

                // Extract the problematic text segment.
                if (issue.Offset >= 0 && issue.Offset + issue.Length <= plainText.Length)
                {
                    issue.BadText = plainText.Substring(issue.Offset, issue.Length);
                }

                // Collect replacement suggestions from LanguageTool.
                if (match.TryGetProperty("replacements", out var replacements))
                {
                    foreach (var r in replacements.EnumerateArray().Take(3))
                    {
                        if (r.TryGetProperty("value", out var val))
                        {
                            issue.Suggestions.Add(val.GetString() ?? "");
                        }
                    }
                }

                issues.Add(issue);
            }

            return Ok(issues);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { error = "Could not reach grammar check service." });
        }
    }
}
