using Microsoft.AspNetCore.Http;

namespace Services.Base
{
    public class BaseResult<TRequestModel, TResponseModel> : BaseResult where TRequestModel : class where TResponseModel : class
    {
        public TRequestModel? Request { get; set; }
        public TResponseModel? Response { get; set; }
    }

    public class BaseResult<TResponseRequestModel> : BaseResult where TResponseRequestModel : class
    {
        public TResponseRequestModel? ResponseRequest { get; set; }
    };

    public class BaseResult
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public BaseResult()
        {

        }
        public BaseResult(int statusCode, string message, bool isSuccess)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccess = isSuccess;
        }
    };
}
