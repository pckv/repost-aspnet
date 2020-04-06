using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    [Table("resubs")]
    public class Resub
    {
        [JsonIgnore] public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }

        [JsonIgnore] public User Owner { get; set; }
        [JsonIgnore] public List<Post> Posts { get; set; }

        [JsonPropertyName("owner_username")] public string OwnerUsername => Owner.Username;

        public bool IsOwner(User user)
        {
            return user == Owner;
        }

        public bool IsAllowedToEdit(User user)
        {
            return IsOwner(user);
        }
    }

    public class CreateResub
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditResub : EditModel
    {
        public string Description
        {
            get => (string) GetField(nameof(Description));
            set => SetField(nameof(Description), value);
        }

        [JsonPropertyName("new_owner_username")]
        public string NewOwnerUsername { get; set; }
    }
}