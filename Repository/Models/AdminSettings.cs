﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class AdminSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? UserId { get; set; }
        public bool NotificationStatus { get; set; }
        public double Threshold { get; set; }
        public double Frequency { get; set; }
        
        
        public MovingAverageSettings? MovingAverage { get; set; }
        public RSIThresholds? RSIThresholds { get; set; }
        public MACDSettings? MACD_Settings { get; set; }
        public MeanReversion? MeanReversion { get; set; }
        public List<StrategyWeight>? StrategyWeighted { get; set; }
        public UserAuthtoken UserAuthtoken { get; set; }
    }
    public class StrategyWeight
    {
        public string? Strategy { get; set; }
        public double Weight { get; set; }
        public bool IsActive { get; set; }
    }
    public class MovingAverageSettings
    {
        public int SMA_Periods { get; set; }
        public int LMA_Periods { get; set; }
    }

    public class RSIThresholds
    {
        public int Overbought { get; set; }
        public int Oversold { get; set; }
        public int RsiPeriod { get; set; }
    }

    public class MACDSettings
    {
        public int ShortEmaPeriod { get; set; }
        public int LongEmaPeriod { get; set; }
        public int SignalPeriod { get; set; }
    }

    public class MeanReversion
    {
        public int Period { get; set; }
        public double Threshold { get; set; }
    }

    public class UserAuthtoken
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? code { get; set; }
        public string? Pin { get; set; }
    }

}
