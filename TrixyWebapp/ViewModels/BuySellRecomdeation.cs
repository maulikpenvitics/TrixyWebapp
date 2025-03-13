using Microsoft.ML.Data;
using System.Security.Cryptography;

namespace TrixyWebapp.ViewModels
{
    public class BuySellRecomdeation
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double SMA { get; set; }
        public double LMA { get; set; }
       // Relative Strength Index(RSI) Strategy
        public double RSI { get; set; }
        //Bollinger Bands Strategy
        public double MiddleBand { get; set; }
        public double UpperBand { get; set; }
        public double LowerBand { get; set; }
        //Mean Reversion Strategy
        public double MeanPrice { get; set; }
        //Volume-Weighted Average Price (VWAP) Strategy
        public double Volume { get; set; }
        public double VWAP { get; set; }

        //
        public double MACD { get; set; }
        public double TradeSignal { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
    }
    public class StockSentiment
    {
        public string? NewsHeadline { get; set; }
        public float SentimentScore { get; set; } // 0 (negative) to 1 (positive)
        public string? TradeSignal { get; set; } // BUY, SELL, or HOLD
    }
    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }
    public class BuySellForMAC
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double SMA { get; set; }
        public double LMA { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD
      
    }

    public class BuySellForRSI
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        //Bollinger Bands Strategy
        public double RSI { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
    }

    public class BuySellforBollingerBands
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double MiddleBand { get; set; }
        public double UpperBand { get; set; }
        public double LowerBand { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
    }

    public class BuySellforMeanReversion
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
        public double MeanPrice { get; set; }
    }

    public class BuySellforVWAP
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double VWAP { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
        
    }
    public class BuySellforMACD
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
        public double MACD { get; set; }
        public double TradeSignal { get; set; }
        public string Signal { get; set; } = "HOLD";// BUY, SELL, or HOLD 
    }
   
}
