﻿using System;
using Xunit;
using Moq;
using UpDiddyApi.Business;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;

namespace API.Tests
{
    public class LinkedInTests
    {
        [Fact]
        public void AcquireBearerToken_InvalidSubscriberGuidProvided()
        {            
            /* todo: we need to write unit tests that cover functionality within the API, but this presents the following challenges
             * - how to mock the UpDiddyDbContext? use shims, Moq, something else?
             * - how to mock 3rd party dependencies in our code (e.g. LinkedIn)?
             * - do we need to rewrite our classes/methods so that unit tests can be written for it?
             */ 
            var mockSubscriberDbSet = new Mock<DbSet<Subscriber>>();
            var mockContext = new Mock<UpDiddyDbContext>();
            mockContext.Setup(m => m.Subscriber).Returns(mockSubscriberDbSet.Object);

            Assert.True(true);
        }
    }
}
