using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    [Serializable]
    public class Load
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public double ForecastValue { get; set; }

        [DataMember]
        public double MeasuredValue { get; set; }

        [DataMember]
        public double AbsolutePercentageDeviation { get; set; }

        [DataMember]
        public double SquaredDeviation { get; set; }

        [DataMember]
        public int ImportedFileId { get; set; }

        public Load() { }

        public Load(DateTime timestamp, double forecastValue, double measuredValue, double absolutePercentageDeviation, double squaredDeviation)
        {
            Id = -1;
            Timestamp = timestamp;
            ForecastValue = forecastValue;
            MeasuredValue = measuredValue;
            AbsolutePercentageDeviation = absolutePercentageDeviation;
            SquaredDeviation = squaredDeviation;
            ImportedFileId = -1;
        }
    }
}
