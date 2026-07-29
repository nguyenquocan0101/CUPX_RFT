using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public BaseResult()
        {

        }
        public BaseResult(int statusCode, string message, bool isSuccess)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccess = isSuccess;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    };
}
