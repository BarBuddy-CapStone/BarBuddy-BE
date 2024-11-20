using Domain.Constants;
using Domain.CustomException;
using Domain.IRepository;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Quartz
{
    public class TokenJob : IJob
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TokenJob> _logger;

        public TokenJob(IUnitOfWork unitOfWork, ILogger<TokenJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dateTime = DateTime.Now;
                var getAllToken = _unitOfWork.TokenRepository
                                                .Get(filter: x => x.IsRevoked == PrefixKeyConstant.FALSE && 
                                                                  x.IsUsed == PrefixKeyConstant.FALSE);
                foreach (var token in getAllToken)
                {
                    if(token.Expires <= dateTime)
                    {
                        token.IsUsed = PrefixKeyConstant.TRUE;
                        await _unitOfWork.TokenRepository.UpdateRangeAsync(token);
                        await Task.Delay(10);
                        await _unitOfWork.SaveAsync();
                    }
                }
            }
            catch
            {
                _logger.LogInformation("Lỗi hệ thống Token Job");
            }
        }
    }
}
