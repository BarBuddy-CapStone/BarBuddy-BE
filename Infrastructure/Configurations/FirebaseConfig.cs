using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class FirebaseConfig
    {
        public string ApiKey { get; set; }
        public string Storage { get; set; }
        public string AuthEmail { get; set; }
        public string AuthPassword { get; set; }
        public string ProjectId { get; set; }
        public string FcmServerKey { get; set; }
        public string CredentialFile { get; set; }
    }
}
