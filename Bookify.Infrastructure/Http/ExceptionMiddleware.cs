using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Http
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

        /// <summary>
        /// Invoke Exception Middleware
        /// </summary>
        /// <param name="context">Http context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var errorMessage = await HandleErrorAndSendProperMessage(context, exception);
                _logger.LogError(exception, errorMessage);
            }
        }

        /// <summary>
        /// Handle Global error and throw proper Http exception with status code
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="exception">Exception</param>
        /// <returns>error message</returns>
        /// 
        //TODO after test return keyword static:  remove static for test 
        private async Task<string> HandleErrorAndSendProperMessage(HttpContext context, Exception exception)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError; //Default is 500 Internal Server Error
            ResponseErrorModel errorModel = null;
            switch (exception.GetType().ToString())
            {
                case ExceptionName.DbUpdateException:
                    if (exception.GetBaseException() is SqlException sqlException && sqlException.Errors.Count > 0)
                    {
                        switch (sqlException.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                {
                                    statusCode = (int)HttpStatusCode.BadRequest;
                                    errorModel = new ResponseErrorModel($"{ nameof(ExceptionMiddleware)}_{ExceptionName.ForeignKeyViolation}", sqlException.Errors[0].Message);
                                }
                                break;
                            default:
                                {
                                    errorModel = new ResponseErrorModel($"{ nameof(ExceptionMiddleware)}_{ExceptionName.SqlException}", sqlException.Errors[0].Message);
                                }
                                break;
                        }
                    }
                    else
                    {
                        errorModel = new ResponseErrorModel($"{nameof(ExceptionMiddleware) }_{ExceptionName.NoneSqlMiddlewareExceptionKey}", exception.Message);
                    }
                    break;
                case ExceptionName.HttpResponseException:
                    // this check is meaningless. added to avoid resharper warning
                    if (exception is HttpResponseException httpResponseException)
                    {
                        statusCode = (int)httpResponseException.StatusCode;
                        errorModel = new ResponseErrorModel(httpResponseException.Error?.ErrorMessages?.ToArray());
                    }
                    break;
                default:
                    {
                        errorModel = new ResponseErrorModel($"{nameof(ExceptionMiddleware)}_{ExceptionName.DefaultMiddlewareExceptionKey}", exception.Message);
                        _logger.LogError(exception, exception.Message);
                    }
                    break;
            }
            var serializerSetting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var jsonResult = JsonConvert.SerializeObject(errorModel, serializerSetting);
            await CreateContextResponse(context, statusCode, jsonResult);

            return string.Join(",", errorModel.ErrorMessages);
        }

        /// <summary>
        /// Put message in Http context
        /// </summary>        
        /// <param name="context">http context</param>
        /// <param name="statusCode">http status code</param>
        /// <param name="errorMessageSerialized">error message serialized</param>
        /// <returns></returns>
        private static async Task CreateContextResponse(HttpContext context, int statusCode, string errorMessageSerialized)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(errorMessageSerialized);
        }
    }
}
