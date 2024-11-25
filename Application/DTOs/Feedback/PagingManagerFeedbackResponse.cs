using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Feedback
{
    public class PagingManagerFeedbackResponse : PagingResponse
    {
        public List<ManagerFeedbackResponse> ManagerFeedbackResponses { get; set; }
    }
}
