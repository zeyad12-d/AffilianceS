namespace Affiliance_core.ApiHelper
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T Data { get; private set; }

        private ApiResponse() { }
        public static ApiResponse<T> CreateSuccess(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        public static ApiResponse<T> CreateFail(string message = "")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,

            };


    }
   }
}