using System;
using System.Collections.Generic;
using System.Text;

namespace DataEntities
{
    public class DailySummary
    {
        public DateTime Date { get; set; }
        public float Calories { get; set; }
        public float Distance { get; set; }
        public float MaxSpeed { get; set; }
        public int StepCount { get; set; }
        public int MoveMinutes { get; set; }
        public double HeartPoints { get; set; }
        public int HeartMinutes { get; set; }
        public int BikingDuration { get; set; }
        public int WalkingDuration { get; set; }
        public int RunningDuration { get; set; }
        public int SleepDuration { get; set; }
        public int RollerDuration { get; set; }
        public int TreadmillDuration { get; set; }
        public int StrengthDuration { get; set; }

    }
}
