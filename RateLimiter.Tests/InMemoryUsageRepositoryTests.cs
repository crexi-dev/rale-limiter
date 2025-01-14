using System;
using NUnit.Framework;
using RateLimiter.Interfaces;

namespace RateLimiter.Tests
{
    public class InMemoryUsageRepositoryTests
    {
        /**
         * this tests for the InMemoryUsageRepository to ensure default usage for new clients
         * and to verify that updated usage values are retrieved correctly.
        **/

        [Test]
        public void Should_ReturnDefaultUsage_When_NoPreviousRecordExists()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var clientToken = "crexi-nonexistent";

            var usage = usageRepo.GetUsageForClient(clientToken);

            Assert.IsNotNull(usage, "usage object should be returned even if no record of one exists.");
            Assert.AreEqual(0, usage.RequestCount, "request count should default to 0 for new clients.");
            Assert.AreEqual(DateTime.MinValue, usage.WindowStart, "WindowStart could default to DateTime.MinValue (or similar) to indicate no prior usage.");
        }

        [Test]
        public void Should_UpdateUsage_AndRetrieveItCorrectly()
        {
            IUsageRepository usageRepo = new InMemoryUsageRepository();
            var clientToken = "crexi-client123";

            // set up some initial values 
            var usageToStore = new RequestUsage
            {
                RequestCount = 5,
                WindowStart = DateTime.UtcNow
            };

            usageRepo.UpdateUsageForClient(clientToken, usageToStore);
            var retrieveUsage = usageRepo.GetUsageForClient(clientToken);

            Assert.AreEqual(usageToStore.RequestCount, retrieveUsage.RequestCount, "RequestCount should match the updated value.");
            Assert.AreEqual(usageToStore.WindowStart, retrieveUsage.WindowStart, "WindowStart should match the updated value.");
        }
    }
}
