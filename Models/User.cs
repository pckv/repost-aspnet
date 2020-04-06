using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    [Table("users")]
    public class User
    {
        [JsonIgnore] public int Id { get; set; }

        public string Username { get; set; }
        public string Bio { get; set; }

        [JsonPropertyName("avatar_url")] public string AvatarUrl { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }

        [JsonIgnore] public string HashedPassword { get; set; }

        [JsonIgnore] public virtual List<Resub> Resubs { get; set; }
        [JsonIgnore] public virtual List<Post> Posts { get; set; }
    }

    public class CreateUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EditUser : EditModel
    {
        public string Bio
        {
            get => (string) GetField(nameof(Bio));
            set => SetField(nameof(Bio), value);
        }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl
        {
            get => (string) GetField(nameof(AvatarUrl));
            set => SetField(nameof(AvatarUrl), value);
        }
    }
}