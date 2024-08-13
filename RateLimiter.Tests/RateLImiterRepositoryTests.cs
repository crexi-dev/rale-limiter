using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Model;
using RateLimiter.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterDataStoreTest
    {
        private RateLimiterDataStore _dataStore;
        private Mock<ILogger<RateLimiterDataStore>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<RateLimiterDataStore>>();
            _dataStore = new RateLimiterDataStore();
        }

        [Test]
        public void Update_Existing_Request_Should_Update_AccessTime_And_Return_Current_Request()
        {            
            var requestDTO = new RequestDTO
            {
                CallId = "123",
                CurrentTime = DateTime.Now
            };
            var existingRequest = new Request
            {
                CallId = "123",
                AccessTime = new List<DateTime> { DateTime.Now.AddMinutes(-5) }
            };
            _dataStore.Update(requestDTO);
            var result = _dataStore.Update(requestDTO);
            Assert.AreEqual(requestDTO.CallId, result.CallId);
            Assert.AreEqual(2, result.AccessTime.Count);
            Assert.Contains(requestDTO.CurrentTime, result.AccessTime);
        }

        [Test]
        public void Update_New_Request_Should_Add_Request_To_DataStore_And_Return_New_Request()
        {           
            var requestDTO = new RequestDTO
            {
                CallId = "123",
                CurrentTime = DateTime.Now
            };           
            var result = _dataStore.Update(requestDTO);           
            Assert.AreEqual(requestDTO.CallId, result.CallId);
            Assert.AreEqual(1, result.AccessTime.Count);
            Assert.Contains(requestDTO.CurrentTime, result.AccessTime);
        }

        [Test]
        public void Get_Existing_Request_Should_Return_Current_Request()
        {
            var currentTime = DateTime.Now;
            var requestDTO = new RequestDTO
            {
                CallId = "123",
                CurrentTime = currentTime
            };
            var existingRequest = new Request
            {
                CallId = "123",
                CurrentTime = currentTime,
                AccessTime = new List<DateTime> { currentTime }
            };
            _dataStore.Update(existingRequest);           
            var result = _dataStore.Get(requestDTO);           
            Assert.AreEqual(requestDTO.CallId, result.CallId);
            Assert.AreEqual(requestDTO.CurrentTime, result.AccessTime.First());
        }

        [Test]
        public void Get_New_Request_Should_Add_Request_To_DataStore_And_Return_New_Request()
        {           
            var requestDTO = new RequestDTO
            {
                CallId = "123",
                CurrentTime = DateTime.Now
            }; 
            var result = _dataStore.Get(requestDTO);
            Assert.AreEqual(requestDTO.CallId, result.CallId);
            Assert.AreEqual(1, result.AccessTime.Count);
            Assert.Contains(requestDTO.CurrentTime, result.AccessTime);
        }
    }
}