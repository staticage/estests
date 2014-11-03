using ElasticsearchTests.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticsearchTests
{
    public class DeleteTests : ElasticSearchTestBase
    {
        [Test]
        public void Delete_by_id_is_not_realtime()
        {
            _client.Index(GenerateEmployee("1"), index => index.Refresh());
            _client.Delete<Employee>(delete => delete.Id("1"));

            _client.Count<Employee>().Count.Should().Be(1);
        }

        [Test]
        public void Delete_by_id()
        {
            //  curl -XDELETE 'http://localhost:9200/shinetech/employee/1'

            _client.Index(GenerateEmployee("1"), index => index.Refresh());
            _client.Delete<Employee>(delete => delete.Id("1").Refresh());

            _client.Count<Employee>().Count.Should().Be(0);
        }

        [Test]
        public void Delete_all()
        {
            IndexEmployees();

            _client.DeleteByQuery<Employee>(query => query.MatchAll());

            _client.Count<Employee>().Count.Should().Be(0);
        }

        [Test]
        public void Delete_by_query()
        {
            //  curl -XDELETE 'http://localhost:9200/shinetech/employee/_query?q=firstName:John'

            //  curl -XDELETE 'http://localhost:9200/shinetech/employee/_query' -d '{
            //      "query" : {
            //          "term" : { "firstName" : "John" }
            //      }
            //  }'

            IndexEmployees();

            _client.DeleteByQuery<Employee>(delete => delete.Query(q => q
                .Match(m => m.OnField(e => e.FirstName).Query("John"))));

            _client.Count<Employee>().Count.Should().Be(2);
        }

        [Test]
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
