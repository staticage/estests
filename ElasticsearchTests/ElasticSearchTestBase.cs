﻿using System;
using ElasticsearchTests.Models;
using Faker;
using Nest;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class ElasticSearchTestBase
    {
        protected ElasticClient _client;

        [SetUp]
        public void Setup()
        {
            SetupClient();
        }

        protected void SetupClient()
        {
            var node = new Uri("http://localhost:9200");

            var settings = new ConnectionSettings(uri: node, defaultIndex: "shinetech");
            _client = new ElasticClient(settings);
            _client.DeleteIndex(x => x.Index("shinetech"));
            _client.CreateIndex("shinetech");
        }

        protected Employee GenerateEmployee()
        {
            return GenerateEmployee(Guid.NewGuid().ToString());
        }

        protected Employee GenerateEmployee(string id)
        {
            return new Employee
            {
                Id = id,
                FirstName = Name.First(),
                LastName = Name.Last(),
                Email = Internet.Email(),
                Address = Address.StreetAddress(true),
                Age = RandomNumber.Next(18,60),
                About = Lorem.Sentence()
            };
        }
    }
}