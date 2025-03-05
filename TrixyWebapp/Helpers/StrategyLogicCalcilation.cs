using Repository.Models;

namespace TrixyWebapp.Helpers
{
    public static class StrategyLogicCalcilation
    {
             // 1 → BUY
             //-1 → SELL
             //0 → HOLD
        public static int MovingAverageCrossover(List<double> prices, int shortPeriod, int longPeriod)
        {
            if (prices.Count < longPeriod) return 0;
            double shortSMA = prices.Skip(prices.Count - shortPeriod).Average();
            double longSMA = prices.Skip(prices.Count - longPeriod).Average();
            return shortSMA > longSMA ? 1 : (shortSMA < longSMA ? -1 : 0);
        }

        public static int CalculateRSISignal(List<double> prices, int period)
        {
            if (prices.Count < period) return 0;
            double avgGain = prices.TakeLast(period).Where((p, i) => i > 0 && p > prices[i - 1]).Sum() / period;
            double avgLoss = prices.TakeLast(period).Where((p, i) => i > 0 && p < prices[i - 1]).Sum() / period;
            double rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            double rsi = 100 - (100 / (1 + rs));
            return rsi < 30 ? 1 : (rsi > 70 ? -1 : 0);
        }

        public static int BollingerBandsSignal(List<double> prices, int period)
        {
            if (prices.Count < period) return 0;
            double mean = prices.TakeLast(period).Average();
            double stdDev = Math.Sqrt(prices.TakeLast(period).Select(p => Math.Pow(p - mean, 2)).Average());
            double upperBand = mean + (2 * stdDev);
            double lowerBand = mean - (2 * stdDev);
            double latestPrice = prices.Last();
            return latestPrice < lowerBand ? 1 : (latestPrice > upperBand ? -1 : 0);
        }

        public static int MeanReversionSignal(List<double> prices, int period)
        {
            if (prices.Count < period) return 0;
            double meanPrice = prices.TakeLast(period).Average();
            double latestPrice = prices.Last();
            return latestPrice < meanPrice * 0.95 ? 1 : (latestPrice > meanPrice * 1.05 ? -1 : 0);
        }

        public static int VWAPSignal(List<double> prices)
        {
            double vwap = prices.Average();
            double latestPrice = prices.Last();
            return latestPrice < vwap ? 1 : (latestPrice > vwap ? -1 : 0);
        }

        public static int MACDSignal(List<double> prices)
        {
            if (prices.Count < 26) return 0;
            double ema12 = prices.Skip(prices.Count - 12).Average();
            double ema26 = prices.Skip(prices.Count - 26).Average();
            double macd = ema12 - ema26;
            double signalLine = prices.Skip(prices.Count - 9).Average();
            return macd > signalLine ? 1 : (macd < signalLine ? -1 : 0);
        }

        public static string GetCombinationStrategyDecision(List<int> signals, List<double> weights)
        {
            if (signals.Count != weights.Count)
                throw new ArgumentException("Signals and weights must have the same length.");
            double finalScore = signals.Zip(weights, (signal, weight) => signal * weight).Sum();
            return finalScore > 0.5 ? "BUY" : "SELL";//(finalScore < -0.5 ?  : "HOLD");
        }
    }

    public static class finalStocksignalCalulation
    {
        public static void GetCombinationsignal(AdminSettings userSettings, List<Historical_Data> stockData)
        {
            var weightedstrategy = userSettings.StrategyWeighted.Select(x=>x.Weight).ToList();

            List<double> stockPrices = stockData.Select(x => Convert.ToDouble(x.Close)).ToList();
            int smaSignal = StrategyLogicCalcilation.MovingAverageCrossover(stockPrices, 10, 50);
            int rsiSignal = StrategyLogicCalcilation.CalculateRSISignal(stockPrices, 14);
            int bollingerSignal = StrategyLogicCalcilation.BollingerBandsSignal(stockPrices, 20);
            int meanReversionSignal = StrategyLogicCalcilation.MeanReversionSignal(stockPrices, 30);
            int vwapSignal = StrategyLogicCalcilation.VWAPSignal(stockPrices);
            int macdSignal = StrategyLogicCalcilation.MACDSignal(stockPrices);

            // Combine all signals
            List<int> signals = new List<int> { smaSignal, rsiSignal, bollingerSignal, meanReversionSignal, vwapSignal, macdSignal };
            string finalDecision = StrategyLogicCalcilation.GetCombinationStrategyDecision(signals, weightedstrategy);
            Console.WriteLine($"Final Trading Decision: {finalDecision}");
        }
    }
}
