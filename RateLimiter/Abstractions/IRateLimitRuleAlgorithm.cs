﻿using RateLimiter.Enums;

namespace RateLimiter.Abstractions;

public interface IRateLimitRuleAlgorithm
{
    string Name { get; set; }

    bool IsAllowed(string discriminator);

    RateLimitingAlgorithm Algorithm { get; set; }
}