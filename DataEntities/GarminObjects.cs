using System;
using System.Collections.Generic;
using System.Text;

namespace DataEntities
{
    public class Activity
    {
        public string Id { set; get; }
        public string Sport { set; get; }
        public List<Lap> Laps { set; get; }
    }

    public class Lap
    {
        public double TotalTimeSeconds { set; get; }
        public double DistanceMeters { set; get; }
        public double MaximumSpeed { set; get; }
        public double Calories { set; get; }
        public string TriggerMethod { set; get; }
        public float AverageHeartRateBpm { set; get; }
        public double MaximumHeartRateBpm { set; get; }
        public string Intensity { set; get; }
        //public int Cadence { set; get; }
        public string Notes { set; get; }
        public DateTime StartTime { get; set; }
        //public List<Track> Tracks { set; get; }

        public static Lap operator +(Lap a, Lap b)
        {
            if (a == null && b == null)
                throw new InvalidOperationException("Don't be silly"); 

            if (a == null)
                return b;
            if (b == null)
                return a;

            return new Lap
            {
                Calories = a.Calories + b.Calories,
                DistanceMeters = a.DistanceMeters + b.DistanceMeters,
                TotalTimeSeconds = a.TotalTimeSeconds + b.TotalTimeSeconds,
                TriggerMethod = a.TriggerMethod,
                MaximumHeartRateBpm = a.MaximumHeartRateBpm > b.MaximumHeartRateBpm ? a.MaximumHeartRateBpm : b.MaximumHeartRateBpm,
                AverageHeartRateBpm = (a.AverageHeartRateBpm + b.AverageHeartRateBpm) / 2,
                Intensity = a.Intensity,
                MaximumSpeed = a.MaximumSpeed > b.MaximumSpeed ? a.MaximumSpeed : b.MaximumSpeed,
                StartTime = a.StartTime < b.StartTime ? a.StartTime : b.StartTime,
                Notes = a.Notes
            };
        }
    }

    public class Track
    {
        public List<TrackPoint> TrackPoints { set; get; }
    }

    public class TrackPoint
    {
        public string Time { set; get; }
        public double AltitudeMeters { get; set; }
        public double DistanceMeters { get; set; }
        public double HeartRateBpm { get; set; }
       //public int Cadence { get; set; }
        public string SensorState { get; set; }
        public List<Position> Position { get; set; }
    }

    public class Position
    {
        public double LatitudeDegrees { set; get; }
        public double LongitudeDegrees { set; get; }
    }

    public class ActivitySummary
    {

        public double Calories { get; set;  }
        public double DistanceMeters { get; set; }
        public double TotalTimeSeconds { get; set; }
        public string Id { get; set; }
        public string Sport { get; set; }
        public DateTime StartTime { get;set; }
    }
}
