using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole;
using System;
using TearsInRain.Entities;
using TearsInRain.Serializers;

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
            Hour = 0;
            if (Day == 28) {
                Day = 1;
                if (Season == 3) {
                    Season = 0; 
                    Year++;
                } else {
                    Season++;
                }
            } else {
                Day++;
            }

            //for (int i = 0; i < GameLoop.World.CurrentMap.Tiles.Length; i++) {
            //    if (GameLoop.World.CurrentMap.NewTiles.Name == "farmland") {
            //        Item item = null;
            //        item = GameLoop.World.CurrentMap.GetEntityAt<Item>(i.ToPoint(GameLoop.World.CurrentMap.Width)); 

            //        if (item != null && item.Quantity == 1 && item.Properties.ContainsKey("qualities") && item.Properties["qualities"].Contains("plantable")) { 
            //            if (item.Properties.ContainsKey("growing-seasons") && item.Properties["growing-seasons"].Contains(GetSeasonName())) {

            //                GameLoop.UIManager.MessageLog.Add("Ticked crop, " + item.Properties["current-stage-time"]);
            //                string[] tempStr = item.Properties["current-stage-time"].Split(',');
            //                string[] stageTimes = item.Properties["growth-times"].Split(',');
            //                int currStage = Convert.ToInt32(tempStr[0]);
            //                int timeInStage = Convert.ToInt32(tempStr[1]);
            //                string newCurrTimer = "";

            //                if (stageTimes.Length > currStage) {
            //                    timeInStage++;

            //                    if (Convert.ToInt32(stageTimes[currStage]) <= timeInStage) {
            //                        currStage += 1;
            //                        newCurrTimer = currStage.ToString() + ",0";


            //                        if (currStage == 0) {
            //                            item.Animation.CurrentFrame[0].Glyph = 269;
            //                            item.Animation.CurrentFrame[0].Foreground = new Color(0, 255, 12, 255);
            //                        } else if (currStage < stageTimes.Length - 1) {
            //                            item.Animation.CurrentFrame[0].Glyph = 270;
            //                            item.Animation.CurrentFrame[0].Foreground = new Color(0, 255, 12, 255);
            //                        } else if (currStage >= stageTimes.Length - 1) {
            //                            item.Animation.CurrentFrame[0].Glyph = 271;
            //                            item.Animation.CurrentFrame[0].Foreground = new Color(0, 255, 12, 255);
            //                        }

            //                        item.Animation.IsDirty = true;
            //                    } else {
            //                        newCurrTimer = currStage.ToString() + "," + (timeInStage).ToString();
            //                    }

            //                    if (currStage >= stageTimes.Length) {

            //                        string[] quantityStr = item.Properties["harvest-yield"].Split(',');
            //                        int minQ = Convert.ToInt32(quantityStr[0]);
            //                        int maxQ = Convert.ToInt32(quantityStr[1]);
            //                        int quan = GameLoop.Random.Next(minQ, maxQ + 1);

            //                        Point pos = item.Position;
            //                        newCurrTimer = "0,0";

            //                        Item newItem = GameLoop.ItemLibrary[item.Properties["grown"]].Clone();


            //                        newItem.Quantity = quan;
            //                        newItem.Position = pos;

            //                        newItem.Animation.IsDirty = true;

            //                        GameLoop.World.CurrentMap.Remove(item);
            //                        GameLoop.World.CurrentMap.Add(newItem);
            //                        GameLoop.UIManager.SyncMapEntities(true);

            //                        string itemDrop = "i_data|update|" + item.Position.X + "|" + item.Position.Y + "|" + JsonConvert.SerializeObject(newItem, Formatting.Indented, new ItemJsonConverter());
            //                        GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(itemDrop));
            //                    } else {
            //                        string itemDrop = "i_data|update|" + item.Position.X + "|" + item.Position.Y + "|" + JsonConvert.SerializeObject(item, Formatting.Indented, new ItemJsonConverter());
            //                        GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(itemDrop));

            //                    }

            //                    item.Properties["current-stage-time"] = newCurrTimer;

            //                }
            //            }
            //        }
            //    }
            // }
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
