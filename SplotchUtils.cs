using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Splotch
{
    /// <summary>
    /// A utility class with general static functions
    /// </summary>
    public static class SplotchUtils
    {
        /// <summary>
        /// Gets the GameSessionHandler
        /// </summary>
        /// <returns>the GameSessionHandler or null if it isn't instantized</returns>
        public static GameSessionHandler GetGameSessionHandler()
        {
            FieldInfo selfRefField = typeof(GameSessionHandler).GetField("selfRef", BindingFlags.Static | BindingFlags.NonPublic);
            return selfRefField.GetValue(null) as GameSessionHandler;
        }

        /// <summary>
        /// Gets the slime controllers
        /// </summary>
        /// <returns>A list of slime controllers</returns>
        public static SlimeController[] GetSlimeControllers()
        {
            FieldInfo slimeControllersField = typeof(GameSessionHandler).GetField("slimeControllers", BindingFlags.Instance | BindingFlags.NonPublic);
            return slimeControllersField.GetValue(GetGameSessionHandler()) as SlimeController[];
        }

        /// <summary>
        /// Gives access to the private field <c>gameOver</c> in <c>GameSessionHandler</c>
        /// </summary>
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

        /// <summary>
        /// Gives access to the private function <c>prepareNextLevel</c> in <c>GameSessionHandler</c>
        /// </summary>
        public static void PrepareNextLevel()
        {
            MethodInfo prepareNextLevelMethod = typeof(GameSessionHandler).GetMethod("prepareNextlevel", BindingFlags.Instance | BindingFlags.NonPublic);
            prepareNextLevelMethod.Invoke(GetGameSessionHandler(), null);
        }

        /// <summary>
        /// Retrieves the <c>Player</c> corresponding to a <c>PlayerBody</c>
        /// </summary>
        /// <param name="playerBody">The <c>PlayerBody</c> object</param>
        /// <returns>The corresponding <c>Player</c> object</returns>
        public static Player GetPlayerFromPlayerBody(PlayerBody playerBody)
        {
            FieldInfo type = typeof(PlayerBody).GetField("idHolder", BindingFlags.NonPublic | BindingFlags.Instance);
            IPlayerIdHolder idHolder = (IPlayerIdHolder)type.GetValue(playerBody);
            Player player = PlayerHandler.Get().GetPlayer(idHolder.GetPlayerId());
            return player;
        }

        /// <summary>
        /// Retrieves the <c>PlayerPhysics</c> corresponding to a <c>PlayerBody</c>
        /// </summary>
        /// <param name="playerBody">The <c>PlayerBody</c> object</param>
        /// <returns>The corresponding <c>PlayerPhysics</c> object</returns>
        public static PlayerPhysics GetPlayerPhysicsFromPlayerBody(PlayerBody playerBody)
        {
            FieldInfo physicsField = typeof(PlayerBody).GetField("physics", BindingFlags.NonPublic | BindingFlags.Instance);
            return physicsField.GetValue(playerBody) as PlayerPhysics;
        }
    }
}
