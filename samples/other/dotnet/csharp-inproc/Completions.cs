// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenAI;
using Microsoft.Extensions.Logging;

namespace CSharpInProcSamples;

/// <summary>
/// These samples show how to use the OpenAI Completions APIs. For more details on the Completions APIs, see
/// https://platform.openai.com/docs/guides/completion.
/// </summary>
public static class Completions
{
    /// <summary>
    /// This sample demonstrates the "templating" pattern, where the function takes a parameter
    /// and embeds it into a text prompt, which is then sent to the OpenAI completions API.
    /// </summary>
    [FunctionName(nameof(WhoIs))]
    public static string WhoIs(
        [HttpTrigger(AuthorizationLevel.Function, Route = "whois/{name}")] HttpRequest req,
        [TextCompletion("Who is {name}?")] Response<Azure.AI.OpenAI.Completions> response)
    {
        return response.Value.Choices[0].Text;
    }

    /// <summary>
    /// This sample takes a prompt as input, sends it directly to the OpenAI completions API, and results the 
    /// response as the output.
    /// </summary>
    [FunctionName(nameof(GenericCompletion))]
    public static IActionResult GenericCompletion(
        [HttpTrigger(AuthorizationLevel.Function, "post")] PromptPayload payload,
        [TextCompletion("{Prompt}", Model = "gpt-3.5-turbo-instruct")] Response<Azure.AI.OpenAI.Completions> response,
        ILogger log)
    {
        if (response.GetRawResponse().Status != (int)HttpStatusCode.OK)
        {
            log.LogError("Error invoking OpenAI completions API: {status}", response.GetRawResponse().Status);
            return new ObjectResult(response.GetRawResponse().Content) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
        log.LogInformation("Prompt = {prompt}, Response = {response}", payload.Prompt, response);
        string text = response.Value.Choices[0].Text;
        return new OkObjectResult(text);
    }

    public record PromptPayload(string Prompt);
}
