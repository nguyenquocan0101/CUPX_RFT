
namespace Services.Dtos.OrderCache
{
    public class CacheOrderDto
    {
        public CacheOrderDto() { } // Parameterless constructor for deserialization

        public CacheOrderDto(string orderId, IEnumerable<string> productIdList)
        {
            OrderId = orderId;
            Products = productIdList.Select(productId => new CacheProductDto
            {
                ProductId = productId,
                FinishTime = null,
                FailTime = null,
            }).ToList();
            CreatedTime = DateTime.Now; //based on system -> maybe we need to show this for client
            IsFault = false;
        }
        public string OrderId { get; set; }
        public List<CacheProductDto>  Products { get; set; }

        public DateTime CreatedTime { get; set; }
        public bool IsFault { get; set; }
    }

    public class CacheProductDto
    {
        public string ProductId { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? FailTime { get; set; }
    }
}
