using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Domain.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }

        public ApiResponse()
        {
            Success = true;
            Message = string.Empty;
            Data = default(T);
            Error = null;
        }

    }
}
