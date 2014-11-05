using System;
using System.Runtime.InteropServices;
using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using ElasticsearchTests.Models;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class MoreLikeThisTests : ElasticSearchTestBase
    {
//        [Test]
//        public void MoreLikeThis()
//        {
//            var config = new ConnectionConfiguration(new Uri("http://localhost:9200"));
//            var client = new ElasticsearchClient(config);
//
//            client.DoRequest<MoreLikeThisQuery>()
//           
//            var result = _client.MoreLikeThis<Employee>(mlt => mlt
//                .MinDocFreq(1)
//                .Id("1")
//                
//                );
//        }

        [Test]
        public void Test()
        {
            var employee = new Employee
            {
                About = @"The more_like_this_field query is the same as the more_like_this query, except it runs against a single field. It provides nicer query DSL over the generic more_like_this query, and support typed fields query (automatically wraps typed fields with type filter to match only on the specific type)."
            };
            _client.Map<Employee>(mapping => mapping);
            _client.Index(employee, i => i.Refresh());

            var result = _client.Suggest<Employee>(suggest => suggest
                .GlobalText("fiele")
                .Term("test", term => term.OnField("about")));
            result.Suggestions.Count.Should().BeGreaterThan(0);
        }
    }
}
