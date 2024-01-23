using HarmonyLib;
using Splotch.Event.AbilityEvents;
using Splotch.Loader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;

namespace Splotch.Event
{

    public static class EventManager
    {
        public static Dictionary<Type, List<MethodInfo>> registeredEventHandlers = new Dictionary<Type, List<MethodInfo>>();
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

                                Logger.Log($"Successfully registered event handler \"{eventHandler.Name}\"!");
                            }
                            else
                            {
                                Logger.Error($"Could not register event handler \"{eventHandler.Name}\" as the argument \"{eventType} {eventHandlerArguments[0].Name}\" does not extend \"{nameof(Event)}\"!");
                            }
                        }
                        else
                        {
                            Logger.Error($"Could not register event handler \"{eventHandler.Name}\" as it has {eventHandlerArguments.Length} arguments, not 1!");
                        }
                    }
                    else
                    {
                        Logger.Error($"Could not register event handler \"{eventHandler.Name}\" as it is not static!");
                    }
                }
            }
        }

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
        internal static void Load()
        {
            PatchEventTypes();
        }
    }

    public abstract class Event
    {
        public static void Setup()
        {

        }

        public static void RunHandlers(Event e)
        {
            if (EventManager.registeredEventHandlers.ContainsKey(e.GetType()))
            {
                foreach (MethodInfo method in EventManager.registeredEventHandlers[e.GetType()])
                {
                    method.Invoke(null, new object[] { e });
                }
            }
        }
    }

    public abstract class CancellableEvent : Event
    {
        public bool Cancelled { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute { };
}

namespace Splotch.Event.AbilityEvents
{
    public abstract class AbilityEvent : CancellableEvent
    {
        public abstract Ability GetAbility();
        public IAbilityComponent[] GetAbilityComponents()
        {
            Ability ability = GetAbility();
            var field = typeof(Ability).GetField("abilityComponents", BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(ability) as IAbilityComponent[];
        }
    }
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
    public abstract class PlayerEvent : Event
    {
        public abstract Player GetPlayer();
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