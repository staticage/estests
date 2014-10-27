using System.Linq;
using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class IndexingTests : ElasticSearchTestBase
    {
        [Test]
        public void DefaultIndexingIsNotRealtime()
        {
            _client.Index(GenerateEmployee());

            Assert.AreEqual(_client.Count<Employee>().Count, 0);
        }

        [Test]
        public void RealtimeIndexing()
        {
            _client.Index(GenerateEmployee());
            _client.Refresh(x => x.Index<Employee>());

            Assert.AreEqual(_client.Count<Employee>().Count, 1);
        }

        [Test]
        public void RealtimeIndexing2()
        {
            _client.Index(GenerateEmployee(), i => i.Refresh());

            Assert.AreEqual(_client.Count<Employee>().Count, 1);
        }

        [Test]
        public void Get()
        {
            var employee = GenerateEmployee();
            _client.Index(employee, i => i.Refresh());

            var result = _client.Get<Employee>(request => request.Id(employee.Id)).Source;

            result.Id.Should().Be(employee.Id);
        }
    }
}
