﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LunaCinemasBackEndInDotNet.Models;
using MongoDB.Driver;

namespace LunaCinemasBackEndInDotNet.Persistence
{
    public interface IReviewContext
    {
        List<Review> FindByFilmId(string filmId);
        void AddReview(Review review);
        List<Review> FindById(string reviewId);
        void deleteAll();
    }
    
    [ExcludeFromCodeCoverage]
    public class ReviewContext : IReviewContext
    {
        private readonly ILunaCinemasDatabaseSettings _settings;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Review> _reviewCollection;

        public ReviewContext(ILunaCinemasDatabaseSettings settings)
        {
            _settings = settings;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
            _reviewCollection = _database.GetCollection<Review>(_settings.ReviewsCollectionName);
        }

        public List<Review> FindByFilmId(string filmId)
        {
            return _reviewCollection.Find(review => review.FilmId.Equals(filmId)).ToList();
        }

        public void AddReview(Review review)
        {
            _reviewCollection.InsertOne(review);
        }

        public List<Review> FindById(string reviewId)
        {
            return _reviewCollection.Find(review => review.Id.Equals(reviewId)).ToList();
        }

        public void deleteAll()
        {
            _reviewCollection.DeleteMany(review => true);
        }
    }
}
