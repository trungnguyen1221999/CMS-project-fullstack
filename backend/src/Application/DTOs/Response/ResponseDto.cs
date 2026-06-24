using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Response
{
    public class ResponseDto<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public T? Data { get; set; }
    }
}
