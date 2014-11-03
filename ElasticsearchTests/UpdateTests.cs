using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class UpdateTests : ElasticSearchTestBase
    {
        [Test]
        public void Update()
        {
            _client.Index(GenerateEmployee("1"), index => index.Refresh());

            _client.Update<Employee>(update => update
                .Id("1")
                .Doc(new Employee { FirstName = "NewFirstName" })
                .Refresh());

            var updated = _client.Get<Employee>(get => get.Id("1")).Source;
            updated.FirstName.Should().Be("NewFirstName");
        }

        [Test]
        public void Update_by_script()
        {
            //  script.disable_dynamic: false

            //  curl -XPOST 'localhost:9200/shinetech/employee/1/_update' -d '{
            //      "script" : "ctx._source.age += count",
            //      "params" : {
            //          "count" : 1
            //      }
            //  }'
            var employee = GenerateEmployee("1");
            _client.Index(employee, index => index.Refresh());

            _client.Update<Employee>(update => update
                .Id("1")
                .Script("ctx._source.age += 1")
                .Refresh());

            var updated = _client.Get<Employee>(get => get.Id("1")).Source;
            updated.Age.Should().Be(employee.Age + 1);
        }
    }
}
