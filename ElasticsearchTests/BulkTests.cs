using System.Linq;
using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class BulkTests : ElasticSearchTestBase
    {
        [Test]
        public void Bulk()
        {
            var employees = Enumerable.Range(1, 100).Select(x => GenerateEmployee());

            _client.Bulk(bulk =>
                bulk.CreateMany(employees)
                    .Refresh()
            );

            _client.Count<Employee>().Count.Should().Be(100);
        }

        [Test]
        public void Perform_many_operations_in_one_bulk()
        {
            //  curl -XPOST localhost:9200/_bulk'

            //  { "index" : { "_index" : "test", "_type" : "type1", "_id" : "1" } }
            //  { "field1" : "value1" }
            //  { "delete" : { "_index" : "test", "_type" : "type1", "_id" : "2" } }
            //  { "create" : { "_index" : "test", "_type" : "type1", "_id" : "3" } }
            //  { "field1" : "value3" }
            //  { "update" : {"_id" : "1", "_type" : "type1", "_index" : "index1"} }
            //  { "doc" : {"field2" : "value2"} }

            var employees = Enumerable.Range(1, 10).Select(x => GenerateEmployee(x.ToString()));
            _client.Bulk(bulk => bulk
                .CreateMany(employees)
                .Refresh());

            _client.Bulk(bulk => bulk
                .CreateMany(Enumerable.Range(1, 50).Select(x => GenerateEmployee()))
                .DeleteMany<Employee>(Enumerable.Range(1, 5).Select(x => x.ToString()))
                .Refresh());

            _client.Count<Employee>().Count.Should().Be(55);
        }
    }
}
