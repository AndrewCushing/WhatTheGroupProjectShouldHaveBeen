﻿using LunaCinemasBackEndInDotNet.BusinessLogic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LunaCinemasBackEndInDotNet.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("reviewId")]
        public string ReviewId { get; set; }
        [BsonElement("username")]
        public string Username { get; set; }
        [BsonElement("body")]
        public string Body { get; set; }
        [BsonElement("_class")]
        public string JavaClass { get; set; }

        public Comment(string reviewId, string username, string body)
        {
            ReviewId = reviewId;
            Username = username;
            Body = body;
        }
    }
}