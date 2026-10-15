using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.ApisResponses
{
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public T? Data { get; set; }
        public string? Error { get; set; }

        public APIResponse(bool success, string message, T? data, string error = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Error = error;
        }
        public static APIResponse<T> SuccessResponse(T data, string message = "Request Success")
        {
            return new APIResponse<T>(true, message, data);
        }

        public static APIResponse<T> ErrorResponse(string message = "Request Failed", string error = null)
        {
            return new APIResponse<T>(false, message, default, error);
        }

    }
}
