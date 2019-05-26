using Discord;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole;
using System;
using TearsInRain.Tiles;
using TearsInRain.UI;

namespace TearsInRain {
    class NetworkingManager {
        public Discord.Discord discord;
        private static long applicationID = 579827348665532425;

        public long myUID = 0;
        public bool updateUID = true;
        public long hostUID = 0;
        public long lobbyID = 0;
        public string myUsername = "";
        public string MD5map = "";

        public UserManager userManager;

        public void InitNetworking(long lobbyId) {
            // First, connect to the lobby network layer
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.ConnectNetwork(lobbyId);

            lobbyID = lobbyId;

            // Next, deterministically open our channels
            // Reliable on 0, unreliable on 1
            lobbyManager.OpenNetworkChannel(lobbyId, 0, true); // Game Logic
            lobbyManager.OpenNetworkChannel(lobbyId, 1, false); // Chat Logic
            lobbyManager.OpenNetworkChannel(lobbyId, 2, true); // World Logic

            lobbyManager.OnNetworkMessage += processMessage;
            

            hostUID = lobbyManager.GetLobby(lobbyID).OwnerId;  
            // We're ready to go!
        }

        private void processMessage(long lobbyId, long userId, byte channelId, byte[] data) {
            // DO SOME NETWORK PROCESSING BULLSHIT HERE
            if (data == System.Text.Encoding.UTF8.GetBytes("a")) {
                return;
            }

            if (channelId == 0) {
                var msg = System.Text.Encoding.UTF8.GetString(data);
                var splitMsg = msg.Split('|');

                if (splitMsg[0] == "move_p") { // Player got moved
                    if (GameLoop.World.players[Convert.ToInt64(splitMsg[1])] != null) {
                        GameLoop.World.players[Convert.ToInt64(splitMsg[1])].Position = new Point(Convert.ToInt32(splitMsg[2]), Convert.ToInt32(splitMsg[3]));
                    }
                }

                if (splitMsg[0] == "p_list") { // Translates a list of players being sent
                    for (int i = 1; i < splitMsg.Length; i++) {
                        GameLoop.World.CreatePlayer(Convert.ToInt64(splitMsg[i]));
                    }
                }

                if (splitMsg[0] == "t_data") { // Used for complex tile data
                    if (splitMsg[1] == "door") {
                        if (GameLoop.World.CurrentMap.GetTileAt<TileBase>(Convert.ToInt32(splitMsg[2]), Convert.ToInt32(splitMsg[3])) is TileDoor door) {
                            if (splitMsg[4] == "open") {
                                door.Open();
                            } else {
                                door.Close();
                            }

                            if(splitMsg[5] == "lock") {
                                door.Lock();
                            } else {
                                door.Unlock();
                            }

                            GameLoop.UIManager.MapConsole.IsDirty = true;
                        }
                    }
                }
            }

            if (channelId == 1) { // Chat Processing 
                GameLoop.UIManager.ChatLog.Add(System.Text.Encoding.UTF8.GetString(data));

               // if (myUID == hostUID) { SendNetMessage(channelId, data, userId); }
            }

            if (channelId == 2) { // World Data Processing
                var encoded = System.Text.Encoding.UTF8.GetString(data);

                if (encoded != "a") {
                    GameLoop.UIManager.MessageLog.Add("Map data received");
                    TileBase[] pretiles = Utils.GetMapFromString(encoded);
                    TileBase[] tiles = new TileBase[encoded.Length];
                    tiles = pretiles;
                    Map newMap = new Map(tiles);
                    GameLoop.World.CurrentMap = newMap;
                    //GameLoop.UIManager.MapConsole = new SadConsole.ScrollingConsole(GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height, Global.FontDefault, new Rectangle(0, 0, GameLoop.GameWidth, GameLoop.GameHeight), newMap.Tiles);
                    //GameLoop.UIManager.MapWindow.Invalidate();
                    GameLoop.UIManager.MapConsole.SetSurface(tiles, 100, 100);

                    GameLoop.UIManager.MessageLog.Add("Map processed " + GameLoop.World.CurrentMap.Tiles.Length);
                }
            }
        }

        public void SendNetMessage(byte channel, byte[] packet, long ignoredID=0) {
            var lobbyManager = discord.GetLobbyManager();

            try { 
                foreach (Discord.User user in lobbyManager.GetMemberUsers(lobbyID)) {
                    lobbyManager.SendNetworkMessage(lobbyID, user.Id, channel, packet);
                }
            } catch (Discord.ResultException e) {
            }

        }

        public NetworkingManager() {
            discord = new Discord.Discord(applicationID, (UInt64)Discord.CreateFlags.Default);
            userManager = discord.GetUserManager();
            discord.RunCallbacks();

            if (userManager != null) {

                try {
                    myUID = userManager.GetCurrentUser().Id;
                } catch (Discord.ResultException e) {
                    myUID = 0;
                }

            }
        }

        public void changeClientTarget(string cID) { // Only used for testing multiple clients on a single computer
            System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", cID);
            discord = new Discord.Discord(applicationID, (UInt64)Discord.CreateFlags.Default);

            userManager = discord.GetUserManager();
            userManager.OnCurrentUserUpdate += currentUserUpdate;
            discord.RunCallbacks();
        }

        private void currentUserUpdate() {
            myUID = userManager.GetCurrentUser().Id;
        }

        public void Update() {
            discord.RunCallbacks();
            
            try {
                if (myUID != userManager.GetCurrentUser().Id) {
                    myUID = userManager.GetCurrentUser().Id;

                    GameLoop.World.players.Add(myUID, GameLoop.World.players[0]);
                    GameLoop.World.players.Remove(0);
                    updateUID = false;
                }
            } catch (Discord.ResultException e) {

            }
        }
    }
}
