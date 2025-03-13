namespace TrixyWebapp.ViewModels
{
    public class StockCandleChartVM
    {
        public string? Symbol { get; set; }
        public string? CompanyName { get; set; }
        public string? Companylogo { get; set; }
        public decimal? Currentprice { get; set; }
        public decimal? High { get; set; }
        public decimal? low { get; set; }
        public decimal? close { get; set; }
        public decimal? volume { get; set; }
    }
}
