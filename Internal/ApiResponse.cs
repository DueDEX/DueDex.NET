namespace DueDex.Internal
{
    internal class ApiResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public ApiResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    internal class ApiResponse<T>
    {
        public int Code { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public ApiResponse(int code, T data, string message)
        {
            Code = code;
            Data = data;
            Message = message;
        }
    }
}