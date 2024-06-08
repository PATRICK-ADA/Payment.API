
namespace API.BidService.Core.ApiResponse
{

    public class ApiResponse<T>
    {
        public ApiResponse(string msg, object? T, int status = 200)
        {
            Succeeded = status >= 200 && status < 300;
            Msg = msg;
            Data = T;
            Status = status;
        }

        public ApiResponse(string msg, int status = 200)
        {
            Msg = msg;
            Status = status;
        }
        public bool Succeeded { get; set; }
        public string Msg { get; set; }
        public object? Data { get; set; }
        public int Status { get; set; }
    }

    public class SuccessApiResponse<T> : ApiResponse<object>
    {
        public SuccessApiResponse(string msg, object? T, int status = 200) : base(msg, T, status)
        {
        }
    }

    public class FailureApiResponse : ApiResponse<object>
    {
        public FailureApiResponse(string msg, object? data, int status = 400) : base(msg, data, status)
        {
        }
    }


}
