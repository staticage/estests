using System.Linq;
using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class GetTests : ElasticSearchTestBase
    {
        [Test]
        public void Get()
        {
            //      curl localhost:9200/shinetech/employee/1

            var employee = GenerateEmployee(id: "1");
            _client.Index(employee, i => i.Refresh());

            var result = _client.Get<Employee>(request => request.Id(employee.Id)).Source;

            result.Id.Should().Be(employee.Id);
        }

        [Test]
        public void Multiple_Get()
        {
            //      curl 'localhost:9200/_mget' -d '{
            //          "docs" : [
            //              {
            //                  "_index" : "shinetech",
            //                  "_type" : "employee",
            //                  "_id" : "1"
            //              },
            //              {
            //                  "_index" : "shinetech",
            //                  "_type" : "employee",
            //                  "_id" : "2"
            //              }
            //          ]
            //      }'

            //      curl 'localhost:9200/shinetech/_mget' -d '{
            //          "docs" : [
            //              {
            //                  "_type" : "employee",
            //                  "_id" : "1"
            //              },
            //              {
            //                  "_type" : "employee",
            //                  "_id" : "2"
            //              }
            //          ]
            //      }'

            //      curl 'localhost:9200/shinetech/employee/_mget' -d '{
            //          "docs" : [
            //              {
            //                  "_id" : "1"
            //              },
            //              {
            //                  "_id" : "2"
            //              }
            //          ]
            //      }'

            var ids = Enumerable.Range(1, 20).Select(x => x.ToString());
            _client.Bulk(bulk => bulk.CreateMany(ids.Select(GenerateEmployee)));

            var result = _client.MultiGet(multiGet => multiGet.GetMany<Employee>(ids));

            result.Documents.Count().Should().Be(20);
        }
    }
}
