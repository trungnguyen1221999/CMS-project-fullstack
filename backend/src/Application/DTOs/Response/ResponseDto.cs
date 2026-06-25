using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Response
{
    public class WriteResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ReadResponseDto<T> : WriteResponseDto
    {
        public T? Data { get; set; }
    }
}
