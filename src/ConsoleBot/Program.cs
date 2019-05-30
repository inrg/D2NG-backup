﻿using D2NG;
using Serilog;
using System;
using McMaster.Extensions.CommandLineUtils;
using D2NG.BNCS.Packet;
using System.Linq;
using D2NG.MCP;
using System.Collections.Generic;
using Serilog.Events;
using System.Threading;

namespace ConsoleBot
{
    class Program
    {
        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option(Description = "Config File", LongName = "config", ShortName = "c")]
        public string ConfigFile { get; }

        [Option]
        public bool Verbose { get; set; }

        private readonly Client Client = new Client();

        private Config Config;

        private LogEventLevel LogLevel() => Verbose ? LogEventLevel.Verbose: LogEventLevel.Debug;

        private void OnExecute()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Is(LogLevel())
                .CreateLogger();

            if (ConfigFile != null)
            {
                Config = Config.FromFile(this.ConfigFile);
            }

            //Client.OnReceivedPacketEvent(Sid.CHATEVENT, HandleChatEvent);

            try {
                Client.Connect(
                    Prompt.GetString($"Realm:", Config.Realm, ConsoleColor.Green),
                    Prompt.GetString($"Classic Key:", Config.ClassicKey, ConsoleColor.Blue),
                    Prompt.GetString($"Expansion Key:", Config.ExpansionKey, ConsoleColor.Blue));

                var characters = Client.Login(
                    Prompt.GetString("Username: ", Config.Username, ConsoleColor.Green), 
                    Prompt.GetPassword("Password: ", ConsoleColor.Red));

                Client.SelectCharacter(SelectCharacter(characters));

                Client.Chat.EnterChat();

                Client.Chat.JoinChannel("D2NG");

                Client.Chat.Send("Hello World!");
                Client.Chat.Emote("is alive");

                Client.CreateGame(Difficulty.Normal, $"d2ng{new Random().Next(1000)}", "d2ng");
                Thread.Sleep(30_000);
                Client.Game.LeaveGame();
                Thread.Sleep(30_000);
                Client.CreateGame(Difficulty.Normal, $"ngd2{new Random().Next(1000)}", "d2ng");
                Thread.Sleep(30_000);
                Client.Game.LeaveGame();
            }
            catch (Exception e)
            {
                Log.Error(e, "Unhandled Exception");
            }
        }

        private Character SelectCharacter(List<Character> characters)
        {
            var charsPrompt = characters
                .Select((c, index) => $"{index + 1}. {c.Name} - Level {c.Level} {c.Class}")
                .Aggregate((i, j) => i + "\n" + j);

            return characters[Prompt.GetInt($"Select Character:\n{charsPrompt}\n", 1, ConsoleColor.Green) - 1];
        }

        private static void HandleChatEvent(BncsPacket obj)
        {
            var packet = new ChatEventPacket(obj.Raw);
            Log.Information(packet.RenderText());
        }
    }
}
