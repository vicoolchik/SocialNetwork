using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SocialNetworkMongoDB.Entities
{

    public class Comment
    {
        [BsonElement("username")]
        public string UserName { get; set; }
        [BsonElement("text")]
        public string CommentText { get; set; }
        [BsonElement("date")]
        public DateTime CreationDate { get; set; }
        public override string ToString()
        {

            return $"username: {UserName}   date: {CreationDate.ToShortDateString()}\n{CommentText}\n\n";

        }
    }
}

