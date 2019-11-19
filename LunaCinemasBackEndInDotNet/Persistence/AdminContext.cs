﻿using System.Collections.Generic;
using LunaCinemasBackEndInDotNet.Models;
using MongoDB.Driver;

namespace LunaCinemasBackEndInDotNet.Persistence
{
    public interface IAdminContext
    {
        void Save(Admin user);
        List<Admin> FindByEmail(string email);
        Admin FindById(string userId);
        bool DeleteUser(string userId);
        void DeleteAll();
        string FindByEmailAndPassword(string email, string password);
    }
    public class AdminContext : IAdminContext
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        public AdminContext(ILunaCinemasDatabaseSettings settings)
        {
            _adminCollection = new MongoClient(settings.ConnectionString)
                .GetDatabase(settings.DatabaseName)
                .GetCollection<Admin>(settings.AdminCollectionName);
        }
        public void Save(Admin admin)
        {
            _adminCollection.InsertOne(admin);
        }

        public List<Admin> FindByEmail(string email)
        {
            return _adminCollection.Find(user => user.Email == email).ToList();
        }

        public Admin FindById(string userId)
        {
            return _adminCollection.Find(user => user.Id == userId).ToList()[0];
        }

        public bool DeleteUser(string userId)
        {
            try
            {
                _adminCollection.DeleteOne(user => user.Id == userId);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public void DeleteAll()
        {
            _adminCollection.DeleteMany(admin => true);
        }

        public string FindByEmailAndPassword(string email, string password)
        {
            List<Admin> result = _adminCollection
                .Find(admin => admin.Email == email && admin.Password == password).ToList();
            if (result.Count > 0)
            {
                return result[0].Id;
            }
            return null;
        }
    }
}