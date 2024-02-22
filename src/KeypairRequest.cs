using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenAMTEnterpriseAssistant
{
    public class KeypairRequest : RequestModel
    {
        public string DERKey { get; set; }
        public string keyInstanceId { get; set; }

        public override bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(nodeid))
                validationErrors.Add("Nodeid is required.");

            if (authProtocol != 0)
                validationErrors.Add("AuthProtocol must be 0");

            if (string.IsNullOrWhiteSpace(DERKey))
                validationErrors.Add("DERKey is required.");

            if (string.IsNullOrWhiteSpace(keyInstanceId))
                validationErrors.Add("KeyInstanceId is required.");

            return validationErrors.Count == 0;
        }
    }
}
