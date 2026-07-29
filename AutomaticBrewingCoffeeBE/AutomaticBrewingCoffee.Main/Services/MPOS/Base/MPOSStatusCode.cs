namespace Services.MPOS.Base
{
    public class MPOSStatusCode
    {
        public const int Ok = 200;
        public const int BadRequest = 11002; //sai tham số đầu vào
        public const int UnknowedError = 1004; //lỗi không xác định, cần liên hệ MPOS
        public const int NotFoundOrder = 61116; //không tìm thấy hóa đơn dựa trên Id
    }
}
