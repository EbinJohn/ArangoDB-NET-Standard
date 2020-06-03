using Arango.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Arango.Tests
{
    [Ignore]
    [TestClass()]
    public class FoxxOperationsTests
    {
        [TestMethod()]
        public void Should_execute_get_foxx_request()
        {
            var db = new ADatabase(Database.SystemAlias);
            var getResult = db.Foxx.Get<Dictionary<string, object>>("/getting-started/hello-world");

            Assert.AreEqual(200, getResult.StatusCode);
            Assert.AreEqual("bar", getResult.Value.String("foo"));
        }

        [TestMethod()]
        public void Should_execute_post_foxx_request_with_body()
        {
            var db = new ADatabase(Database.SystemAlias);

            var body = Dictator.New()
                .String("foo", "some string");

            var postResult = db.Foxx
                .Body(body)
                .Post<Dictionary<string, object>>("/getting-started/hello-world");

            Assert.AreEqual(200, postResult.StatusCode);
            Assert.AreEqual(body.String("foo"), postResult.Value.String("foo"));
        }
    }
}
