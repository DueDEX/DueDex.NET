namespace DueDex.Models
{
    public class Instrument
    {
        /// <summary>
        /// The instrument id
        /// </summary>
        public string InstrumentId { get; set; }
        /// <summary>
        /// Status of the market
        /// </summary>
        public MarketStatus Status { get; set; }
        /// <summary>
        /// The base currency symbol for display purpose only
        /// </summary>
        public string BaseCurrencySymbol { get; set; }
        /// <summary>
        /// The quote currency symbol for display purpose only
        /// </summary>
        public string QuoteCurrencySymbol { get; set; }
        /// <summary>
        /// The position currency in the market
        /// </summary>
        public CurrencyInfo PositionCurrency { get; set; }
        /// <summary>
        /// The settlement currency in the market
        /// </summary>
        public CurrencyInfo SettlementCurrency { get; set; }
        /// <summary>
        /// The expiration type
        /// </summary>
        public ExpirationType ExpirationType { get; set; }
        /// <summary>
        /// The method used for calculating the mark price
        /// </summary>
        public MarkMethod MarkMethod { get; set; }
        /// <summary>
        /// Contract value multiplier
        /// </summary>
        public decimal Multiplier { get; set; }
        /// <summary>
        /// The fixed exchange rate for quanto contracts
        /// </summary>
        public decimal QuantoExchangeRate { get; set; }
        /// <summary>
        /// Is this an inverse market
        /// </summary>
        public bool IsInverse { get; set; }
        /// <summary>
        /// Is this a quanto market
        /// </summary>
        public bool IsQuanto { get; set; }
        /// <summary>
        /// Is ADL enabled
        /// </summary>
        public bool AdlEnabled { get; set; }
        /// <summary>
        /// The maker fee
        /// </summary>
        public decimal MakerFee { get; set; }
        /// <summary>
        /// The taker fee
        /// </summary>
        public decimal TakerFee { get; set; }
        /// <summary>
        /// The lot size
        /// </summary>
        public long LotSize { get; set; }
        /// <summary>
        /// The maximum size of an order
        /// </summary>
        public long MaxSize { get; set; }
        /// <summary>
        /// Number of decimals in the price
        /// </summary>
        public int PricePrecision { get; set; }
        /// <summary>
        /// The minimum price increment
        /// </summary>
        public decimal TickSize { get; set; }
        /// <summary>
        /// The maximum price of an order
        /// </summary>
        public decimal MaxPrice { get; set; }
        /// <summary>
        /// The base mininum initial margin requirement
        /// </summary>
        public decimal MinInitMargin { get; set; }
        /// <summary>
        /// The base maintenance margin requirement
        /// </summary>
        public decimal MaintMargin { get; set; }
        /// <summary>
        /// The base risk limit in settlement currency
        /// </summary>
        public decimal BaseRiskLimit { get; set; }
        /// <summary>
        /// The risk step in settlement currency
        /// </summary>
        public decimal RiskStep { get; set; }
        /// <summary>
        /// The inclusive maximum risk limit in settlement currency
        /// </summary>
        public decimal MaxRiskLimit { get; set; }
    }
}
