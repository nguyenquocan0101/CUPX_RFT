namespace Services.Dtos.Dashboard.Traffic.HourlyPeak;

public class HourlyPeakDto
{
    public int WindowStartHour { get; set; } = 0;
    public int WindowEndHour { get; set; } = 0;
    public List<HourlyPointDto> Points { get; set; } = new List<HourlyPointDto>();
    public HourlyPointDto? Peak { get; set; }

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
}