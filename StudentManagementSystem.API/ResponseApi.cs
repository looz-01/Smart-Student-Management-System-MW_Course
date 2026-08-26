namespace StudentManagementSystem.API
{
    public class ResponseApi<TData>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Error { get; set; }
        public TData? Value { get; set; }

        private static ResponseApi<TData> Create(bool isSuccess, int statusCode, string message,
            TData? value = default, object? error = null)
        {
            return new ResponseApi<TData>
            {
                StatusCode = statusCode,
                Message = message,
                IsSuccess = isSuccess,
                Value = value,
                Error = error
            };
        }

        public static ResponseApi<TData> BadRequest(string message, object? error = null)
            => Create(false, 400, message, error: error);

        public static ResponseApi<TData> Unauthorized(string message = "Unauthorized.")
            => Create(false, 401, message);

        public static ResponseApi<TData> NotFound(string message = "The Resource Not Found")
            => Create(false, 404, message);

        public static ResponseApi<TData> Conflict(string message)
            => Create(false, 409, message);

        public static ResponseApi<TData> Ok(TData? data, string message)
            => Create(true, 200, message, data);

        public static ResponseApi<TData> CreatedAt(TData? data, string message)
            => Create(true, 201, message, data);

        public static ResponseApi<TData> NoContent(TData? data, string message = "The Task Went Perfect")
            => Create(true, 204, message, data);
    }
}