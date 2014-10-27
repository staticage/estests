using System.Collections.Generic;
using System.Linq;
using ElasticsearchTests.Models;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class SearchTests : ElasticSearchTestBase
    {
        [Test]
        public void SimpleSearch()
        {
            var allResult = _client.Search<Employee>(s => s);
            allResult.Total.Should().Be(3);


            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Query(q => q.Term(t => t.LastName, "smith"))
            );
            result.Total.Should().Be(2);
        }

        [Test]
        public void Sort()
        {
            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .SortAscending(x => x.FirstName)
            );

            result.Documents.ToList().Should().BeInAscendingOrder(x => x.FirstName);
        }

        [Test]
        public void SearchByRange()
        {
            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Size(100)
                .Query(q => q
                    .Match(x => x.OnField(e => e.LastName).Query("Smith")))
                    .Filter(f => f.Range(r => r.OnField(e => e.Age).Greater(30)))
            );

            result.Total.Should().Be(1);
        }

        [Test]
        public void SearchByKeyword()
        {
            //curl -XGET http://localhost:9200/shinetech/employee/_search?pretty -d '
            //{
            //    "query" : {
            //        "match_phrase" : {
            //            "about" : "rock climbing"
            //        }
            //    }
            //}'

            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Size(100)
                .Query(q => q
                    .Match(x => x.OnField(e => e.About).Query("rock climbing")))
            ).Documents.ToList();

            result[0].Id.Should().Be("1");
            result[1].Id.Should().Be("2");
        }

        [Test]
        public void SearchByPharse()
        {
            //curl -XGET http://localhost:9200/shinetech/employee/_search?pretty -d '
            //{
            //    "query" : {
            //        "match_phrase" : {
            //            "about" : "rock climbing"
            //        }
            //    }
            //}'

            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Size(100)
                .Query(q => q
                    .MatchPhrase(x => x.OnField(e => e.About).Query("rock climbing")))
            ).Documents.ToList();

            result.Count.Should().Be(1);
            result[0].Id.Should().Be("1");
        }

        [Test]
        public void Highlight()
        {
            //curl -XGET http://localhost:9200/shinetech/employee/_search?pretty -d '
            //{
            //    "query" : {
            //        "match" : {
            //            "about" : "rock climbing"
            //        }
            //    },
            //    "highlight": {
            //        "fields" : {
            //            "about" : {}
            //        }
            //    }
            //}'

            var highlights = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Size(100)
                .Query(q => q
                    .Match(x => x.OnField(e => e.About).Query("rock climbing")))
                .Highlight(h => h.OnFields(
                    f => f.OnField(e => e.About)))
            ).Highlights;

            highlights["1"]["about"].Highlights.First().Should().Be("I love to go <em>rock</em> <em>climbing</em>");
            highlights["2"]["about"].Highlights.First().Should().Be("I like to collect <em>rock</em> albums");
        }

        [Test]
        public void Aggregations()
        {
            //curl -XGET http://localhost:9200/shinetech/employee/_search?pretty -d '
            //{
            //    "aggs": {
            //    "all_interests": {
            //      "terms": { "field": "interests" }
            //    }
            //  }
            //}'

            var result = _client.Search<Employee>(s => s
                .AllIndices()
                .AllTypes()
                .Aggregations(a => a.Terms("all_interests", x => x.Field(e => e.Interests)))
            );

            var items = result.Aggs.Terms("all_interests").Items;

            items.First(x => x.Key == "music").DocCount.Should().Be(2);
            items.First(x => x.Key == "sports").DocCount.Should().Be(1);
            items.First(x => x.Key == "movie").DocCount.Should().Be(1);
        }

        [Test]
        public void MultipleAggregations()
        {
            //curl -XGET http://localhost:9200/shinetech/employee/_search?pretty -d '
            //{
            //    "aggs" : {
            //        "all_interests" : {
            //            "terms" : { "field" : "interests" },
            //            "aggs" : {
            //                "avg_age" : {
            //                    "avg" : { "field" : "age" }
            //                }
            //            }
            //        }
            //    }
            //}'

            var request = new SearchRequest
            {
                Aggregations = new Dictionary<string, IAggregationContainer>()
                {
                    {"all_interests",new AggregationContainer{
                            Terms = new TermsAggregator
                            {
                                Field = new PropertyPathMarker{Name = "interests"}
                            },
                            Aggregations = new Dictionary<string, IAggregationContainer>
                            {
                                {"avg_age",new AggregationContainer{
                                    Average = new AverageAggregator{Field = new PropertyPathMarker{Name = "age"}}
                                }}
                            }
                        }
                    }
                }
            };

            var result = _client.Search<Employee>(request);

            var items = result.Aggs.Terms("all_interests").Items;

            var music = items.First(x => x.Key == "music");
            music.DocCount.Should().Be(2);
            new AggregationsHelper(music.Aggregations).Average("avg_age").Value.Should().Be(28.5);

            var sports = items.First(x => x.Key == "sports");
            sports.DocCount.Should().Be(1);
            new AggregationsHelper(sports.Aggregations).Average("avg_age").Value.Should().Be(25);

            var forestry = items.First(x => x.Key == "movie");
            forestry.DocCount.Should().Be(1);
            new AggregationsHelper(forestry.Aggregations).Average("avg_age").Value.Should().Be(35);
        }

        [SetUp]
        public void IndexEmployees()
        {
            var employees = new[]
            {
                new Employee
                {
                    Id = "1",
                    FirstName = "John",
                    LastName = "Smith",
                    Age = 25,
                    About = "I love to go rock climbing",
                    Interests = new []{"sports", "music" }
                },
                new Employee
                {
                    Id = "2",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Age = 32,
                    About = "I like to collect rock albums",
                    Interests = new []{ "music" }
                },
                new Employee
                {
                    Id = "3",
                    FirstName = "Douglas",
                    LastName = "Fir",
                    Age = 35,
                    About = "I like to build cabinets",
                    Interests = new []{"movie"}
                },
            };

            foreach (var employee in employees)
            {
                _client.Index(employee);
            }
            _client.Refresh(i => i.Index<Employee>());
        }
    }
}
