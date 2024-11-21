using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Tokens
{
    public class TokenResponse
    {
        public Guid TokenId { get; set; }
        public Guid AccountId { get; set; }
        public string Tokens { get; set; }
        public string AccessToken { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
    }
}
