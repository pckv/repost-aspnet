using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    [Table("comments")]
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }

        [JsonIgnore] public Resub ParentResub { get; set; }
        [JsonIgnore] public Post ParentPost { get; set; }
        [JsonIgnore] public Comment ParentComment { get; set; }
        [JsonIgnore] public User Author { get; set; }
        [JsonIgnore] public List<CommentVote> Votes { get; set; }

        [JsonPropertyName("parent_resub_name")]
        public string ParentResubName => ParentResub.Name;

        [JsonPropertyName("author_username")] public string AuthorUsername => Author.Username;
        [JsonPropertyName("parent_post_id")] public int ParentPostId => ParentPost.Id;

        [JsonPropertyName("parent_comment_id")]
        public int? ParentCommentId => ParentComment?.Id;

        [JsonPropertyName("votes")] public int SumVotes => Votes.Sum(v => v.Vote);

        public bool IsAuthor(User user)
        {
            return user == Author;
        }

        public bool CanDelete(User user)
        {
            return IsAuthor(user) || ParentResub.IsOwner(user);
        }
    }

    public class CreateComment
    {
        public string Content { get; set; }
    }

    public class EditComment : EditModel
    {
        public string Content { get; set; }
    }

    [Table("comments_votes")]
    public class CommentVote
    {
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int Vote { get; set; }
    }
}