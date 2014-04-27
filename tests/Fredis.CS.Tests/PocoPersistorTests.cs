﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fredis;
using NUnit.Framework;
using ServiceStack.OrmLite;

namespace Fredis.Persistence.Tests {

    public class DataObject : BaseDataObject {
        public string Value { get; set; }
    }

    public class RootAsset : BaseDistributedDataObject {
        public string Value { get; set; }
    }

    public class DependentAsset : BaseDistributedDataObject {
        public string Value { get; set; }
        public Guid RootAssetGuid { get; set; }

        public override Guid GetRootGuid() {
            return RootAssetGuid;
        }
    }

    [TestFixture]
    public class PocoPersistorTests {

        public IPocoPersistor Persistor { get; set; }

        public PocoPersistorTests() {
            //var shards = new Dictionary<ushort, string> {
            //    {0, "App_Data/0.sqlite"}
            //    //,{1, "App_Data/1.sqlite"}
            //};
            //Persistor = new BasePocoPersistor(SqliteDialect.Provider, "App_Data/main.sqlite", shards);

            var shards = new Dictionary<ushort, string> {
                {0, "Server=localhost;Database=fredis.0;Uid=test;Pwd=test;"}
                //,{1, "App_Data/1.sqlite"}
            };
            Persistor = new BasePocoPersistor(MySqlDialect.Provider,
                "Server=localhost;Database=fredis;Uid=test;Pwd=test;", shards);


        }


        [Test]
        public void CouldCreateTableAndCrudDataObject() {
            

            Persistor.CreateTable<DataObject>(true);

            for (int i = 0; i < 10; i++) {
                var dobj = new DataObject() {
                    Value = "inserted"
                };

                Persistor.Insert(dobj);

                var fromDb = Persistor.GetById<DataObject>(dobj.Id);
                Assert.AreEqual("inserted", fromDb.Value);

                fromDb.Value = "updated";
                Persistor.Update(fromDb);
                fromDb = Persistor.GetById<DataObject>(dobj.Id);
                Assert.AreEqual("updated", fromDb.Value);
            }
        }

        [Test]
        public void CouldCreateTableAndCrudDistributedDataObject() {

            Persistor.CreateTable<RootAsset>(true);

            for (int i = 0; i < 10; i++) {


                var dobj = new RootAsset() {
                    Value = "inserted"
                };

                Persistor.Insert(dobj);

                var fromDb = Persistor.GetById<RootAsset>(dobj.Guid);
                Assert.AreEqual("inserted", fromDb.Value);

                fromDb.Value = "updated";
                Persistor.Update(fromDb);
                fromDb = Persistor.GetById<RootAsset>(dobj.Guid);
                Assert.AreEqual("updated", fromDb.Value);
            }
        }

        [Test]
        public void CouldCreateTableAndCrudManyDataObject() {

            Persistor.CreateTable<DataObject>(false);
            var list = new List<DataObject>();
            for (int i = 0; i < 500; i++) {

                var dobj = new DataObject() {
                    Value = "inserted"
                };
                list.Add(dobj);
            }
            Persistor.Insert(list);
        }


        [Test]
        public void CouldCreateTableAndCrudManyDistributedDataObject() {

            Persistor.CreateTable<RootAsset>(false);
            var list = new List<RootAsset>();
            for (int i = 0; i < 500; i++) {

                var dobj = new RootAsset() {
                    Value = "inserted"
                };
                list.Add(dobj);
            }
            Persistor.Insert(list);
        }
    }
}