using DataEntities;
using System;
using System.IO;
using System.Linq;
using DataAccess;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FitDataToSql
{
    class Program
    {

        static readonly string ActivitiesFolder = "activities";
        static readonly string DailyAggFolder = "daily aggregations";
        static readonly string DailySummaryFile = "Daily Summaries.csv";
       
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                MessageWait("Please enter a path");
                return;
            }

            if (args.Length > 1)
            {
                var cmd = args[1];
                switch (cmd.ToLower().Substring(1))
                {
                    case "activities":
                        if (Directory.Exists(args[0]))
                            SaveNewActivites(LoadActivities(new DirectoryInfo(Path.Combine(args[0], ActivitiesFolder))), Path.Combine(args[0], "activities.csv"));
                        break;
                    case "shealth":
                        if (Directory.Exists(args[0]) && args.Length >= 3)
                            SaveCsv(LoadSamsungHealth(new DirectoryInfo(Path.Combine(args[0])), args[2]), Path.Combine(args[0], "shealth-steps.csv"));
                        break;
                    case "dailysum":
                        if (Directory.Exists(args[0]))
                            SaveNewSummaries(LoadDailyAgg(new DirectoryInfo(Path.Combine(args[0], DailyAggFolder))), Path.Combine(args[0], "daily-summaries.csv"));
                        break;
                    default:
                        MessageWait("Invalid command");
                        return;
                }
            }
            else //load everything
            {
                LoadData(args[0]);
            }
        }

        //saving methods
        private static void SaveNewActivites(List<Activity> list, string csvPath)
        {
            var listToSave = new List<ActivitySummary>();

            //flatten activity laps by adding them
            foreach (var activity in list)
            {
                //no laps to flatten
                if (!activity.Laps.Any())
                    continue;


                //take the first lap, add the rest (see class def for addition logic)
                Lap lap = activity.Laps[0];
                if (activity.Laps.Count > 1)
                {
                    for (int i = 1; i <= activity.Laps.Count - 1; i++)
                    {
                        lap += activity.Laps[i];
                    }
                }

                listToSave.Add(new ActivitySummary
                {
                    Id = activity.Id,
                    Sport = activity.Sport,
                    StartTime = lap.StartTime,
                    DistanceMeters = lap.DistanceMeters,
                    Calories = lap.Calories,
                    TotalTimeSeconds = lap.TotalTimeSeconds
                });
            }

            SaveCsv(listToSave, csvPath);
        }

        private static void SaveNewSummaries(List<DailySummary> list, string csvPath)
        {
            SaveCsv(list, csvPath);
        }

        private static void SaveCsv<T>(IEnumerable<T> list, string csvPath)
        {
            if (File.Exists(csvPath))
                File.Delete(csvPath);

            DataTable.New.FromEnumerable(list).SaveCSV(csvPath);
        }

        //dry
        static void MessageWait(string msg)
        {
            Console.WriteLine(msg);
            Console.ReadKey();
        }

        static void MessageWait(string format, params object[] values)
        {
            Console.WriteLine(string.Format(format, values));
            Console.ReadKey();
        }

        /*loading methods*/
        static void LoadData(string path)
        {
            if (!Directory.Exists(path))
            {
                MessageWait("The path {0} doesn't exist", path);
                return;
            }

            var di = new DirectoryInfo(path);

            foreach (var dir in di.GetDirectories())
            {
                if (dir.Name.Equals(ActivitiesFolder, StringComparison.InvariantCultureIgnoreCase))
                    SaveNewActivites(LoadActivities(dir), Path.Combine(path, "activities.csv"));


                if (dir.Name.Equals(DailyAggFolder, StringComparison.InvariantCultureIgnoreCase))
                    SaveNewSummaries(LoadDailyAgg(dir), Path.Combine(path, "daily-summaries.csv"));

            }
        }

        private static List<SamsungStepSummary> LoadSamsungHealth(DirectoryInfo directoryInfo, string fileName)
        {
            var filePath = Path.Combine(directoryInfo.FullName, fileName);
            var list = new List<SamsungStepSummary>(); 

            if(File.Exists(filePath))
            {
                var dt = DataTable.New.ReadCsv(filePath);

                foreach(var row in dt.Rows)
                {
                    var stepCount = row["step_count"];
                    var activeTime = row["active_time"];
                    var date = row["create_time"];
                    var deviceID = row["deviceuuid"];  

                    Console.WriteLine("Parsing and adding data for {0}", date);

                    if (deviceID != "VfS0qUERdZ") //excluding data from my old phone
                        continue;

                    list.Add(new SamsungStepSummary()
                    {
                        Date = string.IsNullOrWhiteSpace(stepCount) ? default : DateTime.Parse(date).Date,
                        ActiveTime = string.IsNullOrWhiteSpace(activeTime) ? 0 : int.Parse(activeTime),
                        StepCount = string.IsNullOrWhiteSpace(stepCount) ? 0 : int.Parse(stepCount)
                    });
                }
            }
            return list;
        }

        private static List<DailySummary> LoadDailyAgg(DirectoryInfo dir)
        {
            var dailySummaryPath = Path.Combine(dir.FullName, DailySummaryFile);
            var dailySummaries = new List<DailySummary>();

            if (File.Exists(dailySummaryPath))
            {
                var dt = DataTable.New.ReadCsv(dailySummaryPath);

                foreach (var row in dt.Rows)
                {
                    var date = row["Date"];
                    var kcal = row["Calories (kcal)"];
                    var distance = row["Distance (m)"];
                    var maxSpeed = row["Max speed (m/s)"];
                    var stepCount = row["Step Count"];
                    var walkingDuration = row["Walking duration (ms)"];
                    var rollerDuration = row["Inline skating duration (ms)"];
                    var runningDuration = row["Running duration (ms)"];
                    var sleepDuration = row["Sleep duration (ms)"];
                    var strengthDuration = row["Strength training duration (ms)"];
                    var treadmillDuration = row["Treadmill running duration (ms)"];
                    var bikingDuration = row["Road biking duration (ms)"];
                    var heartPoints = row["Heart Points"];
                    var moveMinutes = row["Move Minutes count"];
                    var heartMinutes = row["Heart Minutes"];

                    Console.WriteLine("Parsing and adding data for {0}", date);

                    dailySummaries.Add(new DailySummary()
                    {
                        Date = DateTime.Parse(date),
                        Calories = string.IsNullOrWhiteSpace(kcal) ? 0 : float.Parse(kcal),
                        Distance = string.IsNullOrWhiteSpace(distance) ? 0 : float.Parse(distance),
                        MaxSpeed = string.IsNullOrWhiteSpace(maxSpeed) ? 0 : float.Parse(maxSpeed),
                        StepCount = string.IsNullOrWhiteSpace(stepCount) ? 0 : int.Parse(stepCount),
                        WalkingDuration = string.IsNullOrWhiteSpace(walkingDuration) ? 0 : int.Parse(walkingDuration),
                        RollerDuration = string.IsNullOrWhiteSpace(rollerDuration) ? 0 : int.Parse(rollerDuration),
                        RunningDuration = string.IsNullOrWhiteSpace(runningDuration) ? 0 : int.Parse(runningDuration),
                        SleepDuration = string.IsNullOrWhiteSpace(sleepDuration) ? 0 : int.Parse(sleepDuration),
                        StrengthDuration = string.IsNullOrWhiteSpace(strengthDuration) ? 0 : int.Parse(strengthDuration),
                        TreadmillDuration = string.IsNullOrWhiteSpace(treadmillDuration) ? 0 : int.Parse(treadmillDuration),
                        BikingDuration = string.IsNullOrWhiteSpace(bikingDuration) ? 0 : int.Parse(bikingDuration),
                        HeartMinutes = string.IsNullOrWhiteSpace(heartMinutes) ? 0 : int.Parse(heartMinutes),
                        HeartPoints = string.IsNullOrWhiteSpace(heartPoints) ? 0 : double.Parse(heartPoints),
                        MoveMinutes = string.IsNullOrWhiteSpace(moveMinutes) ? 0 : int.Parse(moveMinutes)
                    });
                }
            }

            return dailySummaries;
        }

        private static List<Activity> LoadActivities(DirectoryInfo dir)
        {
            var activities = new List<Activity>();
            foreach (var file in dir.EnumerateFiles("*.tcx"))
            {
                Console.WriteLine("Analyzing {0}", file.FullName);

                XElement root = XElement.Load(file.FullName);
                XNamespace ns = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";

                var results = from activityElement in root.Descendants(ns + "Activities")
                              select new Activity
                              {
                                  Id = (activityElement.FirstNode != null && activityElement.Element(ns + "Activity") != null && activityElement.Element(ns + "Activity").Element(ns + "Id") != null) ? activityElement.Element(ns + "Activity").Element(ns + "Id").Value : default,
                                  Sport = (activityElement.FirstNode != null &&  activityElement.Element(ns + "Activity") != null && activityElement.Element(ns + "Activity").Attributes("Sport") != null) ? activityElement.Element(ns + "Activity").Attribute("Sport").Value : default,
                                  Laps =    (from lapElement in activityElement.Descendants(ns + "Lap")
                                            select new Lap
                                            {
                                                TotalTimeSeconds = lapElement.Element(ns + "TotalTimeSeconds") != null ? Convert.ToDouble(lapElement.Element(ns + "TotalTimeSeconds").Value) : 0.00,
                                                DistanceMeters = lapElement.Element(ns + "DistanceMeters") != null ? Convert.ToDouble(lapElement.Element(ns + "DistanceMeters").Value) : 0.00,
                                                MaximumSpeed = lapElement.Element(ns + "MaximumSpeed") != null ? Convert.ToDouble(lapElement.Element(ns + "MaximumSpeed").Value) : 0.00,
                                                Calories = lapElement.Element(ns + "Calories") != null ? Convert.ToDouble(lapElement.Element(ns + "Calories").Value) : 0.0,
                                                MaximumHeartRateBpm = lapElement.Element(ns + "MaximumHeartRateBpm") != null ? Convert.ToDouble(lapElement.Element(ns + "MaximumHeartRateBpm").Value) : 0.0,
                                                AverageHeartRateBpm = lapElement.Element(ns + "AverageHeartRateBpm") != null ? float.Parse(lapElement.Element(ns + "AverageHeartRateBpm").Value) : 0,
                                                Intensity = lapElement.Element(ns + "Intensity") != null ? lapElement.Element(ns + "Intensity").Value : "",
                                                TriggerMethod = lapElement.Element(ns + "TriggerMethod") != null ? lapElement.Element(ns + "TriggerMethod").Value : "",
                                                Notes = lapElement.Element(ns + "Notes") != null ? lapElement.Element(ns + "Notes").Value : "",
                                                StartTime = lapElement.Attributes("StartTime") != null ? DateTime.Parse(lapElement.Attribute("StartTime").Value) : default//,
                                            }).ToList()
                              };

                activities.AddRange(results);

            }

            return activities;
        }
    }
}
