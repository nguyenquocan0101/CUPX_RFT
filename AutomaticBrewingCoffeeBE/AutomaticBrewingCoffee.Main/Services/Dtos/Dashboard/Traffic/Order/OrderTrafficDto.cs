namespace Services.Dtos.Dashboard.Traffic.Order;

public class OrderTrafficDto
{
    public int WindowDays { get; set; }

    public string WindowDayType
    {
        get
        {
            return WindowDays switch
            {
                0 => "Toàn thời gian",
                1 => "Ngày",
                7 => "Tuần",
                3 or 4 => "Nửa tuần",
                14 or 15 => "Nửa tháng",
                30 or 31 => "Tháng",
                180 or 182 => "Nửa năm",
                365 or 366 => "Năm",
                _ => $"{WindowDays} ngày"
            };
        }
    }

    public List<OrderTrafficByShiftDto> TrafficByShift { get; set; } = [];
    public int TotalCurrentPeriod { get; set; }
    public int TotalPreviousPeriod { get; set; }

    public double GrowthRate
    {
        get
        {
            if (TotalPreviousPeriod == 0)
            {
                if (TotalCurrentPeriod == 0) return 0d;
                return 100d;
            }

            return Math.Round(
                (double)(TotalCurrentPeriod - TotalPreviousPeriod) / TotalPreviousPeriod * 100d,
                2
            );
        }
    }
}