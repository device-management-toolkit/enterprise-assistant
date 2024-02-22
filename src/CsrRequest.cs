using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAMTEnterpriseAssistant
{
    public class CsrRequest : RequestModel
    {
        public string signedcsr { get; set; }

        public override bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(nodeid))
                validationErrors.Add("Nodeid is required.");

            if (authProtocol != 0)
                validationErrors.Add("AuthProtocol must be 0");

            if (string.IsNullOrWhiteSpace(signedcsr))
                validationErrors.Add("Signedcsr is required.");

            return validationErrors.Count == 0;
        }
    }
}
