﻿using Discord;
using System;

namespace TearsInRain {
    class NetworkingManager {
        public Discord.Discord discord;
        private static long applicationID = 579827348665532425;

        public long myUID;
        public long hostUID;
        public long lobbyID;

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
        }

        public void SendNetMessage(byte channel, byte[] packet, long ignoredID=0) {
            var lobbyManager = discord.GetLobbyManager();

            foreach (Discord.User user in lobbyManager.GetMemberUsers(lobbyID)) {
                lobbyManager.SendNetworkMessage(lobbyID, user.Id, channel, packet);
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
