﻿using Repository.Models;
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
            }).OrderByDescending(x => x.Date).ToList();
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
                    signal = "N/A";
                    returnresult = 0;
                }
            }
            return signal;
        }

        public static string genratesignalsforRSI(List<Historical_Data> stockData, int rsiPeriod,int overbought,int oversold)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockdata = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).OrderByDescending(x => x.Date).ToList();
            
            StockCalculator.CalculateRSI(stockdata, rsiPeriod);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result != null)
            {
                if (result.RSI < oversold)
                {
                    Signal = "BUY";
                    returnresult = 1;
                }
                else if (result.RSI > overbought)
                {
                    Signal = "SELL";
                    returnresult = -1;
                }
                else
                {
                    Signal = "N/A";
                    returnresult = 0;
                }
            }

            return Signal;
        }
        //BollingerBands strategy
        public static string GenerateBuySellSignalsForBB(List<Historical_Data> stockData,int rsiPeriod)
        {
            string Signal = "HOLD";
            int returnresult = 0;
            var stockdata = stockData.Select(x => new BuySellRecomdeation
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).OrderByDescending(x => x.Date).ToList();
            //int rsiPeriod = 14; // RSI period (default 14 days)
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
                    Signal = "N/A";
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
                    signal = "N/A";
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
            }).OrderByDescending(x => x.Date).ToList();
            stockPrices = stockPrices.OrderByDescending(x => x.Date).ToList();

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
                    Signal = "N/A";
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
            }).OrderByDescending(x => x.Date).ToList();
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
                    Signal = "N/A";
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

        public static string GetCombinationsignal(AdminSettings userSettings, List<Historical_Data> stockData, List<UserStrategy> userStrategies)
        {
            var weightedstrategy = userSettings.StrategyWeighted;
            var thresold = userSettings.Threshold;
            List<string> signals = new List<string>();
            foreach (var item in userStrategies)
            {
                switch (item.StretagyName)
                {
                    case "Bollinger_Bands":
                        var bollingerBands = GenerateBuySellSignalsForBB(stockData, userSettings.RSIThresholds.RsiPeriod);
                        signals.Add(bollingerBands);
                        break;
                    case "MACD":
                        var MACD = CalculateMACD(stockData, userSettings.MACD_Settings.ShortEmaPeriod, userSettings.MACD_Settings.LongEmaPeriod,
            userSettings.MACD_Settings.SignalPeriod);
                        signals.Add(MACD);
                        break;
                    case "Mean_Reversion":
                        var MeanReversion = CalculateMeanReversion(stockData, userSettings.MeanReversion.Period, userSettings.MeanReversion.Threshold);
                        signals.Add(MeanReversion);
                        break;
                    case "Moving_Average":
                        var MovingAverageCrossover = GenerateSignalsforMovingAverageCrossover(stockData, userSettings.MovingAverage.SMA_Periods, userSettings.MovingAverage.LMA_Periods);
                        signals.Add(MovingAverageCrossover);
                        break;
                    case "RSI":
                        var RSI = genratesignalsforRSI(stockData, userSettings.RSIThresholds.RsiPeriod, userSettings.RSIThresholds.Overbought
                 ,userSettings.RSIThresholds.Oversold);
                        signals.Add(RSI);
                        break;
                    case "VWAP":
                        var VWAP = CalculateVWAP(stockData);
                        signals.Add(VWAP);
                        break;
                }
            }

            var finalsignal = GetCombinationStrategyDecision(signals, thresold);

            return finalsignal;
        }

        public static string GetCombinationStrategyDecision(List<string> signals, double thresold)
        {
           
            Dictionary<string, int> signalMapping = new Dictionary<string, int>
            {
                 { "BUY", 1 },
                 { "SELL", -1 },
                 { "N/A", 0 }
            };
            
            int weightedSum = 0;
            foreach (var signal in signals)
            {
                if (signalMapping.ContainsKey(signal))
                {
                    weightedSum += signalMapping[signal];
                }
            }
            int finalScore = weightedSum;

            if (finalScore >0)
            {
                return "BUY";
            }
            else if (finalScore<0)
            {
                return "SELL";
            }
            else {
                return "N/A";
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
        public string Signal { get; set; } = "N/A";// BUY, SELL, or HOLD 
    }
}
