﻿namespace RuleLimiterTask
{
    public interface ICacheService
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
    }
}
