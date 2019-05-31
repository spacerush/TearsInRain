using Microsoft.Xna.Framework;
using SadConsole;

namespace TearsInRain {
    class TimeManager {
        public int Day = 1;
        public int Season = 0;
        public int Year = 1;

        public int Hour = 6;
        public int Minute = 0;


        public TimeManager(int day = 1, int season = 0, int year = 1, int hour = 6, int minute = 0) {
            Day = day;
            Season = season;
            Year = year;
            Hour = hour;
            Minute = minute;
        }


        public void EndDay() {
            if (Day == 28) {
                Day = 1;
                if (Season == 3) {
                    Season = 0; 
                    Year++;
                } else {
                    Season++;
                }
            } else {
                Hour = 0;
                Day++;
            }
        }


        public ColoredString ColoredSeason() {
            Color SeasonColor = new Color();

            switch (GetSeasonName()) {
                case "Spring":
                    SeasonColor = Color.SpringGreen;
                    break;
                case "Summer":
                    SeasonColor = Color.Goldenrod;
                    break;
                case "Autumn":
                    SeasonColor = Color.OrangeRed;
                    break;
                case "Winter":
                    SeasonColor = Color.CornflowerBlue;
                    break;
                default:
                    SeasonColor = Color.Red;
                    break;
            }

            var date = GetSeasonName() + " ";
            ColoredString season = new ColoredString(date, SeasonColor, Color.TransparentBlack);
            return season;
        }

        public void AddMinute() {
            if (Minute == 59) {
                Minute = 0;
                if (Hour == 23) {
                    EndDay();
                } else {
                    Hour++;
                }
            } else {
                Minute++;
            }

            if (GameLoop.NetworkingManager.myUID == GameLoop.NetworkingManager.hostUID && GameLoop.NetworkingManager.myUID != 0) {
                var timeString = "time|" + GameLoop.TimeManager.Year + "|" + GameLoop.TimeManager.Season + "|" + GameLoop.TimeManager.Day + "|" + GameLoop.TimeManager.Hour + "|" + GameLoop.TimeManager.Minute;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(timeString));
            }
        }


        public string GetTimeString() {
            string time = "00:00";

            if (Hour < 10) {
                time = "0" + Hour.ToString() + ":";
            } else {
                time = Hour.ToString() + ":";
            }

            if (Minute < 10) {
                time += "0" + Minute.ToString();
            } else {
                time += Minute.ToString();
            }

            return time;
        }


        public string GetSeasonName() {
            switch(Season) {
                case 0:
                    return "Spring";
                case 1:
                    return "Summer";
                case 2:
                    return "Autumn";
                case 3:
                    return "Winter";
                default:
                    return "ERROR";
            }
        }


    }
}
