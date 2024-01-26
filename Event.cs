using HarmonyLib;
using Splotch.Event.GameEvents;
using Splotch.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Splotch.Event
{

    /// <summary>
    /// Manages various events that happen in-game
    /// </summary>
    public static class EventManager
    {
        public static Dictionary<Type, List<MethodInfo>> registeredEventHandlers = new Dictionary<Type, List<MethodInfo>>();
        /// <summary>
        /// Registers a static event listener class containing functions tagged with <c>[EventHandler]</c>
        /// </summary>
        /// <param name="eventListener">The class</param>
        public static void RegisterEventListener(Type eventListener)
        {
            foreach (MethodInfo eventHandler in eventListener.GetMethods())
            {
                bool hasAttribute = false;
                foreach (CustomAttributeData attributeData in eventHandler.CustomAttributes)
                    if (attributeData.AttributeType == typeof(EventHandler))
                        hasAttribute = true;

                if (hasAttribute)
                {
                    if (eventHandler.IsStatic)
                    {
                        ParameterInfo[] eventHandlerArguments = eventHandler.GetParameters();

                        if (eventHandlerArguments.Length == 1)
                        {
                            Type eventType = eventHandlerArguments[0].ParameterType;
                            if (eventType.IsSubclassOf(typeof(Event)))
                            {
                                if (!registeredEventHandlers.ContainsKey(eventType))
                                {
                                    registeredEventHandlers.Add(eventType, new List<MethodInfo>()); 
                                }
                                registeredEventHandlers[eventType].Add(eventHandler);

                                Logger.Debug($"Successfully registered event handler \"{eventHandler.FullDescription()}\"!");
                            }
                            else
                            {
                                Logger.Error($"Could not register event handler \"{eventHandler.FullDescription()}\" as the argument \"{eventType} {eventHandlerArguments[0].Name}\" does not extend \"{nameof(Event)}\"!");
                            }
                        }
                        else
                        {
                            Logger.Error($"Could not register event handler \"{eventHandler.FullDescription()}\" as it has {eventHandlerArguments.Length} arguments, not 1!");
                        }
                    }
                    else
                    {
                        Logger.Error($"Could not register event handler \"{eventHandler.FullDescription()}\" as it is not static!");
                    }
                }
            }
        }

        /// <summary>
        /// Searches for Events and runs their harmony patches
        /// </summary>
        internal static void PatchEventTypes()
        {
            // retrieve all types that extend Event
            Type[] eventExtensions = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(type => type.IsSubclassOf(typeof(Event))
               ).ToArray();

            // run all patches on Event extensions
            foreach (Type eventExtension in eventExtensions) { 
                Patcher.harmony.PatchAll(eventExtension);
                // run the startup method
                eventExtension.GetMethod(nameof(Event.Setup), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).Invoke(null, null);

            }


        }

        /// <summary>
        /// Loads the patcher
        /// </summary>
        internal static void Load()
        {
            PatchEventTypes();
        }
    }

    /// <summary>
    /// A basic event, any extensions of it will automatically be loaded
    /// </summary>
    public abstract class Event
    {
        /// <summary>
        /// Called after the event's patches are ran
        /// </summary>
        public static void Setup()
        {

        }

        /// <summary>
        /// Call this inside of your harmony patch to run all of the registered event handlers
        /// </summary>
        /// <param name="e">An instance of the event class, should be created during the harmony patch</param>
        public static void RunHandlers(Event e)
        {
            if (EventManager.registeredEventHandlers.ContainsKey(e.GetType()))
            {
                foreach (MethodInfo method in EventManager.registeredEventHandlers[e.GetType()])
                {
                    try
                    {
                        method.Invoke(null, new object[] { e });
                    } catch(Exception ex)
                    {
                        Logger.Error($"An error occurred while running the event handler {method.FullDescription()}:\n{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// An event that can be cancelled
    /// </summary>
    public interface Cancellable
    {
        bool Cancelled { get; set; }
    }

    /// <summary>
    /// Marks functions as event handlers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute { };
}

namespace Splotch.Event.AbilityEvents
{
    /// <summary>
    /// A class that should be extended by any ability-related events
    /// </summary>
    public abstract class AbilityEvent : GameEvent, Cancellable
    {
        public bool Cancelled { get; set; } = false;

        /// <summary>
        /// Gets the ability related to the event
        /// </summary>
        /// <returns>The ability</returns>
        public abstract Ability GetAbility();

        /// <summary>
        /// Returns the private abilityComponents field in the ability
        /// </summary>
        /// <returns>A list of abilityComponents</returns>
        public IAbilityComponent[] GetAbilityComponents()
        {
            Ability ability = GetAbility();
            var field = typeof(Ability).GetField("abilityComponents", BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(ability) as IAbilityComponent[];
        }
    }

    /// <summary>
    /// Called when a player begins holding down the button for an ability
    /// </summary>
    public class AbilityEnterEvent : AbilityEvent
    {
        internal AbilityEnterEvent(Ability ability)
        {
            _ability = ability;
        }


        [HarmonyPatch(typeof(Ability), nameof(Ability.EnterAbility))]
        [HarmonyPrefix]
        public static bool Patch(Ability __instance)
        {
            AbilityEnterEvent e = new AbilityEnterEvent(__instance);
            RunHandlers(e);
            return !e.Cancelled;
        }

        private readonly Ability _ability;
        public override Ability GetAbility()
        {
            return _ability;
        }
    }

    /// <summary>
    /// Called when a player stops holding down the button for an ability
    /// </summary>
    public class AbilityExitEvent : AbilityEvent
    {
        internal AbilityExitEvent(Ability ability)
        {
            _ability = ability;
        }

        [HarmonyPatch(typeof(Ability), nameof(Ability.ExitAbility))]
        [HarmonyPrefix]
        public static bool Patch(Ability __instance)
        {
            AbilityExitEvent e = new AbilityExitEvent(__instance);
            RunHandlers(e);
            return !e.Cancelled;
        }

        private readonly Ability _ability;
        public override Ability GetAbility()
        {
            return _ability;
        }
    }

    
}
namespace Splotch.Event.PlayerEvents
{
    /// <summary>
    /// A class that should be extended by any player-related events
    /// </summary>
    public abstract class PlayerEvent : GameEvent
    {
        /// <summary>
        /// Retrieves the player related to the event
        /// </summary>
        /// <returns></returns>
        public abstract Player GetPlayer();

        public SlimeController GetSlimeController()
        {
            return Splotch.GetSlimeControllers()[GetPlayer().Id - 1];
        }
    }

    public class PlayerDeathEvent : PlayerEvent
    {
        internal PlayerDeathEvent(Player player, CauseOfDeath causeOfDeath)
        {
            _player = player;
            _causeOfDeath = causeOfDeath;
        }


        private readonly Player _player;
        private readonly CauseOfDeath _causeOfDeath;

        public override Player GetPlayer()
        {
            return _player;
        }

        /// <summary>
        /// Gets the cause of death
        /// </summary>
        /// <returns>the cause of death</returns>
        public CauseOfDeath GetCauseOfDeath()
        {
            return _causeOfDeath;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Kill))]
        [HarmonyPrefix]
        public static void Patch(Player __instance, ref CauseOfDeath causeOfDeath)
        {
            PlayerDeathEvent e = new PlayerDeathEvent(__instance, causeOfDeath);
            RunHandlers(e);
        }
    }

    /// <summary>
    /// Called when the simulation for a player updates
    /// </summary>
    public class PlayerTickEvent : PlayerEvent
    {
        internal PlayerTickEvent(PlayerBody playerBody)
        {
            FieldInfo type = typeof(PlayerBody).GetField("idHolder", BindingFlags.NonPublic | BindingFlags.Instance);
            IPlayerIdHolder idHolder = (IPlayerIdHolder) type.GetValue(playerBody);
            Player player = PlayerHandler.Get().GetPlayer(idHolder.GetPlayerId());
            _player = player;
            _playerBody = playerBody;
        }


        private readonly Player _player;
        private readonly PlayerBody _playerBody;
        public override Player GetPlayer()
        {
            return _player;
        }

        /// <summary>
        /// Gets the PlayerBody object of the event
        /// </summary>
        /// <returns>The PlayerBodt object</returns>
        public PlayerBody GetPlayerBody()
        {
            return _playerBody;
        }

        [HarmonyPatch(typeof(PlayerBody), nameof(PlayerBody.UpdateSim))]
        [HarmonyPrefix]
        public static void Patch(ref PlayerBody __instance)
        {
            PlayerTickEvent e = new PlayerTickEvent(__instance);
            RunHandlers(e);
        }
    }

}

namespace Splotch.Event.GameEvents
{
    public abstract class GameEvent : Event, Cancellable
    {
        public bool Cancelled { get; set; } = false;

    }
}