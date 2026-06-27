using FluentValidation;
using ITIGraduationProject.Application.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ITIGraduationProject.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.ContentType = "application/json";

            var response = new Response<string>
            {
                Succeeded = false
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.Message = "Unauthorized access.";
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ValidationException validationException:
                    response.Message = validationException.Message;
                    response.StatusCode = HttpStatusCode.UnprocessableEntity;
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    break;

                case KeyNotFoundException:
                    response.Message = exception.Message;
                    response.StatusCode = HttpStatusCode.NotFound;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case DbUpdateException:
                    response.Message = "Database operation failed.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.Message = "An internal server error occurred.";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(result);
        }
    }
}
