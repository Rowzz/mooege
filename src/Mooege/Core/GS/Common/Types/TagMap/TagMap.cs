﻿/*
 * Copyright (C) 2011 - 2012 mooege project - http://www.mooege.org
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CrystalMpq;
using Gibbed.IO;
using System.Reflection;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Common.MPQ;
using Mooege.Common.MPQ.FileFormats.Types;
using Mooege.Common.Storage;

namespace Mooege.Core.GS.Common.Types.TagMap
{
    /// <summary>
    /// Implementation of a dictionary-like tag map.
    /// You can access elements either using a key object that identifies which of the TagKey
    /// fields holds your value or by accessing the entry directly with the interger tag id.
    /// </summary>
    public class TagMap : ISerializableData, IEnumerable<TagMapEntry>
    {

        public int TagMapSize { get; private set; }

        [PersistentProperty("TagMapEntries")]
        private Dictionary<int, TagMapEntry> _tagMapEntries { get; set; }

        [Obsolete("Use TagKeys instead. If it is missing create it.")]
        public List<TagMapEntry> TagMapEntries
        {
            get
            {
                return _tagMapEntries.Values.ToList();
            }
        }

        # region compile a dictionary to access keys from ids. If you need a readable name for a TagID, look up its key and get its name.
        // This is a combination of all dictionaries, so it HAS COLLISIONS
        // This is mainly for debugging purposes, if you have a tagID and want to know what key it is / might be
        private static Dictionary<int, List<TagKey>> tags = new Dictionary<int, List<TagKey>>();

        public static List<TagKey> GetKeys(int index)
        {
            return tags.ContainsKey(index) ? tags[index] : new List<TagKey>();
        }

        static TagMap()
        {
            foreach (Type t in new Type[] { typeof(MarkerKeys), typeof(ActorKeys), typeof(PowerKeys), typeof(AnimationSetKeys) })
                foreach (FieldInfo field in t.GetFields())
                {
                    TagKey key = field.GetValue(null) as TagKey;
                    if (!tags.ContainsKey(key.ID))
                        tags.Add(key.ID, new List<TagKey>());

                    tags[key.ID].Add(key);
                }
        }
        #endregion

        public TagMap()
        {
            _tagMapEntries = new Dictionary<int, TagMapEntry>();
        }


        [Obsolete("Use TagKeys instead. If it is missing create it")]
        public bool ContainsKey(int key) { return _tagMapEntries.ContainsKey(key); }
        public bool ContainsKey(TagKey key) { return _tagMapEntries.ContainsKey(key.ID); }

        public void Add(TagKey key, TagMapEntry entry) { _tagMapEntries.Add(key.ID, entry); }

        public void Replace(TagKey key, TagMapEntry entry) // erekose
        {
            if (_tagMapEntries.ContainsKey(key.ID))
                _tagMapEntries[key.ID] = entry;
        }


        public void Read(MpqFileStream stream)
        {
            TagMapSize = stream.ReadValueS32();
            _tagMapEntries = new Dictionary<int, TagMapEntry>();

            for (int i = 0; i < TagMapSize; i++)
            {
                var entry = new TagMapEntry(stream);
                this._tagMapEntries.Add(entry.TagID, entry);
            }
        }

        #region accessors

        public int this[TagKeyInt key]
        {
            get
            {
                return key.GetValue(_tagMapEntries[key.ID]);
            }
            set
            {
                _tagMapEntries[key.ID].Int = value;
            }
        }

        public float this[TagKeyFloat key]
        {
            get
            {
                return key.GetValue(_tagMapEntries[key.ID]);
            }
        }

        public ScriptFormula this[TagKeyScript key]
        {
            get
            {
                return key.GetValue(_tagMapEntries[key.ID]);
            }
        }

        public SNOHandle this[TagKeySNO key]
        {
            get
            {
                return key.GetValue(_tagMapEntries[key.ID]);
            }
        }

        public GizmoGroup this[TagKeyGizmoGroup key]
        {
            get
            {
                return key.GetValue(_tagMapEntries[key.ID]);
            }
        }


        [Obsolete("Use TagKeys instead. If it is missing create it")]
        public TagMapEntry this[int key]
        {
            get
            {
                return _tagMapEntries[key];
            }
        }

        #endregion

        #region enumurators

        public IEnumerator<TagMapEntry> GetEnumerator()
        {
            return _tagMapEntries.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _tagMapEntries.Values.GetEnumerator();
        }

        #endregion
    }


    public abstract class TagKey
    {
        public int ID { get; private set; }
        public string Name { get; set; }

        public TagKey(int id) { ID = id; }
    }

    public class TagKeyInt : TagKey { public TagKeyInt(int id) : base(id) { } public int GetValue(TagMapEntry entry) { return entry.Int; } }
    public class TagKeyFloat : TagKey { public TagKeyFloat(int id) : base(id) { } public float GetValue(TagMapEntry entry) { return entry.Float; } }
    public class TagKeyScript : TagKey { public TagKeyScript(int id) : base(id) { } public ScriptFormula GetValue(TagMapEntry entry) { return entry.ScriptFormula; } }
    public class TagKeySNO : TagKey { public TagKeySNO(int id) : base(id) { } public SNOHandle GetValue(TagMapEntry entry) { return new SNOHandle(entry.Int); } }


    public class TagMapEntry
    {
        [PersistentProperty("Type")]
        public int Type { get; private set; }

        [PersistentProperty("TagID")]
        public int TagID { get; private set; }

        [PersistentProperty("ScriptFormula")]
        public ScriptFormula ScriptFormula { get; private set; }

        [PersistentProperty("Int")]
        public int Int { get; set; }

        [PersistentProperty("Float")]
        public float Float { get; private set; }

        public TagMapEntry() { }

        public TagMapEntry(int tag, int value, int type)
        {
            Type = type;
            TagID = tag;
            Int = value;
        }

        public override string ToString()
        {
            List<TagKey> keys = TagMap.GetKeys(TagID);

            if (keys.Count == 0)
                switch (Type)
                {
                    case 1: return String.Format("{0} = {1}", TagID.ToString(), Float);
                    case 4: return String.Format("{0} = {1}", TagID.ToString(), ScriptFormula);
                    default: return String.Format("{0} = {1}", TagID.ToString(), Int);
                }

            if (keys.Count == 1)
            {
                var value = keys.First().GetType().GetMethod("GetValue").Invoke(keys.First(), new object[] { this });
                return String.Format("{0} = {1}", keys.First().Name, value == null ? "null" : value.ToString());
            }

            return String.Format("Ambigious key: Depending of the context it one of {0}", String.Join(",", keys.Select(x => x.Name).ToArray()));

        }

        public TagMapEntry(MpqFileStream stream)
        {
            this.Type = stream.ReadValueS32();
            this.TagID = stream.ReadValueS32();

            switch (this.Type)
            {
                case 0:
                    this.Int = stream.ReadValueS32();
                    break;
                case 1:
                    Float = stream.ReadValueF32();
                    break;
                case 2: // SNO
                    this.Int = stream.ReadValueS32();
                    break;

                // TODO: Create strong type for gbid (at least i think they are)
                case 3:
                    this.Int = stream.ReadValueS32();
                    break;

                case 4:
                    this.ScriptFormula = new ScriptFormula(stream);
                    break;

                // TODO: Strong type for group hashes
                case 5:
                    this.Int = stream.ReadValueS32();
                    break;

                // Todo: Strong type for ... hmmm.. is that a gameattributeindex?
                case 6:
                    this.Int = stream.ReadValueS32();
                    break;

                // Todo: Strong type fo StartingLocationID
                case 7:
                    this.Int = stream.ReadValueS32();
                    break;

                default:
                    // if this break hits, blizz introduced a new key type and most likey we should have to react to it
                    System.Diagnostics.Debugger.Break();
                    this.Int = stream.ReadValueS32();
                    break;
            }
        }
    }
}