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

            foreach (Discord.User user in lobbyManager.GetMemberUsers(lobbyID)) {
                lobbyManager.SendNetworkMessage(lobbyID, user.Id, channel, packet);
            }

            if (channel == 2) {
                GameLoop.UIManager.MessageLog.Add("Sent Map Packet");
            }


           //lobbyManager.FlushNetwork();


                //if (myUID != hostUID) {
                //    lobbyManager.SendNetworkMessage(lobbyID, hostUID, channel, packet);

                //    lobbyManager.FlushNetwork();
                //} else {
                //    foreach (Discord.User user in lobbyManager.GetMemberUsers(lobbyID)) {
                //        if (ignoredID == user.Id) { continue; }
                //        lobbyManager.SendNetworkMessage(lobbyID, user.Id, channel, packet);
                //    }


                //    lobbyManager.FlushNetwork();
                //}

            }

        public NetworkingManager() {
            discord = new Discord.Discord(applicationID, (UInt64)Discord.CreateFlags.Default);
            userManager = discord.GetUserManager();
            discord.RunCallbacks();
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
        }
    }
}
