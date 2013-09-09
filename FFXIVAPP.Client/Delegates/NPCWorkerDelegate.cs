﻿    // FFXIVAPP.Client
// NPCWorkerDelegate.cs
// 
// © 2013 Ryan Wilson

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Client.Helpers.SocketIO;
using FFXIVAPP.Client.Memory;

#endregion

namespace FFXIVAPP.Client.Delegates
{
    internal static class NPCWorkerDelegate
    {
        #region Property Backings

        private static List<uint> _pets;

        public static List<uint> Pets
        {
            get
            {
                return _pets ?? (_pets = new List<uint>
                {
                    1398,
                    1399,
                    1400,
                    1401,
                    1402,
                    1403,
                    1404,
                    2095
                });
            }
        }

        #endregion

        #region Declarations

        public static NPCEntry CurrentUser;
        public static readonly List<NPCEntry> MonsterList = new List<NPCEntry>();

        #endregion

        /// <summary>
        /// </summary>
        public static void OnNewNPC(List<NPCEntry> npcEntry)
        {
            if (!npcEntry.Any())
            {
                return;
            }
            CurrentUser = npcEntry.First();
            Func<bool> saveToDictionary = delegate
            {
                foreach (var entry in npcEntry.Where(e => e.NPCType == NPCType.Monster)
                                              .Where(entry => MonsterList.All(m => m.ID != entry.ID) && !Pets.Contains(entry.ModelID)))
                {
                    MonsterList.Add(entry);
                }
                return true;
            };
            saveToDictionary.BeginInvoke(delegate
            {
                const int chunkSize = NPCEntryHelper.ChunkSize;
                var chunksProcessed = NPCEntryHelper.ChunksProcessed;
                if (MonsterList.Count <= (chunkSize * (chunksProcessed + 1)))
                {
                    return;
                }
                if (!NPCEntryHelper.Processing)
                {
                    NPCEntryHelper.ProcessUpload(new List<NPCEntry>(MonsterList.Skip(chunksProcessed * chunkSize)));
                }
            }, saveToDictionary);
        }
    }
}
