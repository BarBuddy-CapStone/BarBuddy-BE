using Application.DTOs.BarTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBarTimeService
    {
        Task CreateBarTimeOfBar(Guid barId, List<BarTimeRequest> request);
        Task UpdateBarTimeOfBar(Guid barId, List<UpdateBarTimeRequest> request);
    }
}
