using System;
using RateLimiter.Enums;

namespace RateLimiter.Models;

public record Request(Guid Id, RegionType RegionType, DateTime DateTime);