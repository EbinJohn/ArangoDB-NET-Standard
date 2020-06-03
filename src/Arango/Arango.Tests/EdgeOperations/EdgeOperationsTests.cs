using Arango.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Arango.Tests
{
    [TestClass()]
    public class EdgeOperationsTests : IDisposable
    {
        readonly List<Dictionary<string, object>> _documents;
        
        public EdgeOperationsTests()
        {
            Database.CreateTestDatabase(Database.TestDatabaseGeneral);
			Database.CreateTestCollection(Database.TestDocumentCollectionName, ACollectionType.Document);
			Database.CreateTestCollection(Database.TestEdgeCollectionName, ACollectionType.Edge);
			
			_documents = Database.ClearCollectionAndFetchTestDocumentData(Database.TestDocumentCollectionName);
        }
        
        #region Create operations
        
        [TestMethod()]
        public void Should_create_empty_edge()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID());
            
            Assert.AreEqual(202, createResult.StatusCode);
            Assert.IsTrue(createResult.Success);
            Assert.IsTrue(createResult.HasValue);
            Assert.IsTrue(createResult.Value.IsString("_id"));
            Assert.IsTrue(createResult.Value.IsString("_key"));
            Assert.IsTrue(createResult.Value.IsString("_rev"));
        }
        
        [TestMethod()]
        public void Should_create_empty_edge_with_waitForSync()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var createResult = db
                .Document
                .WaitForSync(true)
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID());
            
            Assert.AreEqual(201, createResult.StatusCode);
            Assert.IsTrue(createResult.Success);
            Assert.IsTrue(createResult.HasValue);
            Assert.IsTrue(createResult.Value.IsString("_id"));
            Assert.IsTrue(createResult.Value.IsString("_key"));
            Assert.IsTrue(createResult.Value.IsString("_rev"));
        }
        
        [TestMethod()]
        public void Should_create_edge()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, document);
            
            Assert.AreEqual(202, createResult.StatusCode);
            Assert.IsTrue(createResult.Success);
            Assert.IsTrue(createResult.HasValue);
            Assert.IsTrue(createResult.Value.IsString("_id"));
            Assert.IsTrue(createResult.Value.IsString("_key"));
            Assert.IsTrue(createResult.Value.IsString("_rev"));
            
            var getResult = db
                .Document
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(getResult.Value.IsString("_from"));
            Assert.IsTrue(getResult.Value.IsString("_to"));
            Assert.AreEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.Int("bar"), document.Int("bar"));
        }

        [TestMethod()]
        public void Should_create_edge_with_returnNew_parameter()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "foo string value")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .ReturnNew()
                .CreateEdge(Database.TestEdgeCollectionName, document);

            Assert.AreEqual(202, createResult.StatusCode);
            Assert.IsTrue(createResult.Success);
            Assert.IsTrue(createResult.HasValue);
            Assert.IsTrue(createResult.Value.IsString("_id"));
            Assert.IsTrue(createResult.Value.IsString("_key"));
            Assert.IsTrue(createResult.Value.IsString("_rev"));
            Assert.IsTrue(createResult.Value.Has("new"));
            Assert.AreEqual(createResult.Value.ID(), createResult.Value.String("new._id"));
            Assert.AreEqual(createResult.Value.Key(), createResult.Value.String("new._key"));
            Assert.AreEqual(createResult.Value.Rev(), createResult.Value.String("new._rev"));
            Assert.AreEqual(document.String("_from"), createResult.Value.String("new._from"));
            Assert.AreEqual(document.String("_to"), createResult.Value.String("new._to"));
            Assert.AreEqual(document.String("foo"), createResult.Value.String("new.foo"));
            Assert.AreEqual(document.Int("bar"), createResult.Value.Int("new.bar"));
        }

        [TestMethod()]
        public void Should_create_edge_from_generic_object()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var dummy = new Dummy();
            dummy.Foo = "foo string value";
            dummy.Bar = 12345;

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), dummy);
            
            Assert.AreEqual(202, createResult.StatusCode);
            Assert.IsTrue(createResult.Success);
            Assert.IsTrue(createResult.HasValue);
            Assert.IsTrue(createResult.Value.IsString("_id"));
            Assert.IsTrue(createResult.Value.IsString("_key"));
            Assert.IsTrue(createResult.Value.IsString("_rev"));
            
            var getResult = db
                .Document
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(getResult.Value.IsString("_from"));
            Assert.IsTrue(getResult.Value.IsString("_to"));
            Assert.AreEqual(getResult.Value.String("foo"), dummy.Foo);
            Assert.AreEqual(getResult.Value.Int("bar"), dummy.Bar);
            Assert.AreEqual(0, dummy.Baz);
        }
        
        #endregion
        
        #region Check operations
        
        [TestMethod()]
        public void Should_check_edge()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var checkResult = db
                .Document
                .Check(createResult.Value.ID());
            
            Assert.AreEqual(200, checkResult.StatusCode);
            Assert.IsTrue(checkResult.Success);
            Assert.IsTrue(checkResult.HasValue);
            Assert.AreEqual(checkResult.Value, createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_check_edge_with_ifMatch()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var checkResult = db
                .Document
                .IfMatch(createResult.Value.Rev())
                .Check(createResult.Value.ID());
            
            Assert.AreEqual(200, checkResult.StatusCode);
            Assert.IsTrue(checkResult.Success);
            Assert.IsTrue(checkResult.HasValue);
            Assert.AreEqual(checkResult.Value, createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_check_edge_with_ifMatch_and_return_412()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var checkResult = db
                .Document
                .IfMatch("123456789")
                .Check(createResult.Value.ID());
            
            Assert.AreEqual(412, checkResult.StatusCode);
            Assert.IsFalse(checkResult.Success);
            Assert.IsTrue(checkResult.HasValue);
            Assert.AreEqual(checkResult.Value, createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_check_edge_with_ifNoneMatch()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var checkResult = db
                .Document
                .IfNoneMatch("123456789")
                .Check(createResult.Value.ID());
            
            Assert.AreEqual(200, checkResult.StatusCode);
            Assert.IsTrue(checkResult.Success);
            Assert.IsTrue(checkResult.HasValue);
            Assert.AreEqual(checkResult.Value, createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_check_edge_with_ifNoneMatch_and_return_304()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var checkResult = db
                .Document
                .IfNoneMatch(createResult.Value.Rev())
                .Check(createResult.Value.ID());
            
            Assert.AreEqual(304, checkResult.StatusCode);
            Assert.IsFalse(checkResult.Success);
            Assert.IsTrue(checkResult.HasValue);
            Assert.AreEqual(checkResult.Value, createResult.Value.Rev());
        }
        
        #endregion
        
        #region Get operations
        
        [TestMethod()]
        public void Should_get_edge()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(getResult.Value.IsString("_from"));
            Assert.IsTrue(getResult.Value.IsString("_to"));
            Assert.IsTrue(getResult.Value.IsString("foo"));
            // integers are by default parsed as long type
            Assert.IsTrue(getResult.Value.IsLong("bar"));
        }
        
        [TestMethod()]
        public void Should_get_edge_with_ifMatch()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .IfMatch(createResult.Value.Rev())
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(getResult.Value.IsString("_from"));
            Assert.IsTrue(getResult.Value.IsString("_to"));
            Assert.IsTrue(getResult.Value.IsString("foo"));
            // integers are by default parsed as long type
            Assert.IsTrue(getResult.Value.IsLong("bar"));
        }
        
        [TestMethod()]
        public void Should_get_edge_with_ifMatch_and_return_412()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .IfMatch("123456789")
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(412, getResult.StatusCode);
            Assert.IsFalse(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_get_edge_with_ifNoneMatch()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .IfNoneMatch("123456789")
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(getResult.Value.IsString("_from"));
            Assert.IsTrue(getResult.Value.IsString("_to"));
            Assert.IsTrue(getResult.Value.IsString("foo"));
            // integers are by default parsed as long type
            Assert.IsTrue(getResult.Value.IsLong("bar"));
        }
        
        [TestMethod()]
        public void Should_get_edge_with_ifNoneMatch_and_return_304()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .IfNoneMatch(createResult.Value.Rev())
                .Get(createResult.Value.ID());
            
            Assert.AreEqual(304, getResult.StatusCode);
            Assert.IsFalse(getResult.Success);
            Assert.IsFalse(getResult.HasValue);
        }
        
        [TestMethod()]
        public void Should_get_edge_as_generic_object()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .Get<Dummy>(createResult.Value.ID());
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(document.String("foo"), getResult.Value.Foo);
            Assert.AreEqual(document.Int("bar"), getResult.Value.Bar);
            Assert.AreEqual(0, getResult.Value.Baz);
        }
        
        #endregion
        
        #region Get in/out/any
        
        [TestMethod()]
        public void Should_get_edges_in()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[1].ID(), _documents[0].ID(), document);
            
            var getResult = db
                .Document
                .GetEdges(Database.TestEdgeCollectionName, _documents[0].ID(), ADirection.In);
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.Count, 1);
            
            Assert.AreEqual(getResult.Value[0].ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value[0].Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value[0].Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_get_edges_out()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .GetEdges(Database.TestEdgeCollectionName, _documents[0].ID(), ADirection.Out);
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.Count, 1);
            
            Assert.AreEqual(getResult.Value[0].ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value[0].Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value[0].Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_get_edges_any()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
        	var db = new ADatabase(Database.Alias);
        	
            var document = new Dictionary<string, object>()
        		.String("foo", "foo string value")
        		.Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var getResult = db
                .Document
                .GetEdges(Database.TestEdgeCollectionName, _documents[0].ID(), ADirection.Any);
            
            Assert.AreEqual(200, getResult.StatusCode);
            Assert.IsTrue(getResult.Success);
            Assert.IsTrue(getResult.HasValue);
            Assert.AreEqual(getResult.Value.Count, 1);
            
            Assert.AreEqual(getResult.Value[0].ID(), createResult.Value.ID());
            Assert.AreEqual(getResult.Value[0].Key(), createResult.Value.Key());
            Assert.AreEqual(getResult.Value[0].Rev(), createResult.Value.Rev());
        }
        
        #endregion
        
        #region Update operations
        
        [TestMethod()]
        public void Should_update_edge()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);
            
            var updateResult = db
                .Document
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreNotEqual(getResult.Value.Int("bar"), document.Int("bar"));
            Assert.AreEqual(getResult.Value.Int("bar"), newDocument.Int("bar"));
            
            // by default JSON integers are deserialized to long type
            Assert.IsTrue(getResult.Value.IsLong("baz"));
        }

        [TestMethod()]
        public void Should_update_edge_with_returnOld()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);

            var updateResult = db
                .Document
                .ReturnOld()
                .Update(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(updateResult.Value.Has("old"));
        }

        [TestMethod()]
        public void Should_update_edge_with_returnNew()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);

            var updateResult = db
                .Document
                .ReturnNew()
                .Update(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(updateResult.Value.Has("new"));
        }

        [TestMethod()]
        public void Should_update_edge_with_waitForSync()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);
            
            var updateResult = db
                .Document
                .WaitForSync(true)
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(201, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreNotEqual(getResult.Value.Int("bar"), document.Int("bar"));
            Assert.AreEqual(getResult.Value.Int("bar"), newDocument.Int("bar"));
            
            // by default JSON integers are deserialized to long type
            Assert.IsTrue(getResult.Value.IsLong("baz"));
        }

        [TestMethod()]
        public void Should_update_edge_with_ignoreRevs_set_to_false()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .Rev(createResult.Value.Rev())
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);

            var updateResult = db
                .Document
                .IgnoreRevs(false)
                .Update(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
        }

        [TestMethod()]
        public void Should_update_edge_with_ifMatch()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);
            
            var updateResult = db
                .Document
                .IfMatch(createResult.Value.Rev())
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreNotEqual(getResult.Value.Int("bar"), document.Int("bar"));
            Assert.AreEqual(getResult.Value.Int("bar"), newDocument.Int("bar"));
            
            // by default JSON integers are deserialized to long type
            Assert.IsTrue(getResult.Value.IsLong("baz"));
        }
        
        [TestMethod()]
        public void Should_update_edge_with_ifMatch_and_return_412()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Int("bar", 54321)
                .Int("baz", 12345);
            
            var updateResult = db
                .Document
                .IfMatch("123456789")
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(412, updateResult.StatusCode);
            Assert.IsFalse(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(updateResult.Value.Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_update_edge_with_keepNull()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Object("bar", null);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            document.Merge(createResult.Value);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Object("baz", null);
            
            var updateResult = db
                .Document
                .KeepNull(false)
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), document.ID());
            Assert.AreEqual(updateResult.Value.Key(), document.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), document.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.IsTrue(getResult.Value.Has("bar"));
            
            Assert.IsFalse(getResult.Value.Has("baz"));
        }
        
        [TestMethod()]
        public void Should_update_edge_with_mergeArrays_set_to_true()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Document("bar", new Dictionary<string, object>().String("foo", "string value"));
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            document.Merge(createResult.Value);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Document("bar", new Dictionary<string, object>().String("bar", "other string value"));
            
            var updateResult = db
                .Document
                .MergeObjects(true) // this is also default behavior
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), document.ID());
            Assert.AreEqual(updateResult.Value.Key(), document.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), document.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.IsTrue(getResult.Value.Has("bar.foo"));
            
            Assert.IsTrue(getResult.Value.Has("bar.bar"));
        }
        
        [TestMethod()]
        public void Should_update_edge_with_mergeArrays_set_to_false()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Document("bar", new Dictionary<string, object>().String("foo", "string value"));
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            document.Merge(createResult.Value);
            
            var newDocument = new Dictionary<string, object>()
                .String("foo", "some other new string")
                .Document("bar", new Dictionary<string, object>().String("bar", "other string value"));
            
            var updateResult = db
                .Document
                .MergeObjects(false)
                .Update(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), document.ID());
            Assert.AreEqual(updateResult.Value.Key(), document.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), document.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.IsFalse(getResult.Value.Has("bar.foo"));
            
            Assert.IsTrue(getResult.Value.Has("bar.bar"));
        }
        
        [TestMethod()]
        public void Should_update_edge_with_generic_object()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var dummy = new Dummy();
            dummy.Foo = "some other new string";
            dummy.Bar = 54321;
            dummy.Baz = 12345;
            
            var updateResult = db
                .Document
                .Update(createResult.Value.ID(), dummy);
            
            Assert.AreEqual(202, updateResult.StatusCode);
            Assert.IsTrue(updateResult.Success);
            Assert.IsTrue(updateResult.HasValue);
            Assert.AreEqual(updateResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(updateResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(updateResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(updateResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), updateResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), updateResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), updateResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), dummy.Foo);
            
            Assert.AreNotEqual(getResult.Value.Int("bar"), document.Int("bar"));
            Assert.AreEqual(getResult.Value.Int("bar"), dummy.Bar);
            Assert.AreEqual(getResult.Value.Int("baz"), dummy.Baz);
        }
        
        #endregion
        
        #region Replace operations
        
        [TestMethod()]
        public void Should_replace_edge()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);
            
            var replaceResult = db
                .Document
                .Replace(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(replaceResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), replaceResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), replaceResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), replaceResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreEqual(getResult.Value.Int("baz"), newDocument.Int("baz"));

            Assert.IsFalse(getResult.Value.Has("bar"));
        }

        [TestMethod()]
        public void Should_replace_edge_with_returnOld()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);

            var replaceResult = db
                .Document
                .ReturnOld()
                .Replace(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(replaceResult.Value.Has("old"));
        }

        [TestMethod()]
        public void Should_replace_edge_with_returnNew()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);

            var replaceResult = db
                .Document
                .ReturnNew()
                .Replace(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(replaceResult.Value.Has("new"));
        }

        [TestMethod()]
        public void Should_replace_edge_with_waitForSync()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);
            
            var replaceResult = db
                .Document
                .WaitForSync(true)
                .Replace(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(201, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(replaceResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), replaceResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), replaceResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), replaceResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreEqual(getResult.Value.Int("baz"), newDocument.Int("baz"));

            Assert.IsFalse(getResult.Value.Has("bar"));
        }

        [TestMethod()]
        public void Should_replace_edge_with_ignoreRevs_set_to_false()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .Rev(createResult.Value.Rev())
                .String("foo", "some other new string")
                .Int("baz", 54321);

            var replaceResult = db
                .Document
                .IgnoreRevs(false)
                .Replace(createResult.Value.ID(), newDocument);

            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
        }

        [TestMethod()]
        public void Should_replace_edge_with_ifMatch()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            document.Merge(createResult.Value);
            
            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);
            
            var replaceResult = db
                .Document
                .IfMatch(document.Rev())
                .Replace(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), document.ID());
            Assert.AreEqual(replaceResult.Value.Key(), document.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), document.Rev());
            
            var getResult = db
                .Document
                .Get(replaceResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), replaceResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), replaceResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), replaceResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), newDocument.String("foo"));
            
            Assert.AreEqual(getResult.Value.Int("baz"), newDocument.Int("baz"));
            
            Assert.IsFalse(getResult.Value.Has("bar"));
        }
        
        [TestMethod()]
        public void Should_replace_edge_with_ifMatch_and_return_412()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            document.Merge(createResult.Value);
            
            var newDocument = new Dictionary<string, object>()
                .From(_documents[0].ID())
                .To(_documents[1].ID())
                .String("foo", "some other new string")
                .Int("baz", 54321);
            
            var replaceResult = db
                .Document
                .IfMatch("123456789")
                .Replace(createResult.Value.ID(), newDocument);
            
            Assert.AreEqual(412, replaceResult.StatusCode);
            Assert.IsFalse(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), document.ID());
            Assert.AreEqual(replaceResult.Value.Key(), document.Key());
            Assert.AreEqual(replaceResult.Value.Rev(), document.Rev());
        }
        
        [TestMethod()]
        public void Should_replace_edge_with_generic_object()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var dummy = new Dummy();
            dummy.Foo = "some other new string";
            dummy.Baz = 54321;
            
            var replaceResult = db
                .Document
                .ReplaceEdge(createResult.Value.ID(), _documents[0].ID(), _documents[1].ID(), dummy);
            
            Assert.AreEqual(202, replaceResult.StatusCode);
            Assert.IsTrue(replaceResult.Success);
            Assert.IsTrue(replaceResult.HasValue);
            Assert.AreEqual(replaceResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(replaceResult.Value.Key(), createResult.Value.Key());
            Assert.AreNotEqual(replaceResult.Value.Rev(), createResult.Value.Rev());
            
            var getResult = db
                .Document
                .Get(replaceResult.Value.ID());
            
            Assert.AreEqual(getResult.Value.ID(), replaceResult.Value.ID());
            Assert.AreEqual(getResult.Value.Key(), replaceResult.Value.Key());
            Assert.AreEqual(getResult.Value.Rev(), replaceResult.Value.Rev());
            
            Assert.AreNotEqual(getResult.Value.String("foo"), document.String("foo"));
            Assert.AreEqual(getResult.Value.String("foo"), dummy.Foo);
            
            Assert.AreEqual(getResult.Value.Int("baz"), dummy.Baz);

            Assert.AreEqual(0, getResult.Value.Int("bar"));
        }
        
        #endregion
        
        #region Delete operations
        
        [TestMethod()]
        public void Should_delete_edge()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var deleteResult = db.Document
                .Delete(createResult.Value.ID());
            
            Assert.AreEqual(202, deleteResult.StatusCode);
            Assert.IsTrue(deleteResult.Success);
            Assert.IsTrue(deleteResult.HasValue);
            Assert.AreEqual(deleteResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(deleteResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(deleteResult.Value.Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_delete_edge_with_waitForSync()
        {
        	Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var deleteResult = db.Document
                .WaitForSync(true)
                .Delete(createResult.Value.ID());
            
            Assert.AreEqual(200, deleteResult.StatusCode);
            Assert.IsTrue(deleteResult.Success);
            Assert.IsTrue(deleteResult.HasValue);
            Assert.AreEqual(deleteResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(deleteResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(deleteResult.Value.Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_delete_edge_with_ifMatch()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var deleteResult = db.Document
                .IfMatch(createResult.Value.Rev())
                .Delete(createResult.Value.ID());
            
            Assert.AreEqual(202, deleteResult.StatusCode);
            Assert.IsTrue(deleteResult.Success);
            Assert.IsTrue(deleteResult.HasValue);
            Assert.AreEqual(deleteResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(deleteResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(deleteResult.Value.Rev(), createResult.Value.Rev());
        }
        
        [TestMethod()]
        public void Should_delete_edge_with_ifMatch_and_return_412()
        {
            Database.ClearTestCollection(Database.TestEdgeCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);
            
            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);
            
            var deleteResult = db.Document
                .IfMatch("123456789")
                .Delete(createResult.Value.ID());
            
            Assert.AreEqual(412, deleteResult.StatusCode);
            Assert.IsFalse(deleteResult.Success);
            Assert.IsTrue(deleteResult.HasValue);
            Assert.AreEqual(deleteResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(deleteResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(deleteResult.Value.Rev(), createResult.Value.Rev());
        }

        [TestMethod()]
        public void Should_delete_edge_with_returnOld_parameter()
        {
            var documents = Database.ClearCollectionAndFetchTestDocumentData(Database.TestDocumentCollectionName);
            var db = new ADatabase(Database.Alias);

            var document = new Dictionary<string, object>()
                .String("foo", "some string")
                .Int("bar", 12345);

            var createResult = db
                .Document
                .CreateEdge(Database.TestEdgeCollectionName, _documents[0].ID(), _documents[1].ID(), document);

            var deleteResult = db.Document
                .ReturnOld()
                .Delete(createResult.Value.ID());

            Assert.AreEqual(202, deleteResult.StatusCode);
            Assert.IsTrue(deleteResult.Success);
            Assert.AreEqual(deleteResult.Value.ID(), createResult.Value.ID());
            Assert.AreEqual(deleteResult.Value.Key(), createResult.Value.Key());
            Assert.AreEqual(deleteResult.Value.Rev(), createResult.Value.Rev());
            Assert.IsTrue(deleteResult.Value.Has("old"));
        }

        #endregion

        public void Dispose()
        {
            Database.DeleteTestDatabase(Database.TestDatabaseGeneral);
        }
    }
}
