using System;
using System.Linq;
using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class HightlightTests : ElasticSearchTestBase
    {

        [Test]
        public void Hightlight()
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "This is some summary",
                About = "This is some summary"
            };

            _client.Index(employee, i => i.Refresh());

            var result = _client.Search<Employee>(s => s
                .Query(q => q
                    .QueryString(qs => qs
                        .Query("summary")
                    )
                )
                .Highlight(h => h
                    .PreTags("<b>")
                    .PostTags("</b>")
                    .OnFields(
                        f => f.OnField(e => e.About)
                              .PreTags("<em>")
                              .PostTags("</em>"),

                        f => f.OnField(e => e.FirstName)
                    )
                )
            );
            var id = result.Documents.First().Id;
            result.Highlights[id]["firstName"].Highlights.First().Should().Be("This is some <b>summary</b>");
            result.Highlights[id]["about"].Highlights.First().Should().Be("This is some <em>summary</em>");
        }
    }
}
