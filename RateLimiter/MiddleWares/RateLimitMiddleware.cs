using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Services.Common.Configurations;
using Services.Common.Models;
using Services.Common.RateLimiters;

namespace RateLimiter.MiddleWares;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;

    public RateLimitMiddleware(RequestDelegate next, IRateLimiter rateLimiter)
    {
        _next = next;
        _rateLimiter = rateLimiter;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var jsonToken = context.Request.Headers["Custom-Header"].ToString(); // Assuming token in headers
        var token = JsonSerializer.Deserialize<RateLimitToken>(jsonToken);
        //var region = context.Request.Headers["Region"].ToString(); // Or determine region by token lookup
        
        if (token is null || string.IsNullOrEmpty(token?.Id.ToString()))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authorization token is missing.");
            return;
        }

        token.Resource = context.Request.Path.ToString(); // Use the endpoint path as resource identifier

        if (!_rateLimiter.IsRequestAllowed(token))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context); // Continue to the next middleware or endpoint
    }   
}