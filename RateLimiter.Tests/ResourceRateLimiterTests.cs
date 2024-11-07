using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace RateLimiter.Tests;

[TestFixture]
public class ResourceRateLimiterTests
{
    // Note: This test uses the sample JSON config file to demonstrate loading the configuration from a file.
    // It is technically not a unit test but has been added for demonstration purposes.
    [Test]
    public void CreateResourceRateLimiters_FromJsonConfig_CreatesLimiters()
    {
        // Load the configuration from the JSON file
        string jsonConfig;
        try
        {
            jsonConfig = File.ReadAllText("../../../../SampleResourceRateLimiterConfig.json");
        }
        catch (FileNotFoundException ex)
        {
            Assert.Fail($"Configuration file not found: {ex.Message}");
            return;
        }

        List<ResourceLimitConfig> resourceLimitConfigs;
        try
        {
            using (JsonDocument document = JsonDocument.Parse(jsonConfig))
            {
                JsonElement root = document.RootElement;
                JsonElement resourceLimitConfigsElement = root.GetProperty("ResourceLimitConfig");
                resourceLimitConfigs = JsonSerializer.Deserialize<List<ResourceLimitConfig>>(resourceLimitConfigsElement.GetRawText());
            }
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Failed to deserialize JSON: {ex.Message}");
            return;
        }

        // Ensure deserialization was successful
        Assert.IsNotNull(resourceLimitConfigs, "Deserialized configuration should not be null");
        Assert.IsNotEmpty(resourceLimitConfigs, "Deserialized configuration should not be empty");

        // Create the ResourceRateLimiter instance
        var resourceRateLimiter = new ResourceRateLimiter();

        // Create the rate limiters based on the configuration
        var limiters = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert that the limiters were created successfully
        Assert.IsNotNull(limiters, "Limiters should not be null");
        Assert.IsNotEmpty(limiters, "Limiters should not be empty");

        // Additional assertions can be added here to verify the specific properties of the created limiters
    }


    [Test]
    public void CreateResourceRateLimiters_SingleLimiter_CreatesSingleLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Resource1", result[0].Resource);
    }

    [Test]
    public void CreateResourceRateLimiters_MultipleLimiters_CreatesChainedLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    },
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.FixedWindowRateLimiter,
                        PermitLimit = 20,
                        WindowMinutes = 1,
                        QueueLimit = 10
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Resource1", result[0].Resource);

        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(2, limiters.Length);
    }

    [Test]
    public void CreateResourceRateLimiters_NoLimiters_ThrowsException()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>()
            }
        };

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs));
    }

    [Test]
    public void CreateResourceRateLimiters_NullResourceName_ThrowsException()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = null, // Resource name not specified
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs));
        Assert.AreEqual("Resource name must be specified.", ex.Message);
    }

    [Test]
    public void CreateResourceRateLimiters_ResourceNameNotSpecified_ThrowsException()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = string.Empty, // Resource name not specified
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs));
        Assert.AreEqual("Resource name must be specified.", ex.Message);
    }

    [Test]
    public void CreateResourceRateLimiters_DuplicateResourceNames_ThrowsException()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            },
            new ResourceLimitConfig
            {
                Name = "Resource1", // Duplicate resource name
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.FixedWindowRateLimiter,
                        PermitLimit = 20,
                        WindowMinutes = 1,
                        QueueLimit = 10
                    }
                }
            }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs));
        Assert.AreEqual("Duplicate resource names are not allowed.", ex.Message);
    }

    [Test]
    public void CreateResourceRateLimiters_TwoResources_CreatesBothResources()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            },
            new ResourceLimitConfig
            {
                Name = "Resource2",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.FixedWindowRateLimiter,
                        PermitLimit = 20,
                        WindowMinutes = 1,
                        QueueLimit = 10
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Resource1", result[0].Resource);
        Assert.AreEqual("Resource2", result[1].Resource);
    }

    [Test]
    public void CreateResourceRateLimiters_ConcurrencyLimiter_CreatesConcurrencyLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.ConcurrencyLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].RateLimiter);
        Assert.AreEqual("Resource1", result[0].Resource);

        // Check the number of limiters (should be 1).
        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(1, limiters.Length);
    }

    [Test]
    public void CreateResourceRateLimiters_FixedWindowRateLimiter_CreatesFixedWindowRateLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.FixedWindowRateLimiter
    ,
                        PermitLimit = 20,
                        WindowMinutes = 1,
                        QueueLimit = 10
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].RateLimiter);
        Assert.AreEqual("Resource1", result[0].Resource);

        // Check the number of limiters (should be 1).
        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(1, limiters.Length);
    }

    [Test]
    public void CreateResourceRateLimiters_RandomRateLimiter_CreatesRandomRateLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.RandomRateLimiter,
                        PermitLimit = 10,
                        QueueLimit = 5
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].RateLimiter);
        Assert.AreEqual("Resource1", result[0].Resource);

        // Check the number of limiters (should be 1).
        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(1, limiters.Length);
    }

    [Test]
    public void CreateResourceRateLimiters_SlidingWindowRateLimiter_CreatesSlidingWindowRateLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.SlidingWindowRateLimiter,
                        PermitLimit = 30,
                        WindowMinutes = 1,
                        SlidingWindowSegments = 2,
                        QueueLimit = 15
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].RateLimiter);
        Assert.AreEqual("Resource1", result[0].Resource);

        // Check the number of limiters (should be 1).
        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(1, limiters.Length);
    }

    [Test]
    public void CreateResourceRateLimiters_TokenBucketRateLimiter_CreatesTokenBucketRateLimiter()
    {
        // Arrange
        var resourceRateLimiter = new ResourceRateLimiter();
        var resourceLimitConfigs = new List<ResourceLimitConfig>
        {
            new ResourceLimitConfig
            {
                Name = "Resource1",
                Limiters = new List<RateLimiterConfig>
                {
                    new RateLimiterConfig
                    {
                        LimiterType = ResourceRateLimiter.RateLimiterType.TokenBucketRateLimiter,
                        PermitLimit = 40,
                        WindowMinutes = 1,
                        TokensPerPeriod = 10,
                        QueueLimit = 20
                    }
                }
            }
        };

        // Act
        var result = resourceRateLimiter.CreateResourceRateLimiters(resourceLimitConfigs);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].RateLimiter);
        Assert.AreEqual("Resource1", result[0].Resource);

        // Check the number of limiters (should be 1).
        var rateLimiter = result[0].RateLimiter;
        var limitersField = rateLimiter.GetType().GetField("_limiters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var limiters = (PartitionedRateLimiter<string>[])limitersField.GetValue(rateLimiter);

        Assert.AreEqual(1, limiters.Length);
    }
}
