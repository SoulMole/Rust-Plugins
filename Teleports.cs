using Network;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Teleports", "Lonely Planet", "1.1.4")]
    internal class Teleports : RustPlugin
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
        // Simplifed Version of NTeleportation
        // Credits to Nogrod for the original plugin (https://umod.org/plugins/nteleportation)
        /////////////////////////////////////////////////////////////////////////////////////////////

        private const string PermissionTp = "Teleports.tp";
        private const string PermissionTpTo = "Teleports.tphere";

        private Dictionary<string, BasePlayer> _ids = new Dictionary<string, BasePlayer>();
        private Dictionary<BasePlayer, string> _players = new Dictionary<BasePlayer, string>();

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Multiple Players"] = "<color=#cfcfcf>Multiple players found <color=#606060>•</color> '<color=#9c3bff>{0}</color>'</color>",
                ["No Player Found"] = "<color=#cfcfcf>No player with the identifer <color=#9c3bff>{0}</color> was found.</color>",
                ["Teleported To"] = "<color=#cfcfcf>You teleported to <color=#9c3bff>{0}</color>.</color>",
                ["Teleported Here"] = "<color=#cfcfcf>You teleported <color=#9c3bff>{0}</color> to <color=#9c3bff>{1}</color>.</color>",
                ["No Perms"] = "<color=#cfcfcf>You are not allowed to use. <color=#606060>•</color> <color=#9c3bff>tp</color></color>",
                ["Not Allowed"] = "<color=#cfcfcf>You are not allowed to teleport players to players with your current Rank</color>",
                ["Bad Syntax"] = "<color=#cfcfcf>Invalid Syntax <color=#606060>•</color> <color=#9c3bff>/tp <name|id> [name|id]</color></color>"
            }, this);
        }

        private void Init()
        {
            permission.RegisterPermission(PermissionTp, this);
            permission.RegisterPermission(PermissionTpTo, this);
        }

        [ChatCommand("tp")]
        private void chat_TP(BasePlayer player, string Command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PermissionTp))
            {
                player.ChatMessage(lang.GetMessage("No Perms", this));
                return;
            }
            if (args.Length == 1 || args.Length == 2)
            {
                BasePlayer target = FindPlayersSingle(args[0], player);
                Vector3 tarpos = target.transform.position;
                switch (args.Length)
                {
                    case 1:
                        if (target != null)
                        {
                            TeleportPlayer(player, tarpos);
                            player.ChatMessage(string.Format(lang.GetMessage("Teleported To", this), target.displayName.Sanitize()));
                        }
                    break;

                    case 2:
                        if (!permission.UserHasPermission(player.UserIDString, PermissionTpTo))
                        {
                            player.ChatMessage(lang.GetMessage("Not Allowed", this));
                            return;
                        }
                        BasePlayer secondTarget = FindPlayersSingle(args[1], player);
                        Vector3 tarpos2 = secondTarget.transform.position;
                        if (target != null || secondTarget != null)
                        {
                            TeleportPlayer(target, tarpos2);
                            player.ChatMessage(string.Format(lang.GetMessage("Teleported Here", this), target.displayName.Sanitize(), secondTarget.displayName.Sanitize()));
                        }
                    break;
                }
            }
            else
            {
                player.ChatMessage(lang.GetMessage("Bad Syntax", this));
                return;
            }
        }

        private BasePlayer FindPlayersSingle(string value, BasePlayer player)
        {
            if (string.IsNullOrEmpty(value)) return null;
            BasePlayer target;
            if (_ids.TryGetValue(value, out target) && target.IsValid())
            {
                return target;
            }
            var targets = FindPlayers(value, true);
            if (targets.Count <= 0)
            {
                player.ChatMessage(string.Format(lang.GetMessage("No Player Found", this), value.Sanitize()));
                return null;
            }
            if (targets.Count > 1)
            {
                player.ChatMessage(string.Format(lang.GetMessage("Multiple Players", this), GetMultiplePlayers(targets).Sanitize()));
                return null;
            }

            return targets.First();
        }


        private List<BasePlayer> FindPlayers(string arg, bool all = false)
        {
            var players = new List<BasePlayer>();

            if (string.IsNullOrEmpty(arg))
            {
                return players;
            }

            BasePlayer target;
            if (_ids.TryGetValue(arg, out target) && target.IsValid())
            {
                if (all || target.IsConnected)
                {
                    players.Add(target);
                    return players;
                }
            }

            foreach (var p in all ? BasePlayer.allPlayerList : BasePlayer.activePlayerList)
            {
                if (p == null || string.IsNullOrEmpty(p.displayName) || players.Contains(p))
                {
                    continue;
                }

                if (p.UserIDString == arg || p.displayName.Contains(arg, CompareOptions.OrdinalIgnoreCase))
                {
                    players.Add(p);
                }
            }

            return players;
        }

        private string _(string msgId, BasePlayer player, params object[] args)
        {
            var msg = lang.GetMessage(msgId, this, player?.UserIDString);
            return args.Length > 0 ? string.Format(msg, args) : msg;
        }

        private void PrintMsgL(BasePlayer player, string msgId, params object[] args)
        {
            if (player == null) return;
            PrintMsg(player, _(msgId, player, args));
        }

        private void PrintMsg(BasePlayer player, string msg)
        {
            if (player == null || string.IsNullOrEmpty(msg)) return;
            Player.Message(player, msg);
        }

        private void TeleportPlayer(BasePlayer player, Vector3 vectorpos)
        {
            if (player.IsConnected)
            {
                player.EndLooting();
                player.DismountObject();
                StartSleeping(player);
            }

            player.Teleport(vectorpos);
            if (player.IsConnected && !Net.sv.visibility.IsInside(player.net.group, vectorpos))
            {
                player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                player.ClientRPCPlayer(null, player, "StartLoading");
                player.SendEntityUpdate();
                player.UpdateNetworkGroup();
                player.SendNetworkUpdateImmediate(false);
            }

            player.metabolism.bleeding.value = 0;
            player.InitializeHealth(100, 100);
        }

        private void StartSleeping(BasePlayer player)
        {
            if (!player.IsSleeping())
            {
                player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, true);
                player.sleepStartTime = Time.time;
                BasePlayer.sleepingPlayerList.Add(player);
                player.CancelInvoke("InventoryUpdate");
                player.CancelInvoke("TeamUpdate");
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            var uid = UnityEngine.Random.Range(1000, 9999).ToString();
            var names = BasePlayer.activePlayerList.Select(x => x.displayName);

            while (_ids.ContainsKey(uid) || names.Any(name => name.Contains(uid)))
            {
                uid = UnityEngine.Random.Range(1000, 9999).ToString();
            }

            _ids[uid] = player;
            _players[player] = uid;
        }

        private string GetMultiplePlayers(List<BasePlayer> players)
        {
            var list = new List<string>();

            foreach (var player in players)
            {
                if (!_players.ContainsKey(player))
                {
                    OnPlayerConnected(player);
                }

                list.Add(string.Format("<color={0}>{1}</color> - {2}", "#FFA500", _players[player], player.displayName));
            }

            return string.Join(", ", list.ToArray());
        }
    }
}