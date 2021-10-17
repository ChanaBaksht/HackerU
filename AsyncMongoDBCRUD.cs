using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisDemoApp.Models;
using MongoDemo.Models;

namespace RedisDemoApp.Extentions
{
    class AsyncMongoDBCRUD
    {
        private IMongoDatabase db;

        public AsyncMongoDBCRUD(string database)
        {
            string connectionString = "mongodb://localhost:27017/" + database;
            var client = new MongoClient(connectionString);
            db = client.GetDatabase(database);
        }

        public async Task InsertRecordAsync<T>(string table, T record)
        {
            var collection = db.GetCollection<T>(table);
            await collection.InsertOneAsync(record);
        }

        public async Task InsertManyRecordAsync<T>(string table, List<T> records)
        {
            var collection = db.GetCollection<T>(table);
            await collection.InsertManyAsync(records);
        }

        public async Task DeleteAllRecordAsync<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Empty;
            await collection.DeleteManyAsync(filter);
        }

        public async Task<List<T>> LoadAllRecordsAsync<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            //return collection.Find(new BsonDocument()).ToList();
            var filter = Builders<T>.Filter.Empty;
            //var x = collection.Find(new BsonDocument()).Project(Builders<PersonModel>.Projection.Include(p=>p.LastName).Exclude("_id")).ToList();
            //var allDocs = collection.Find(new BsonDocument()).Project<PersonModel>("{FirstName: 1, LastName: 1}").ToList();
            var lstRecords = await collection.FindAsync(filter);
            return lstRecords.ToList();
        }

        public async Task<T> LoadRecordByIdAsync<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            //return collection.Find(new BsonDocument()).ToList();
            var filter = Builders<T>.Filter.Eq("_id", id);
            var recordById = await collection.FindAsync(filter);
            return recordById.FirstOrDefault();
        }

        public async Task UpdateRecordByLastNameAsync(string table, string lastName, AdressModel addr)
        {
            var collection = db.GetCollection<PersonModel>(table);

            var filter = Builders<PersonModel>.Filter.Eq(a => a.LastName, lastName);
            var updateDefinition = Builders<PersonModel>.Update.Set(a => a.PrimaryAddress, addr);

            await collection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task UpdateRecordsAsync<T>(string table, FilterDefinition<T> filter, UpdateDefinition<T> updateDefinition)
        {
            var collection = db.GetCollection<T>(table);

            await collection.UpdateManyAsync(filter, updateDefinition);
        }

        public async Task CreateIndexAsync(string table)
        {
            var collection = db.GetCollection<PersonModel>(table);

            var indexKeys = Builders<PersonModel>.IndexKeys;
            var indexModel = new CreateIndexModel<PersonModel>(indexKeys
                .Ascending(a => a.FirstName).Ascending(a => a.LastName), new CreateIndexOptions { Unique = true, Name = "IX_Name" });

            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public async Task<List<NameModel>> LoadExcludeAllRecordsAsync(string table)
        {
            var collection = db.GetCollection<PersonModel>(table);

            var fieldsBuilder = Builders<PersonModel>.Projection;
            var filter = Builders<PersonModel>.Filter.Empty;

            var fields = fieldsBuilder.Include(d => d.FirstName)
                                      .Include(d => d.LastName);
            //.Exclude(d => d.Id);

            //var fields = fieldsBuilder.Exclude(d => d.DateOfBirth)
            //                          .Exclude(d => d.PrimaryAddress)
            //                          .Exclude(d => d.Id)
            //                          .Exclude(d => d.Age);

            var lst = await collection.Find(filter).Project<NameModel>(fields).ToListAsync();
            return lst;
        }

        public async Task<List<PersonLookup>> JoinTablesAsync()
        {
            var usersCollection = db.GetCollection<PersonModel>("users");
            var booksCollection = db.GetCollection<BookModel>("books");

            var resultAll = await usersCollection.Aggregate()
                                            .Lookup<PersonModel, BookModel, PersonLookup>(booksCollection, a => a.Id, a => a.PersonId, a => a.BookList)
                                            .Match(p => p.BookList.Count > 0)
                                            .ToListAsync();

            var filterDefinition = Builders<PersonModel>.Filter.Where(p => p.PrimaryAddress.StreetAddress == "street");

            var result = await usersCollection.Aggregate()
                                        .Match(filterDefinition)
                                        .Lookup<PersonModel, BookModel, PersonLookup>(booksCollection, a => a.Id, a => a.PersonId, a => a.BookList)
                                        .ToListAsync();

            return resultAll;
        }
    }
}
