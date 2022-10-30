using Newtonsoft.Json;

namespace SocialNetworkNeo4J.EntitiesNeo4J
{
    public class User
    {
        public User(string userName, string firstName, string lastName, string password, string email)
        {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            Email = email;
        }
        public User() { }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} - username: {UserName}";
        }
    }
}

