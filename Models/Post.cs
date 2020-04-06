using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    [Table("posts")]
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }

        [JsonIgnore] public Resub ParentResub { get; set; }
        [JsonIgnore] public User Author { get; set; }

        [JsonPropertyName("parent_resub_name")]
        public string ParentResubName => ParentResub.Name;

        [JsonPropertyName("author_username")] public string AuthorUsername => Author.Username;

        // TODO: Implement votes 
        public int Votes => 0;
    }

    public class CreatePost
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }

    public class EditPost : EditModel
    {
        public string Title
        {
            get => (string) GetField(nameof(Title));
            set => SetField(nameof(Title), value);
        }

        public string Content
        {
            get => (string) GetField(nameof(Content));
            set => SetField(nameof(Content), value);
        }

        public string Url
        {
            get => (string) GetField(nameof(Url));
            set => SetField(nameof(Url), value);
        }
    }
}