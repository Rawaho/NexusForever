﻿using System;
using System.Collections.Generic;
using NLog;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Database.Character.Model;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PathManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly Player player;
        private PathEntry pathEntry;

        public PathManager() { }

        /// <summary>
        /// Create a new <see cref="PathManager"/> from <see cref="Player"/> database model.
        /// </summary>
        public PathManager(Player owner, Character model)
        {
            player = owner;

            if(model.CharacterPath != null)
                pathEntry = new PathEntry(model.CharacterPath);
        }

        /// <summary>
        /// Create a new <see cref="PathEntry"/>.
        /// </summary>
        public PathEntry PathCreate(byte activePath, ulong amount = 0)
        {
            if (activePath < 0)
                return null;

            PathUnlocked pathUnlocked = CalculatePathUnlocked(activePath);

            pathEntry = new PathEntry(
                player.CharacterId,
                (Path)activePath,
                pathUnlocked
            );
            return pathEntry;
        }

        /// <summary>
        /// Function to calculate <see cref="PathUnlocked"/> from <see cref="Path"/>
        /// </summary>
        /// <param name="activePath"></param>
        /// <returns></returns>
        public PathUnlocked CalculatePathUnlocked(byte activePath)
        {
            PathUnlocked pathUnlocked;
            switch ((Path)activePath)
            {
                case Path.Soldier:
                    pathUnlocked = PathUnlocked.Soldier;
                    break;
                case Path.Settler:
                    pathUnlocked = PathUnlocked.Settler;
                    break;
                case Path.Scientist:
                    pathUnlocked = PathUnlocked.Scientist;
                    break;
                case Path.Explorer:
                    pathUnlocked = PathUnlocked.Explorer;
                    break;
                default:
                    pathUnlocked = 0;
                    break;
            }

            return pathUnlocked;
        }

        public PathEntry GetPath(ulong characterId)
        {
            if (pathEntry == null)
            {
                log.Warn("No path associated with character ID. Defaulting to Soldier.");
                return PathCreate((byte)Path.Soldier);
            }

            return pathEntry;
        }

        public void Save(CharacterContext context)
        {
            log.Info("PathManager.Save Called");
            pathEntry.Save(context);
        }

        /// <summary>
        /// Create a new <see cref="CharacterCurrency"/>.
        /// </summary>
        //public Currency CurrencyCreate(CurrencyTypeEntry currencyEntry, ulong amount = 0)
        //{
        //    if (currencyEntry == null)
        //        return null;

        //    if (currencies.ContainsKey((byte)currencyEntry.Id))
        //        throw new ArgumentException($"Currency {currencyEntry.Id} is already added to the player!");

        //    Currency currency = new Currency(
        //        player.CharacterId,
        //        currencyEntry,
        //        amount
        //    );
        //    currencies.Add((byte)currencyEntry.Id, currency);
        //    return currency;
        //}

    }
}
