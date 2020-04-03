using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    [Table("users")]
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        
        [JsonIgnore]
        public string HashedPassword { get; set; }
    }
    
    public class CreateUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EditUser
    {
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
    }
}