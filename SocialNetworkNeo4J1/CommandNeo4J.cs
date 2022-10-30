using Neo4jClient;
using SocialNetworkNeo4J.EntitiesNeo4J;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkNeo4J
{
    public class CommandNeo4J
    {
        static BoltGraphClient Client
        {
            get
            {
                BoltGraphClient client = new BoltGraphClient("neo4j+s://7d048141.databases.neo4j.io:7687", "neo4j", "aOPngcWTU4je17igzqCf7sIPJzgo4A0u4oxVSJC3nXE");
                client.ConnectAsync().Wait();
                return client;
            }
        }

        private User currentUser;

        public void Authtentificate(string username, string pass)
        {
            var user = Client.Cypher 
                .Match("(u:User { username: $un})")
                .WithParam("un", username)
                .Where("u.password= $pass")
                .WithParam("pass", pass)
                .Return(u => u.As<User>())
                .ResultsAsync.Result;
            currentUser = user.ElementAt(0);
        }

        public void GerUsers()
        {
            var users = Client.Cypher
                .Match("(u:User)")              
                .Return(u => u.As<User>())
                .OrderBy("u.last_name ASC")
                .ResultsAsync.Result;
        }

        public void GerUsersWithFollowers()
        {
            var usersWithFollowers = Client.Cypher
                .Match("(u:User)")
                .OptionalMatch("(u)-[r:Following]->(f)")
                .Return((u, f)=>new
                {
                    User = u.As<User>(),
                    Followers = f.CollectAs<User>()               
                })
                .OrderBy("u.last_name ASC")
                .ResultsAsync.Result;
        }

        public void CreateUser(string userName, string firstName, string lastName, string password, string email)
        {
            var newUser = new User
                (
                userName,
                firstName,
                lastName,
                password,
                email
                );
            Client.Cypher
                .Create("(u:User $newUser)")
                .WithParam("newUser", newUser)
                .ExecuteWithoutResultsAsync().Wait();

        }

        public void CreateRelationshipUserFollower(string followerName)
        {
            Client.Cypher
                .Match("(u:User{username:$un})", "(f:User{username: $fn})")
                .WithParam("un", currentUser.UserName)
                .WithParam("fn", followerName)
                .Create("(u)-[:Following]->(f)")
                .ExecuteWithoutResultsAsync().Wait();
        }


        public void DeleteRelationshipUserFollower(string followerName)
        {
            Client.Cypher
                .Match("(u:User{username:$un})-[r:Following]->(f:User{username: $fn})")
                .WithParam("un", currentUser.UserName)
                .WithParam("fn", followerName)
                .Delete("r")
                .ExecuteWithoutResultsAsync().Wait();
        }


        public void DeleteUser(string userName)
        {
            DeleteAllRelationshipWithUser(userName);
            Client.Cypher
                .Match("(u:User {username: $deleteUser})")
                .WithParam("deleteUser", userName)
                .Delete("u")
                .ExecuteWithoutResultsAsync().Wait();

        }

        //match(p: User { username: '$un'})-[r]-(f: User)return r
        public void DeleteAllRelationshipWithUser(string userName)
        {
            Client.Cypher
                .Match("(u:User{username:$un})-[r]-(f:User)")
                .WithParam("un", currentUser.UserName)
                .Delete("r")
                .ExecuteWithoutResultsAsync().Wait();
        }


        //match(p: User { username: '$un'})-[r]-> (f: User { username:'user5'})return r
        public IEnumerable<Object>  SearchRelationshipOfUser(string searchedUser)
        {
            var userWithFollower = Client.Cypher
                .Match("(u:User {username: $un})-[r]-> (f: User {username: $fn})")
                .WithParam("un", currentUser.UserName)
                .WithParam("fn", searchedUser)
                .Return((u, f) => new
                {
                    User = u.As<User>(),
                    Follower = f.As<User>()
                })
                .ResultsAsync.Result;
            return userWithFollower;
        }

        // match sp = shortestPath((p: User { username: '$un'})-[*]- (: User{username:'user3'}))return sp gg
        public double ShortestPathToSearthedUser(string searchedUserName)
        {
            var userWithFollowers = Client.Cypher
                .Match("sp = shortestPath((:User {username: $un})-[*]-(:User {username: $fn}))")
                .WithParam("un", currentUser.UserName)
                .WithParam("fn", searchedUserName)
                .Return(sp=>sp.Length())
                .ResultsAsync.Result;
            return userWithFollowers.First();
        }

    }
}
