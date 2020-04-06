using System.Collections.Generic;

namespace RepostAspNet.Models
{
    public class EditModel
    {
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public bool IsFieldSet(string field)
        {
            return _fields.ContainsKey(field);
        }

        public object GetField(string field)
        {
            return _fields[field];
        }

        protected void SetField(string field, object value)
        {
            _fields.Add(field, value);
        }
    }
}