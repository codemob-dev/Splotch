using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Splotch
{
    public static class Splotch
    {
        public static GameSessionHandler GetGameSessionHandler()
        {
            FieldInfo selfRefField = typeof(GameSessionHandler).GetField("selfRef", BindingFlags.Static | BindingFlags.NonPublic);
            return selfRefField.GetValue(null) as GameSessionHandler;
        }

        public static SlimeController[] GetSlimeControllers()
        {
            FieldInfo slimeControllersField = typeof(GameSessionHandler).GetField("slimeControllers", BindingFlags.Instance | BindingFlags.NonPublic);
            return slimeControllersField.GetValue(GetGameSessionHandler()) as SlimeController[];
        }

        public static bool IsGameOver
        {
            get
            {
                FieldInfo gameOverField = typeof(GameSessionHandler).GetField("gameOver", BindingFlags.Instance | BindingFlags.NonPublic);
                return (bool) gameOverField.GetValue(GetGameSessionHandler());
            }
            set
            {
                FieldInfo gameOverField = typeof(GameSessionHandler).GetField("gameOver", BindingFlags.Instance | BindingFlags.NonPublic);
                gameOverField.SetValue(GetGameSessionHandler(), value);
            }
        }

        public static void PrepareNextLevel()
        {
            MethodInfo prepareNextLevelMethod = typeof(GameSessionHandler).GetMethod("prepareNextlevel", BindingFlags.Instance | BindingFlags.NonPublic);
            prepareNextLevelMethod.Invoke(GetGameSessionHandler(), null);
        }

        public static Player GetPlayerFromPlayerBody(PlayerBody playerBody)
        {
            FieldInfo type = typeof(PlayerBody).GetField("idHolder", BindingFlags.NonPublic | BindingFlags.Instance);
            IPlayerIdHolder idHolder = (IPlayerIdHolder)type.GetValue(playerBody);
            Player player = PlayerHandler.Get().GetPlayer(idHolder.GetPlayerId());
            return player;
        }
    }
}
