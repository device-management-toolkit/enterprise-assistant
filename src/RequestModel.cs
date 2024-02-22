using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAMTEnterpriseAssistant
{
    public abstract class RequestModel
    {
        public string nodeid { get; set; }
        public string domain { get; set; } = "";
        public string reqid { get; set; } = "";
        public int authProtocol { get; set; }
        public string osname { get; set; }
        public string devname { get; set; }
        public int icon { get; set; }
        public string ver { get; set; } = "";

        // Converts the object to a dictionary
        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var prop in this.GetType().GetProperties())
            {
                var value = prop.GetValue(this, null);
                dictionary[prop.Name] = value;
            }
            return dictionary;
        }

        public abstract bool IsValid(out List<string> validationErrors);
    }
}
