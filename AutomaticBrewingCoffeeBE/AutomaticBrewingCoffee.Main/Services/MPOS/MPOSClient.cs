using Microsoft.Extensions.Options;
using Services.MPOS.Base;
using Services.Utils;
using System.Text;
using System.Text.Json;
using Services.MPOS.Data;

namespace Services.MPOS
{
    public class MPOSClient
    {
        private readonly HttpClient _httpClient;
        private readonly MPOSMerchant _mPOSMerchant;
        private readonly string _orderUrl;
        private readonly string _transactionUrl;

        public MPOSClient(HttpClient httpClient, IOptions<MPOSMerchant> options)
        {
            _httpClient = httpClient;
            _mPOSMerchant = options.Value;
            _orderUrl = $"{_mPOSMerchant.DevDomain}/orderQR";
            _transactionUrl = $"{_mPOSMerchant.DevDomain}/transaction";
        }

        #region QR Payment functions

        public async Task<CreateQRResponseData> CreateQRPayment(string orderId, string amount,
            string description = "Thanh toan CUPX")
        {
            try
            {
                var data = new CreateQRRequestData(nameof(ServiceName.CREATE_QR), orderId, _mPOSMerchant.Muid, amount,
                    nameof(QrType.VAQR), description);
                //Console.WriteLine(JsonSerializer.Serialize(data));
                var requestData = new Request
                {
                    MerchantId = _mPOSMerchant.MerchantId,
                    ReqData = AES128ECB.EncryptAES128ECB(JsonSerializer.Serialize(data), _mPOSMerchant.Secretkey)
                };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_orderUrl, content);

                //get responseData
                var responseString = await response.Content.ReadAsStringAsync()!;
                //var demo = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseDataObj = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseData =
                    DecodeData<CreateQRResponseData>(responseDataObj.ResData);
                return responseData;
            }
            catch (Exception)
            {
                return new CreateQRResponseData();
            }
        }

        public async Task<CancelQRResponseData> CancelQRPayment(string orderId, string amount)
        {
            try
            {
                var data = new CancelQRRequestData(nameof(ServiceName.REMOVE_QR), orderId, _mPOSMerchant.Muid, amount);
                //Console.WriteLine(JsonSerializer.Serialize(data));
                var requestData = new Request
                {
                    MerchantId = _mPOSMerchant.MerchantId,
                    ReqData = AES128ECB.EncryptAES128ECB(JsonSerializer.Serialize(data), _mPOSMerchant.Secretkey)
                };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_orderUrl, content);

                //get responseData
                var responseString = await response.Content.ReadAsStringAsync()!;
                //var demo = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseDataObj = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseData = DecodeData<CancelQRResponseData>(responseDataObj.ResData);

                //set metatdata
                responseData.ResCode = responseDataObj.ResCode;
                responseData.Message = responseDataObj.Message;
                return responseData;
            }
            catch (Exception)
            {
                return new CancelQRResponseData();
            }
        }

        public async Task<QRStatusResponseData> GetQRPaymentStatus(string orderId, string amount)
        {
            try
            {
                var data = new QRStatusQueryData(nameof(ServiceName.QR_GET_TRANSACTION_STATUS), orderId,
                    _mPOSMerchant.Muid, amount);
                //Console.WriteLine(JsonSerializer.Serialize(data));
                var requestData = new Request
                {
                    MerchantId = _mPOSMerchant.MerchantId,
                    ReqData = AES128ECB.EncryptAES128ECB(JsonSerializer.Serialize(data), _mPOSMerchant.Secretkey)
                };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_orderUrl, content);

                //get responseData
                var responseString = await response.Content.ReadAsStringAsync()!;
                //var demo = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseDataObj = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseData = DecodeData<QRStatusResponseData>(responseDataObj.ResData);
                //set metatdata
                responseData.ResCode = responseDataObj.ResCode;
                responseData.Message = responseDataObj.Message;
                return responseData;
            }
            catch (Exception)
            {
                return new QRStatusResponseData();
            }
        }

        public TransactionStatusResponseData ParsePaymentCallbackRequest(MPOSCallbackRequest callbackRequest)
        {
            try
            {
                var callbackRequestData = DecodeData<TransactionStatusResponseData>(callbackRequest.ReqData);
                return callbackRequestData;
            }
            catch (Exception)
            {
                return new TransactionStatusResponseData();
            }
        }

        #endregion

        /// <summary>
        /// HOÀN gd (sử dụng khi trạng thái gd là 104) thì cũng mất thời gian không ngay lập tức như HỦY được. Sau khi MPOS nhận lệnh HOÀN từ API sẽ gửi lệnh sang Bank thanh toán > sang Bank chủ thẻ > chủ thẻ nhận được tiền hoàn.
        ///Khoảng thời gian từ bank thanh toán sang bank chủ thẻ có thể mất khoảng 5-7 ngày làm việc.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<RefundPaidResponseData> RefundPaidPayment(string orderId, long amount)
        {
            try
            {
                var data = new RefundPaidRequestData(nameof(ServiceName.REFUND_TRANSACTION), orderId, amount);
                //Console.WriteLine(JsonSerializer.Serialize(data));
                var requestData = new Request
                {
                    MerchantId = _mPOSMerchant.MerchantId,
                    ReqData = AES128ECB.EncryptAES128ECB(JsonSerializer.Serialize(data), _mPOSMerchant.Secretkey)
                };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_transactionUrl, content);

                //get responseData
                var responseString = await response.Content.ReadAsStringAsync()!;
                //var demo = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseDataObj = JsonSerializer.Deserialize<Response>(responseString)!;
                var responseData = DecodeData<RefundPaidResponseData>(responseDataObj.ResData);

                //set metatdata
                responseData.ResCode = responseDataObj.ResCode;
                responseData.Message = responseDataObj.Message;
                return responseData;
            }
            catch (Exception)
            {
                return new RefundPaidResponseData();
            }
        }

        private T DecodeData<T>(string data)
        {
            //decode data based on AES128 encryption
            var decodedData = AES128ECB.DecryptAES128ECB(data, _mPOSMerchant.Secretkey);
            T decodedResponseData = JsonSerializer.Deserialize<T>(decodedData) ??
                                    throw new ArgumentException("decode data failed");
            return decodedResponseData;
        }
    }
}