using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public static class GenrateBuysellsignal
    {
        // 1 → BUY
        //-1 → SELL
        //0 → HOLD
        public static string GenerateSignalsforMovingAverageCrossover(List<Historical_Data> stockData, int shortTerm, int longTerm)
        {
            var signal = "HOLD";
            int returnresult = 0;
            var stockdata = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).ToList();
            StockCalculator.CalculateMovingAverages(stockdata, shortTerm, longTerm);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                if (result.SMA > result.LMA)
                {
                    signal = "BUY";
                    returnresult = 1;
                }
                else if (result.SMA < result.LMA)
                {
                    signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    signal = "HOLD";
                    returnresult = 0;
                }
            }
            return signal;
        }

        public static string genratesignalsforRSI(List<Historical_Data> stockData)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockdata = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).ToList();
            int rsiPeriod = 14; // RSI period (default 14 days)
            StockCalculator.CalculateRSI(stockdata, rsiPeriod);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                if (result.RSI < 30)
                {
                    Signal = "BUY";
                    returnresult = 1;
                }
                else if (result.RSI > 70)
                {
                    Signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    Signal = "HOLD";
                    returnresult = 0;
                }
            }

            return Signal;
        }
        //BollingerBands strategy
        public static string GenerateBuySellSignalsForBB(List<Historical_Data> stockData)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockdata = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).ToList();
            int rsiPeriod = 14; // RSI period (default 14 days)
            StockCalculator.CalculateBollingerBands(stockdata, rsiPeriod);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                if (result.Close <= result.LowerBand)
                {
                    Signal = "BUY";
                    returnresult = 1;
                }
                else if (result.Close >= result.UpperBand)
                {
                    Signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    Signal = "HOLD";
                    returnresult = 0;
                }
            }
            return Signal;
        }

        public static string CalculateMACD(List<Historical_Data> stockData, int shortEmaPeriod, int longEmaPeriod, int signalPeriod)
        {
            var signal = "HOLD";
            int returnresult = 0;
            var stockPrices = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).ToList();

            List<double> closePrices = stockPrices.Select(s => s.Close).ToList();

            // Calculate Short-term and Long-term EMAs
            List<double> shortEma = CalculateEMA(closePrices, shortEmaPeriod);
            List<double> longEma = CalculateEMA(closePrices, longEmaPeriod);

            // Calculate MACD Line
            for (int i = 0; i < stockPrices.Count; i++)
            {
                stockPrices[i].MACD = shortEma[i] - longEma[i];
            }

            // Calculate Signal Line (9-day EMA of MACD)
            List<double> macdValues = stockPrices.Select(s => s.MACD).ToList();
            List<double> signalLine = CalculateEMA(macdValues, signalPeriod);

            // Assign Signal Line and generate trading signals
            for (int i = 0; i < stockPrices.Count; i++)
            {
                stockPrices[i].TradeSignal = signalLine[i];
            }
            var result = stockPrices.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                if (result.MACD < result.TradeSignal)
                {
                    signal = "BUY";
                    returnresult = 1;
                }
                else if (result.MACD > result.TradeSignal)
                {
                    signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    signal = "HOLD";
                    returnresult = 0;
                }
            }
            return signal;
        }
        public static string CalculateMeanReversion(List<Historical_Data> stockData, int period, double threshold)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockPrices = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).ToList();


            period = 30; // Mean price calculation period
            threshold = 0.1;
            for (int i = 0; i < stockPrices.Count; i++)
            {
                if (i >= period - 1)
                {
                    var window = stockPrices.Skip(i - period + 1).Take(period).Select(s => s.Close).ToList();
                    double mean = window.Average();
                    stockPrices[i].MeanPrice = mean;
                }
            }

            var result = stockPrices.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                double deviation = (result.Close - result.MeanPrice) / result.MeanPrice;
                if (deviation <= -threshold)
                {
                    Signal = "BUY";
                    returnresult = 1;
                }

                else if (deviation >= threshold)
                {
                    Signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    Signal = "HOLD";
                    returnresult = 0;
                }

            }
            return Signal;
        }

        public static string CalculateVWAP(List<Historical_Data> stockData)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockPrices = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
                Volume = (double)x.Volume,
            }).ToList();
            var result = stockPrices.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                double cumulativePV = result.Close * result.Volume;  // Cumulative Price * Volume
                double cumulativeVolume = result.Volume;// Cumulative Volume
                if (cumulativeVolume != 0)
                {
                    result.VWAP = cumulativePV / cumulativeVolume;
                }
                if (result.Close < result.VWAP)
                {
                    Signal = "BUY";
                    returnresult = 1;
                }
                else if (result.Close > result.VWAP)
                {
                    Signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    Signal = "HOLD";
                    returnresult = 0;
                }
            }
            return Signal;
        }

        public static List<double> CalculateEMA(List<double> prices, int period)
        {
            List<double> ema = new List<double>();
            double multiplier = 2.0 / (period + 1);
            double emaPrev = prices.Take(period).Average(); // Initial EMA is the average of first 'period' prices
            ema.AddRange(Enumerable.Repeat(0.0, period - 1)); // Fill with zeros for initial period
            ema.Add(emaPrev);

            for (int i = period; i < prices.Count; i++)
            {
                emaPrev = (prices[i] - emaPrev) * multiplier + emaPrev;
                ema.Add(emaPrev);
            }

            return ema;
        }

        public static string GetCombinationsignal(AdminSettings userSettings, List<Historical_Data> stockData)
        {
            var weightedstrategy = userSettings.StrategyWeighted;
            var thresold = userSettings.Threshold;
            var MovingAverageCrossover = GenerateSignalsforMovingAverageCrossover(stockData, 10, 50);

            var RSI = genratesignalsforRSI(stockData);
            var bollingerBands = GenerateBuySellSignalsForBB(stockData);
            var MACD = CalculateMACD(stockData, 12, 26, 9);
            var MeanReversion = CalculateMeanReversion(stockData, 10, 0.1);
            var VWAP = CalculateVWAP(stockData);

            double macweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "Moving_Average")?.Weight ?? 0;
            double RSIweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "RSI")?.Weight ?? 0;
            double Bollinger_Bandsweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "Bollinger_Bands")?.Weight ?? 0;
            double Mean_Reversionweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "Mean_Reversion")?.Weight ?? 0;
            double VWAPweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "VWAP")?.Weight ?? 0;
            double MACDweight = weightedstrategy?.FirstOrDefault(x => x.Strategy == "MACD")?.Weight ?? 0;

            List<string> signals = new List<string> { MovingAverageCrossover , RSI , bollingerBands , MACD,
            MeanReversion,VWAP};
            List<double> weights = new List<double> { macweight, RSIweight, Bollinger_Bandsweight,
            MACDweight,Mean_Reversionweight,VWAPweight};
            var finalsignal = GetCombinationStrategyDecision(signals, weights, thresold);

            return finalsignal;
        }

        public static string GetCombinationStrategyDecision(List<string> signals, List<double> weights, double thresold)
        {
            if (signals.Count != weights.Count)
            {
                throw new ArgumentException("Signals and weights must have the same length.");
            }

            Dictionary<string, int> signalMapping = new Dictionary<string, int>
            {
                 { "BUY", 1 },
                 { "SELL", -1 },
                 { "HOLD", 0 }
            };

            double totalWeight = Math.Round(weights.Sum(), 2);
       
            double weightedSum = 0;
            for (int i = 0; i < signals.Count; i++)
            {
                if (signalMapping.ContainsKey(signals[i]))
                {
                    weightedSum += signalMapping[signals[i]] * weights[i];
                    //totalWeight += weights[i];
                }
            }
            double finalScore = totalWeight > 0 ? (weightedSum / totalWeight) * 100 : 0;

            if (finalScore > thresold)
            {
                return "BUY";
            }
            else
            {
                return "SELL";
            }

        }
    }
    public static class StockCalculator
    {
        public static void CalculateMovingAverages(List<BuySellRecomdeation> stockPrices, int shortWindow, int longWindow)
        {
            for (int i = 0; i < stockPrices.Count; i++)
            {
                if (i >= shortWindow - 1)
                    stockPrices[i].SMA = (double)stockPrices.Skip(i - shortWindow + 1).Take(shortWindow).Average(s => s.Close);

                if (i >= longWindow - 1)
                    stockPrices[i].LMA = (double)stockPrices.Skip(i - longWindow + 1).Take(longWindow).Average(s => s.Close);
            }
        }

        public static void CalculateRSI(List<BuySellRecomdeation> stockPrices, int period)
        {
            double avgGain = 0, avgLoss = 0;
            List<double> gains = new List<double>();
            List<double> losses = new List<double>();
            for (int i = 1; i < stockPrices.Count; i++)
            {
                double change = stockPrices[i].Close - stockPrices[i - 1].Close;
                double gain = Math.Max(change, 0);
                double loss = Math.Max(-change, 0);

                gains.Add(gain);
                losses.Add(loss);

                if (i == period)
                {
                    // Calculate initial average gain and loss
                    avgGain = gains.Take(period).Average();
                    avgLoss = losses.Take(period).Average();
                }
                else if (i > period)
                {
                    // Smoothed moving average of gain and loss
                    avgGain = ((avgGain * (period - 1)) + gain) / period;
                    avgLoss = ((avgLoss * (period - 1)) + loss) / period;
                }

                if (i >= period)
                {
                    double rs = (avgLoss == 0) ? 100 : avgGain / avgLoss;
                    stockPrices[i].RSI = 100 - (100 / (1 + rs));
                }
            }
        }

        public static void CalculateBollingerBands(List<BuySellRecomdeation> stockPrices, int period)
        {
            for (int i = 0; i < stockPrices.Count; i++)
            {
                if (i >= period - 1)
                {
                    var window = stockPrices.Skip(i - period + 1).Take(period).Select(s => s.Close).ToList();
                    double sma = window.Average();
                    double stdDev = Math.Sqrt(window.Sum(x => Math.Pow(x - sma, 2)) / period);

                    stockPrices[i].MiddleBand = sma;
                    stockPrices[i].UpperBand = sma + (2 * stdDev);
                    stockPrices[i].LowerBand = sma - (2 * stdDev);
                }
            }
        }


    }
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
}
