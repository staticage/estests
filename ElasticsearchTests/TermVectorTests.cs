using System.Linq;
using ElasticsearchTests.Models;
using Nest;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class TermVectorTests : ElasticSearchTestBase
    {
        [Test]
        public void TermVector()
        {
            //  curl -XGET 'http://localhost:9200/shinetech/employee/1/_termvector?pretty=true'

            //  curl -s -XPUT 'http://shinetech/employee/' -d '{
            //      "mappings": {
            //          "tweet": {
            //              "properties": {
            //                  "about": {
            //                      "type": "string",
            //                      "term_vector": "with_positions_offsets_payloads"
            //                  }
            //              }
            //          }
            //      } 
            //  }'

            //  curl -XPUT 'http://localhost:9200/shinetech/employee/1?pretty=true' -d '{
            //    "about" : "test test test",
            //  }'


            _client.Map<Employee>(mapping => mapping
                .Properties(properties => properties
                    .String(field => field
                        .Name(e => e.About)
                        .TermVector(TermVectorOption.WithPositionsOffsetsPayloads))
                ));



            var employees = Enumerable.Range(1, 10).Select(x =>
                new Employee
                {
                    Id = x.ToString(),
                    About = "test test test"

                });

            _client.Bulk(bulk => bulk
                .CreateMany(employees)
                .Refresh());

            var result = _client.TermVector<Employee>(termVector => termVector
                .Id("1")
                .Fields(e => e.About)
                .TermStatistics());
        }
    }
}
