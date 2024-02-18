using HarmonyLib;
using Splotch.Loader;
using Splotch.Loader.ModLoader;
using Splotch.UserInterface;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Splotch.Network
{
    public abstract class Packet
    {
        public static Packet instance;
        public Packet(ushort id)
        {
            instance = this;
            this.id = id;
        }

        public static void Send(byte[] data, Connection connection)
        {
            Networker.SendMessageBypass(connection, SplotchUtils.CombineArrays<byte>(BitConverter.GetBytes(instance.id + 1), data));
        }

        public readonly ushort id;
        public abstract void OnMessage(byte[] data, Connection connection, NetIdentity netIdentity);
    }

    public class TestPacket : Packet
    {
        public TestPacket(ushort id) : base(id)
        {
        }

        public override void OnMessage(byte[] data, Connection connection, NetIdentity netIdentity)
        {
            SplotchGUI.ShowPopup(Encoding.ASCII.GetString(data));
        }
    }

    public class PacketArrangementPacket : Packet
    {
        public PacketArrangementPacket(ushort id) : base(id)
        {
        }

        public static void SendPacket(Connection connection)
        {
            var data = Encoding.ASCII.GetBytes(string.Join("%", Networker.registeredPackets.Keys.ToArray()));
            Send(data, connection);
        }

        public override void OnMessage(byte[] data, Connection connection, NetIdentity netIdentity)
        {
            if (SteamManager.instance.currentLobby.IsOwnedBy(netIdentity.SteamId))
            {
                string[] ids = Encoding.ASCII.GetString(data).Split('%');
                Networker.packetTypes = new Packet[ids.Length];
                ushort i = 0;
                foreach (var id in ids)
                {
                    Networker.packetTypes[i] = (Packet)AccessTools.Constructor(Networker.registeredPackets[id]).Invoke(new object[] { i });
                    i++;
                }
            }
        }
    }

    public static class Networker
    {
        internal static Dictionary<string, Type> registeredPackets = new Dictionary<string, Type>();
        public static Packet[] packetTypes;

        private static Dictionary<Friend, bool> SplotchPresent = new Dictionary<Friend, bool>();
        private static Dictionary<Friend, NetworkedSplotchInfo> SplotchInfoPerFriend = new Dictionary<Friend, NetworkedSplotchInfo>();
        private static NetworkedSplotchInfo? LobbySplotchInfo = null;
        internal static void Load()
        {
            Patcher.harmony.PatchAll(typeof(Networker));
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;

            MethodInfo baseMethod = AccessTools.Method(typeof(SteamManager), "AddMember");
            HarmonyMethod patchMethod = new HarmonyMethod(typeof(Networker), nameof(Networker.OnConnection));
            Patcher.harmony.Patch(baseMethod, postfix: patchMethod);

        }

        public static void OnConnection(Lobby lobby, Friend newPlayer)
        {
            if (lobby.IsOwnedBy(SteamClient.SteamId))
            {
                var connection = SteamManager.instance.connectedPlayers.Last();
                if (connection.id == SteamClient.SteamId)
                {
                    Networker.packetTypes = new Packet[Networker.registeredPackets.Count];
                    ushort i = 0;
                    foreach (var registeredPacket in registeredPackets.Values)
                    {
                        Networker.packetTypes[i] = (Packet)AccessTools.Constructor(registeredPacket).Invoke(new object[] { i });
                        i++;
                    }
                }
                else
                {
                    PacketArrangementPacket.SendPacket(connection.Connection);
                    TestPacket.Send(Encoding.ASCII.GetBytes("Hello world!"), connection.Connection);
                }
            }
        }

        public static bool GetSplotchPresent(Friend friend) 
        {
            if (SplotchPresent.ContainsKey(friend)) return SplotchPresent[friend];
            var splotchPresent = SteamManager.instance.currentLobby.GetMemberData(friend, "splotchPresent") == "true";
            SplotchPresent[friend] = splotchPresent;
            return splotchPresent;
        }

        public static Friend? GetFriendFromConnection(Connection connection)
        {
            var id = GetSteamConnectionFromConnection(connection).id;
            foreach (var friend in SteamManager.instance.currentLobby.Members)
            {
                if (friend.Id == id) return friend;
            }
            return null;
        }

        public static SteamConnection GetSteamConnectionFromConnection(Connection connection)
        {
            foreach (var steamConnection in SteamManager.instance.connectedPlayers)
            {
                if (steamConnection.Connection == connection) return steamConnection;
            }
            return null;
        }

        public static NetworkedSplotchInfo GetSplotchInfo(Friend friend)
        {
            if (SplotchInfoPerFriend.ContainsKey(friend)) return SplotchInfoPerFriend[friend];
            var splotchInfo = NetworkedSplotchInfo.FromString(SteamManager.instance.currentLobby.GetMemberData(friend, "splotchInfo"));
            SplotchInfoPerFriend[friend] = splotchInfo;
            return splotchInfo;
        }

        public static NetworkedSplotchInfo GetLobbySplotchInfo()
        {
            if (LobbySplotchInfo.HasValue) return LobbySplotchInfo.Value;
            var splotchInfo = NetworkedSplotchInfo.FromString(SteamManager.instance.currentLobby.GetData("splotchInfo"));
            LobbySplotchInfo = splotchInfo;
            return splotchInfo;
        }

        private static void OnLobbyCreatedCallback(Result result, Lobby lobby)
        {
            if (result == Result.OK)
            {
                lobby.SetData("splotchPresent", "true");
                lobby.SetData("splotchInfo", NetworkedSplotchInfo.FromCurrent().ToString());
            }
        }

        private static void OnLobbyEnteredCallback(Lobby lobby)
        {
            SplotchPresent.Clear();
            SplotchInfoPerFriend.Clear();
            LobbySplotchInfo = null;

            lobby.SetMemberData("splotchPresent", "true");
            lobby.SetMemberData("splotchInfo", NetworkedSplotchInfo.FromCurrent().ToString());

            var data = GetLobbySplotchInfo();
            NetworkedSplotchInfo.ModNameCheck(data, NetworkedSplotchInfo.FromCurrent());
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Connection), nameof(Connection.SendMessage), new[] { typeof(byte[]), typeof(SendType) })]
        public static Result SendMessageBypass(object instance, byte[] data, SendType sendType = SendType.Reliable)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
        }

        [HarmonyPatch(typeof(Connection), nameof(Connection.SendMessage), new[] { typeof(byte[]), typeof(SendType) })]
        [HarmonyPrefix]
        public static void SendMessage(ref Connection __instance, ref byte[] data, ref SendType sendType)
        {
            Friend? instanceFriend = GetFriendFromConnection(__instance);
            if (instanceFriend.HasValue && GetSplotchPresent(instanceFriend.Value))
                data = SplotchUtils.CombineArrays<byte>(BitConverter.GetBytes((ushort) 0), data);
        }

        [HarmonyPatch(typeof(SteamSocket), nameof(SteamSocket.OnMessage))]
        [HarmonyPrefix]
        public static bool OnMessage(ref Connection connection, ref NetIdentity identity, ref IntPtr data, ref int size, ref long messageNum, ref long recvTime, ref int channel)
        {
            byte[] specialData = new byte[2];
            Marshal.Copy(data, specialData, 0, 2);

            ushort messageType = BitConverter.ToUInt16(specialData, 0);

            size -= 2;
            data = IntPtr.Add(data, 2);
            if (messageType == 0) return true;  
            else
            {
                byte[] buffer = new byte[size];
                Marshal.Copy(data, buffer, 0, size);
                packetTypes[messageType - 1].OnMessage(buffer, connection, identity);
            }
            return messageType == 0;
        }
    }
    public struct NetworkedModInfo
    {
        public string id;
        public string version;

        public override string ToString()
        {
            return id + "%" + version;
        }

        public static NetworkedModInfo FromString(string str)
        {
            var values = str.Split('%');
            return new NetworkedModInfo
            {
                id = values[0],
                version = values[1]
            };
        }

        public static NetworkedModInfo FromModInfo(ModInfo mod)
        {
            return new NetworkedModInfo
            {
                id = mod.id,
                version = mod.version
            };
        }
    }

    public struct NetworkedSplotchInfo
    {
        public List<NetworkedModInfo> modInfos;
        public string version;
        public static NetworkedSplotchInfo FromCurrent()
        {
            return new NetworkedSplotchInfo
            {
                version = VersionChecker.currentVersionString,

                modInfos = ModManager.loadedMods
                    .Where(modinfo => modinfo.requiredOnOtherClients).ToList()
                    .ConvertAll<NetworkedModInfo>(new Converter<ModInfo, NetworkedModInfo>(NetworkedModInfo.FromModInfo))
            };
        }

        public override string ToString()
        {
            return version + "\n" + string.Join("\n", modInfos);
        }

        public static NetworkedSplotchInfo FromString(string str)
        {
            NetworkedSplotchInfo networkedSplotchInfo = new NetworkedSplotchInfo();
            List<string> data = new List<string>(str.Split('\n'));
            networkedSplotchInfo.version = data[0];
            data.RemoveAt(0);
            networkedSplotchInfo.modInfos = data.ConvertAll<NetworkedModInfo>(
                new Converter<string, NetworkedModInfo>(NetworkedModInfo.FromString)
                );
            return networkedSplotchInfo;
        }

        public bool HasMod(NetworkedModInfo targetModInfo)
        {
            return modInfos.Any(modinfo => targetModInfo.id == modinfo.id);
        }

        public static void ModNameCheck(NetworkedSplotchInfo targetMods, NetworkedSplotchInfo currentMods)
        {
            List<string> modsToAdd = new List<string>();
            List<string> modsToRemove = new List<string>();
            foreach (var targetModInfo in targetMods.modInfos)
            {
                if (!currentMods.HasMod(targetModInfo))
                    modsToAdd.Add(targetModInfo.id);
            }

            foreach (var currentModInfo in currentMods.modInfos)
            {
                if (!targetMods.HasMod(currentModInfo))
                    modsToRemove.Add(currentModInfo.id);
            }

            if (modsToAdd.Any() || modsToRemove.Any())
            {
                string popuptext = "Your mods do not match the host!";
                if (modsToAdd.Any())
                    popuptext += "\nInstall the following mods: " + string.Join(", ", modsToAdd);
                if (modsToAdd.Any())
                    popuptext += "\nUninstall the following mods: " + string.Join(", ", modsToRemove);

                SplotchGUI.ShowPopup(popuptext.Split(new[] { '\n' }, 2)[0], popuptext.Split(new[] { '\n' }, 2)[1]);
                
                Logger.Warning(popuptext);
            }
        }
    }
}
