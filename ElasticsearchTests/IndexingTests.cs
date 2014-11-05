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
            //  curl -XPUT 'http://localhost:9200/shinetech/employee/1' -d '{
            //      "first_name":"John",
            //      "last_name":"Smith",
            //      "age":25,
            //      "about":"I love to go rock climbing",
            //      "interests": [ "sports", "music" ]
            //  }'

            _client.Index(GenerateEmployee());

            Assert.AreEqual(_client.Count<Employee>().Count, 0);
        }

        [Test]
        public void RealtimeIndexing()
        {
            // curl -XPOST localhost:9200/_refresh
            // curl -XPOST localhost:9200/shinetech/employee/_refresh

            _client.Index(GenerateEmployee(),i=> i.Refresh());
           

            Assert.AreEqual(_client.Count<Employee>().Count, 1);
        }

        [Test]
        public void RealtimeIndexing2()
        {
            _client.Index(GenerateEmployee(), i => i.Refresh());

            Assert.AreEqual(_client.Count<Employee>().Count, 1);
        }

        [Test]
        public void Change_refresh_interval()
        {
            //  curl -XPUT 'http://localhost:9200/shinetech' -d '{
            //    "settings": {
            //      "refresh_interval": "30s" 
            //    }
            //  }

            //  curl -POST 'http://localhost:9200/shinetech/_settings' -d '{
            //      "refresh_interval": "30s" 
            //  }

            //  curl -POST 'http://localhost:9200/shinetech/_settings' -d '{
            //      "refresh_interval": "-1" 
            //  }

            _client.UpdateSettings(update => update
                .Index("shinetech")
                .RefreshInterval("30s"));

            var indexSetting = _client.GetIndexSettings(get => get.Index<Employee>()).IndexSettings;
            
            indexSetting.Settings["refresh_interval"].Should().Be("30s");
        }
    }
}
