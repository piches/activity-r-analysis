library(reshape2)
library(dplyr)
library(readr)
library(ggplot2)

options(scipen = 999)

filter <- function(data, field, from, to){
  return(data[which(field>=from & field <= to),])
}

toHours <- function(ms){
  return (ms <- ms/3600000)
}

dateFrom <- as.character('2020-01-01')
dateTo <- as.character('2020-05-31')

#load all CSVs 
shealthSteps <- read_csv("~/Projects/ActivityCompare/data/raw/Takeout/Fit/shealth-steps.csv")
shealthSteps <- filter(shealthSteps, shealthSteps$Date, dateFrom, dateTo)


activities <- read_csv("~/Projects/ActivityCompare/data/raw/Takeout/Fit/activities.csv")
activities <- filter(activities, activities$StartTime, dateFrom, dateTo)

trends <- read_csv("~/Projects/ActivityCompare/data/raw/Takeout/Fit/daily-summaries.csv")
trends <- filter(trends, trends$Date, dateFrom, dateTo)

#generate the graphs
generateGraphs(shealthSteps, activities, trends)

generateGraphs <- function(shealth, activities, trends){

  generateSteps(shealth) #generate
  ggsave(paste("steps", dateFrom, "-" , dateTo , ".png", sep = "")) #save
  
  generateDistribution(activities)
  ggsave(paste("distro", dateFrom, "-" , dateTo , ".png", sep = ""))

  generateTrends(trends)
  ggsave(paste("trends", dateFrom, "-" , dateTo , ".png", sep = ""))
  
}

generateSteps <- function(data){
  
  
  #clean the data a bit 
  data$DateCalc <- as.Date(data$Date)
  data$ActiveTime = toHours(data$ActiveTime)
  
  
  return(ggplot(data, aes(x = DateCalc, y=ActiveTime)) + geom_smooth(method = loess, se = FALSE, size = 2) + geom_line(size=1,position = 'jitter', alpha=0.5) + labs(x = "Date", y = "Hours") )
}

generateDistribution <- function(data){
  
  #clean the data a lil bit more 
  data$DistanceMeters = (round(data$DistanceMeters, digits = 3) / 1000) #distance to meters
  data$Date = as.Date(data$StartTime) #transform chr to date so we group by 30 mins block
  data$StartTimeCalc = as.POSIXct(data$StartTime, tz="EST")
  data$StartTimeDisplay <- substr(strftime(round_date(data$StartTimeCalc,unit="30 minutes"), tz = "EST"), 12, 19) #takes the hour block of the date/time
  data$EndTimeCalc <- data$StartTimeCalc + seconds(data$TotalTimeSeconds)
  data$Duration <- as.double(round(difftime(data$EndTimeCalc, data$StartTimeCalc, units="hours"),2))

  data <- subset(data, StartTimeDisplay != "00:00:00") #removing empty rows
  
  return(ggplot(data, aes(x = Date, y=StartTimeDisplay, color=Sport, size=Duration)) + geom_point() + labs(x = "Date", y = "Starting time", color="Activity", size = "Duration (hours)"))
}

generateTrends <- function(data){
  
  data$Date = as.Date(data$Date,'%Y-%m-%d %H:%M:%S')
  
  #clean the data a lot
  
  #convert ms to hours
  data$WalkingDuration = toHours(data$WalkingDuration)  
  data$SleepDuration = toHours(data$SleepDuration)
  data$RollerDuration = toHours(data$RollerDuration)
  data$RunningDuration = toHours(data$RunningDuration)
  data$StrengthDuration = toHours(data$StrengthDuration)
  data$MoveMinutes = data$MoveMinutes / 60
  
 
  
  trends <- data %>% group_by( Date) %>% summarise_at(vars(StepCount, Calories, Distance, BikingDuration, MoveMinutes, StrengthDuration, WalkingDuration, RunningDuration,RollerDuration, SleepDuration), funs(sum))
  melted <- melt(data = trends, id.vars = "Date", measure.vars = c("WalkingDuration", "RunningDuration", "SleepDuration", "MoveMinutes", "StrengthDuration"))
  
  return(ggplot(melted, aes(x=Date, y=value, col=variable)) + geom_smooth(method = loess, se = FALSE, size = 2) + geom_line(size=1,position = 'jitter', alpha=0.5) + labs(x = "Date", y = "Hours", color="Activity") + scale_colour_hue(labels = c("Walking", "Running", "Sleeping", "Moving", "Strength Training")))
}
