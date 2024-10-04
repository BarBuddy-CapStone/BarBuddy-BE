using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace BarBuddy_API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var endpoint = context.GetEndpoint();

                if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() != null)
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        throw new UnAuthorizedException("Bạn chưa đăng nhập.");
                    }

                    var authorizeData = endpoint.Metadata.GetMetadata<IAuthorizeData>();
                    if (authorizeData.Roles != null)
                    {
                        var roles = authorizeData.Roles.Split(',');
                        if (!roles.Any(role => context.User.IsInRole(role)))
                        {
                            throw new ForbbidenException("Bạn không có quyền truy cập vào tài nguyên này.");
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /*private bool HasAccessPermission(HttpContext context)
        {
            if (context.User.IsInRole("Admin") || context.User.IsInRole("User"))
            {
                return true;
            }
            return false;
        }*/

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            string result = string.Empty;

            switch (exception)
            {
                case CustomException.InvalidDataException invalidDataEx:
                    code = HttpStatusCode.BadRequest;
                    result = invalidDataEx.Message;
                    break;
                case CustomException.DataNotFoundException dataNotFoundEx:
                    code = HttpStatusCode.NotFound;
                    result = dataNotFoundEx.Message;
                    break;
                case CustomException.DataExistException dataExistEx:
                    code = HttpStatusCode.Conflict;
                    result = dataExistEx.Message;
                    break;
                case CustomException.UnAuthorizedException unauthorizedEx:
                    code = HttpStatusCode.Unauthorized;
                    result = unauthorizedEx.Message;
                    break;
                case CustomException.ForbbidenException forbiddenEx:
                    code = HttpStatusCode.Forbidden;
                    result = forbiddenEx.Message;
                    break;
                case CustomException.InternalServerErrorException internalServerEx:
                    code = HttpStatusCode.InternalServerError;
                    result = internalServerEx.Message;
                    break;
                default:
                    _logger.LogError(exception, $"Unhandled exception: {exception.GetType().Name} - {exception.Message}");
                    result = "Đã xảy ra lỗi không xác định.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = result
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
