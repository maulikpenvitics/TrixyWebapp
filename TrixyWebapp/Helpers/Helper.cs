using Humanizer;
using Microsoft.ML;
using Microsoft.ML.Data;
using Repository.Enum;
using Repository.FyersWebSocketServices;
using Repository.Models;
using System.Linq;
using TrixyWebapp.ViewModels;

namespace TrixyWebapp.Helpers
{
    public static class Helper
    {
        public static string GetFullUrl(this HttpContext httpContext, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{relativePath}";
        }
        public static void LogFilegenerate(Exception ex, string path, IWebHostEnvironment env)
        {
            string errorMessage = $"DateTime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";
            errorMessage += Environment.NewLine;
            errorMessage += "------------------------Exception-----------------------------------";
            errorMessage += Environment.NewLine;
            errorMessage += $"Path: {path}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Message: {ex.Message}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Details: {ex}";
            errorMessage += Environment.NewLine;
            errorMessage += "-----------------------------------------------------------";
            errorMessage += Environment.NewLine;

            // Get the path to the "ErrorLog" directory in wwwroot
            string logDirectory = Path.Combine(env.WebRootPath, "ErrorLog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFilePath = Path.Combine(logDirectory, "ErrorLogin.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(errorMessage);
            }
        }

        public static void LogFile(string path, IWebHostEnvironment env)
        {
            string errorMessage = $"DateTime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";
            errorMessage += Environment.NewLine;
            errorMessage += "------------------------Exception-----------------------------------";
            errorMessage += Environment.NewLine;
            errorMessage += $"Path: {path}";
            errorMessage += Environment.NewLine;
         
            errorMessage += Environment.NewLine;
            errorMessage += "-----------------------------------------------------------";
            errorMessage += Environment.NewLine;

            // Get the path to the "ErrorLog" directory in wwwroot
            string logDirectory = Path.Combine(env.WebRootPath, "ErrorLog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFilePath = Path.Combine(logDirectory, "UserLogin.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(errorMessage);
            }
        }

    }

    public static class StockCalculator
    {
        public static void CalculateMovingAverages(List<BuySellForMAC> stockPrices, int shortWindow, int longWindow)
        {
            for (int i = 0; i < stockPrices.Count; i++)
            {
                if (i >= shortWindow - 1)
                    stockPrices[i].SMA = (double)stockPrices.Skip(i - shortWindow + 1).Take(shortWindow).Average(s => s.Close);

                if (i >= longWindow - 1)
                    stockPrices[i].LMA = (double)stockPrices.Skip(i - longWindow + 1).Take(longWindow).Average(s => s.Close);
            }
        }

        public static void CalculateRSI(List<BuySellForRSI> stockPrices, int period)
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

        public static void CalculateBollingerBands(List<BuySellforBollingerBands> stockPrices, int period)
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
    public static class SignalGenerator
    {
        // 1 → BUY
        //-1 → SELL
        //0 → HOLD
        public static string GenerateSignalsforMovingAverageCrossover(List<Historical_Data> stockData, int shortTerm, int longTerm)
        {
            var signal = "HOLD";
           
            var stockdata = stockData.Select(x => new BuySellForMAC
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();
            stockdata= stockdata.OrderByDescending(x => x.Date).ToList();
            StockCalculator.CalculateMovingAverages(stockdata, shortTerm, longTerm);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result!=null)
            {
                if (result.SMA > result.LMA)
                {
                    signal = "BUY";
                }
                else if (result.SMA < result.LMA)
                {
                    signal = "SELL";
                }
                else
                {
                    signal = "N/A";
                }
            }
            return signal;
        }

        public static string genratesignalsforRSI(List<Historical_Data> stockData,int rsiPeriod,int Overbought, int Oversold)
        {
            string Signal = "HOLD";
            var stockdata = stockData.Select(x => new BuySellForRSI
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();
            stockdata=stockdata.OrderByDescending(x => x.Date).ToList();
             StockCalculator.CalculateRSI(stockdata, rsiPeriod);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result!=null)
            {
                if (result.RSI < Overbought)
                {
                    Signal = "BUY";
                }
                else if (result.RSI > Oversold)
                {
                    Signal = "SELL";
                }
                else
                {
                    Signal = "N/A";
                }
            }
            
            return Signal;
        }
        //BollingerBands strategy
        public static string GenerateBuySellSignalsForBB(List<Historical_Data> stockData,int rsiPeriod)
        {
            string Signal = "HOLD";
            var stockdata = stockData.Select(x => new BuySellforBollingerBands
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();
          //  int rsiPeriod = 14; // RSI period (default 14 days)

            stockdata=stockdata.OrderByDescending(x => x.Date).ToList() ;
            StockCalculator.CalculateBollingerBands(stockdata, rsiPeriod);
            var result = stockdata.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result!=null)
            {
                if (result.Close <= result.LowerBand)
                {
                    Signal = "BUY";
                }
                else if (result.Close >= result.UpperBand)
                {
                    Signal = "SELL";
                }
                else
                {
                    Signal = "N/A";
                }
            }
            return Signal;
        }

        public static string CalculateMACD(List<Historical_Data> stockData, int shortEmaPeriod, int longEmaPeriod, int signalPeriod)
        {
            var signal = "HOLD";
            var stockPrices = stockData.Select(x => new BuySellforMACD
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();

            List<double> closePrices = stockPrices.OrderByDescending(x => x.Date).Select(s => s.Close).ToList();

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
            if (result!=null)
            {
                if(result.MACD<result.TradeSignal)
                {
                    signal = "BUY";
                }
                else if (result.MACD > result.TradeSignal)
                {
                    signal = "SELL";
                }
                else
                {
                    signal = "N/A";
                }
            }
            return signal;
        }
        public static string CalculateMeanReversion(List<Historical_Data> stockData, int period, double threshold)
        {
            string Signal = "HOLD";
            var stockPrices = stockData.Select(x => new BuySellforMeanReversion
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();

            stockPrices=stockPrices.OrderByDescending(x => x.Date).ToList();
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
            if (result !=null)
            {
                double deviation=(result.Close-result.MeanPrice)/result.MeanPrice;
                if (deviation <= -threshold)
                {
                    Signal = "BUY";
                }
                    
                else if (deviation >= threshold)
                {
                    Signal = "SELL";
                }
                else
                {
                    Signal = "N/A";
                }
                    
            }
            return Signal;
        }

        public static string CalculateVWAP(List<Historical_Data> stockData)
        {
            string Signal = "HOLD";
            var stockPrices = stockData.Select(x => new BuySellforVWAP
            {
                Close = (double)x.Close,
                Date = x.Timestamp,
                Volume = (double)x.Volume,
            }).DistinctBy(item => item.Date).OrderBy(x => x.Date).ToList();
            var result = stockPrices.OrderByDescending(x => x.Date).FirstOrDefault();
            if (result !=null)
            {
                double cumulativePV = result.Close * result.Volume;  // Cumulative Price * Volume
                double cumulativeVolume = result.Volume;// Cumulative Volume
                if (cumulativeVolume != 0)
                {
                    result.VWAP = cumulativePV / cumulativeVolume;
                }
                if (result.Close< result.VWAP)
                {
                    Signal = "BUY";
                }
                else if (result.Close > result.VWAP)
                {
                    Signal= "SELL";
                }
                else
                {
                    Signal = "N/A";
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

        public static string GetCombinationsignal(AdminSettings userSettings, List<Historical_Data> stockData,List<UserStrategy> userStrategies)
        {
            var weightedstrategy = userSettings.StrategyWeighted;
            var thresold = userSettings.Threshold;
            
            List<string> signals = new List<string>();
            foreach (var item in userStrategies)
            {
                if (Enum.TryParse<StrategyType>(item.StretagyName, out var strategy))
                {
                    switch (strategy)
                    {
                        case StrategyType.Bollinger_Bands://  "Bollinger_Bands":
                            var bollingerBands = GenerateBuySellSignalsForBB(stockData, userSettings?.RSIThresholds?.RsiPeriod ?? 0);
                            signals.Add(bollingerBands);
                            break;
                        case StrategyType.MACD: //"MACD"
                            var MACD = CalculateMACD(stockData, userSettings?.MACD_Settings?.ShortEmaPeriod ?? 0, userSettings?.MACD_Settings?.LongEmaPeriod ?? 0,
                userSettings?.MACD_Settings?.SignalPeriod ?? 0);
                            signals.Add(MACD);
                            break;
                        case StrategyType.Mean_Reversion://"Mean_Reversion"
                            var MeanReversion = CalculateMeanReversion(stockData, userSettings?.MeanReversion?.Period ?? 0, userSettings?.MeanReversion?.Threshold ?? 0);
                            signals.Add(MeanReversion);
                            break;
                        case StrategyType.Moving_Average:// "Moving_Average":
                            var MovingAverageCrossover = GenerateSignalsforMovingAverageCrossover(stockData, userSettings?.MovingAverage?.SMA_Periods ?? 0, userSettings?.MovingAverage?.LMA_Periods ?? 0);
                            signals.Add(MovingAverageCrossover);

                            break;
                        case StrategyType.RSI:// "RSI":
                            var RSI = genratesignalsforRSI(stockData, userSettings?.RSIThresholds?.RsiPeriod ?? 0, userSettings?.RSIThresholds?.Overbought ?? 0
                     , userSettings?.RSIThresholds?.Oversold ?? 0);
                            signals.Add(RSI);
                            break;
                        case StrategyType.VWAP:// "VWAP":
                            var VWAP = CalculateVWAP(stockData);
                            signals.Add(VWAP);
                            break;
                    }
                }
                 
            }
           
            var finalsignal= GetCombinationStrategyDecision(signals, thresold);

            return finalsignal;
        }

        public static string GetCombinationStrategyDecision(List<string> signals,double thresold)
        {
            Dictionary<string, int> signalMapping = new Dictionary<string, int>
            {
                 { "BUY", 1 },
                 { "SELL", -1 },
                 { "HOLD", 0 }
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

            if (finalScore > 0)
            {
                return "BUY";
            }
            else if (finalScore < 0)
            {
                return "SELL";
            }
            else
            {
                return "N/A";
            }

        }
        public static void StockSentimentStrategy()
        {
            var mlContext = new MLContext();
            var newsData = new List<StockSentiment>
            {
            new StockSentiment { NewsHeadline = "Tech stocks rally as earnings exceed expectations" },
            new StockSentiment { NewsHeadline = "Stock market crashes amid inflation fears" },
            new StockSentiment { NewsHeadline = "Investors optimistic about economic recovery" },
            new StockSentiment { NewsHeadline = "Recession fears grow after Fed's interest rate hike" }
            };
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(StockSentiment.NewsHeadline))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Label"))
            .Append(mlContext.Transforms.Concatenate("Features", "Features"))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var trainingData = mlContext.Data.LoadFromEnumerable(newsData);
            var model = pipeline.Fit(trainingData);
            var sentimentEngine = mlContext.Model.CreatePredictionEngine<StockSentiment, SentimentPrediction>(model);
            foreach (var news in newsData)
            {
                var prediction = sentimentEngine.Predict(news);
                news.SentimentScore = prediction.Score;
                news.TradeSignal = GenerateTradeSignal(news.SentimentScore);
            }
        }
        static string GenerateTradeSignal(float sentimentScore)
        {
            if (sentimentScore > 0.7) return "BUY";
            if (sentimentScore < 0.3) return "SELL";
            return "N/A";
        }
    }

}
