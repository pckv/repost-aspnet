using System;
using System.Text.Json.Serialization;

namespace RepostAspNet.Models
{
    public class Resub
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
    }
    
    public class CreateResub
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditResub
    {
        public string Description { get; set; }
    }
}