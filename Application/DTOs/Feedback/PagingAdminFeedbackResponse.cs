using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Feedback
{
    public class PagingAdminFeedbackResponse : PagingResponse
    {
        public List<AdminFeedbackResponse> AdminFeedbackResponses { get; set; }
    }
}
