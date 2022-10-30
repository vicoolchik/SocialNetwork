using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SocialNetworkMongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkMongoDB
{
    public class CommandMongoDB
    {
        static string ConnectionString
        {
            get
            {
                return new ConfigurationBuilder().AddJsonFile(@"C:\work\c#\SocialNetwork\SocialNetworkMongoDB\appsettings.json").Build().GetConnectionString("SN");
            }
        }


        private User currentUser;

        private IMongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<User> usersCollection;
        private IMongoCollection<Post> postsCollection;

        public CommandMongoDB()
        {
            client = new MongoClient(ConnectionString);
            database = client.GetDatabase("social_network");
            usersCollection = database.GetCollection<User>("users");
            postsCollection = database.GetCollection<Post>("posts");
        }
        

        public bool Authtentificate(string username, string pass)
        {
            var documents = usersCollection.Find(_ => true).ToListAsync();
            var filter = Builders<User>.Filter.Eq("username", username) & Builders<User>.Filter.Eq("password", pass);
            var found = usersCollection.Find(filter).ToList();
            if (found.Count != 0)
            {
                currentUser = found[0];
                return true;
            }       
            return false;
        }

        public List<Post> GetStreamPosts()
        {
            var filter = Builders<Post>.Filter.In("username", currentUser.Follows);
            var relatedPosts = postsCollection.Find(filter).Sort("{date : -1}").ToList();
            return relatedPosts;
        }

        public List<Post> GetStreamPosts(string username)
        {
            var filter = Builders<Post>.Filter.Eq("username", username);
            var relatedPosts = postsCollection.Find(filter).Sort("{date : -1}").ToList();
            return relatedPosts;
        }

        public List<User> GetFollows()
        {
            var filter = Builders<User>.Filter.In("username", currentUser.Follows);
            var follows = usersCollection.Find(filter).ToList();
            return follows;
        }

        public void LikePost(Post post)
        {
            if (!post.Likes.Contains(currentUser.UserName))
            {
                post.Likes.Add(currentUser.UserName);
                postsCollection.ReplaceOne(p => p.Id == post.Id, post);
            }
            else
            {
                post.Likes.Remove(currentUser.UserName);
                postsCollection.ReplaceOne(p => p.Id == post.Id, post);
            }
        }

        public void WriteComment(Post post, string comment)
        {
            post.Comments.Add(new Comment { UserName = currentUser.UserName, CommentText = comment, CreationDate = DateTime.Now });
            postsCollection.ReplaceOne(p => p.Id == post.Id, post);
        }

        public bool? UnFollow(string username)
        {
            bool? success = currentUser.Follows.Remove(username);
            if (success==true)
            {
                usersCollection.ReplaceOne(u => u.Id == currentUser.Id, currentUser);
            }
            return success;
        }

        public bool? CheckUserIsFollowed(string username)
        {
            return currentUser.Follows.Contains(username);
        }

        public User FindUser(string username)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = usersCollection.Find(filter).ToList();

            if (users.Count == 1)
            {
                return users[0];
            }
            return null;
        }

        public void Follow(string username)
        {
            currentUser.Follows.Add(username);
            usersCollection.ReplaceOne(u => u.Id == currentUser.Id, currentUser);
        }

        public void CreateUser(string userName, string firstName, string lastName, string password, string email, List<string> follows)
        {
            var newUser = new User
            {
                UserName=userName,
                FirstName=firstName,
                LastName=lastName,
                Password=password,
                Email=email,   
                Follows= follows
            };

            usersCollection.InsertOne(newUser);
        }

        public void DeleteUser(string userName)
        {
            usersCollection.DeleteOne(p => p.UserName == userName);
        }
    }
}

