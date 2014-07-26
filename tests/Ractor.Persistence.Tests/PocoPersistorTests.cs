﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite;

namespace Ractor.Persistence.Tests {

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

            var shards = new Dictionary<byte, string> {
                {1, "Server=localhost;Database=fredis.0;Uid=test;Pwd=test"},
                {2, "Server=localhost;Database=fredis.1;Uid=test;Pwd=test"},
                {3, "Server=localhost;Database=fredis.2;Uid=test;Pwd=test"},
                {4, "Server=localhost;Database=fredis.3;Uid=test;Pwd=test"}
                //,{1, "App_Data/1.sqlite"}
            };
            Persistor = new BasePocoPersistor(MySqlDialect.Provider,
                "Server=localhost;Database=fredis;Uid=test;Pwd=test", shards, null, SequentialGuidType.SequentialAsBinary);
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

            for (int i = 0; i < 1; i++) {

                var dobj = new RootAsset() {
                    Value = "inserted"
                };

                Persistor.Insert(dobj);

                var fromDb = Persistor.GetById<RootAsset>(dobj.Id);
                Assert.AreEqual("inserted", fromDb.Value);

                Console.WriteLine(dobj.Id);

                fromDb.Value = "updated";
                Persistor.Update(fromDb);
                fromDb = Persistor.GetById<RootAsset>(dobj.Id);
                Assert.AreEqual("updated", fromDb.Value);
            }
        }

        [Test]
        public void CouldCreateTableAndInsertManyDataObject() {

            Persistor.CreateTable<DataObject>(true);
            var sw = new Stopwatch();
            sw.Start();
            var list = new List<DataObject>();
            for (int i = 0; i < 100000; i++) {

                var dobj = new DataObject() {
                    Value = "inserted"
                };
                //Persistor.Insert(dobj);
                list.Add(dobj);
            }
            Persistor.Insert(list);
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            
        }

         [Test]
        public void RandomTest() {
            for (int i = 0; i < 100; i++) { Console.WriteLine((new Random()).Next(0, 2)); }
            
        }

        [Test]
        public void CouldCreateTableAndInsertManyDistributedDataObject() {

            Persistor.CreateTable<RootAsset>(true);
            var sw = new Stopwatch();
            sw.Start();
            var list = new List<RootAsset>();
            for (int i = 0; i < 100000; i++) {

                var dobj = new RootAsset() {
                    Value = "inserted"
                };
                list.Add(dobj);
            }
            Persistor.Insert(list);
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
        }


        [Test]
        public void CouldSelectManyDistributedDataObject() {

            //Persistor.CreateTable<RootAsset>(true);
            //var list = new List<RootAsset>();
            //for (int i = 0; i < 100000; i++) {

            //    var dobj = new RootAsset() {
            //        Value = "inserted"
            //    };
            //    list.Add(dobj);
            //}
            //Persistor.Insert(list);

            var values = Persistor.Select<RootAsset>().Select(ra => ra.Id).ToList();
            RootAsset a;
            foreach (var value in values) {
                a = Persistor.GetById<RootAsset>(value);
            }
            //Persistor.GetByIds<RootAsset>(values);
        }

    }
}
