using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAMTEnterpriseAssistant
{
    public class ProfileRequest : RequestModel
    {
        public override bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(nodeid))
                validationErrors.Add("Nodeid is required.");

            if (authProtocol != 0 && authProtocol != 2)
                validationErrors.Add("AuthProtocol must be 0 or 2.");

            return validationErrors.Count == 0;
        }
    }
}