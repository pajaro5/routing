﻿// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Reminiscence.Arrays;
using System;
using System.Collections.Generic;

namespace Itinero.Data.Network.Restrictions
{
    /// <summary>
    /// Represents a restriction database.
    /// </summary>
    /// <remarks>A restriction is a sequence of vertices that is forbidden. A complex restriction is a restriction with > 1 vertex.</remarks>
    public class RestrictionsDb
    {
        private readonly ArrayBase<uint> _hashes; // holds pointers to a set of pointers in the index.
        private readonly ArrayBase<uint> _index; // holds pointers per hash to the actual restrictions.
        private readonly ArrayBase<uint> _restrictions; // holds the actual restrictions with a prefixed count.
        private const int BLOCKSIZE = 1000;
        private const int DEFAULT_HASHCOUNT = 1024 * 1024;
        private const uint NO_DATA = uint.MaxValue;

        private bool _hasComplexRestrictions; // flag to indicate presence of complex restrictions.

        /// <summary>
        /// Creates a new restrictions db.
        /// </summary>
        public RestrictionsDb(int hashes = DEFAULT_HASHCOUNT)
        {
            _hasComplexRestrictions = false;
            _hashes = new MemoryArray<uint>(hashes * 2);
            for (var i = 0; i < _hashes.Length; i++)
            {
                _hashes[i] = NO_DATA;
            }
            _restrictions = new MemoryArray<uint>(BLOCKSIZE);
            _index = new MemoryArray<uint>(BLOCKSIZE);
        }
        
        /// <summary>
        /// Creates a new restrictions db.
        /// </summary>
        private RestrictionsDb()
        {

        }
        
        private uint _nextIndexPointer = 0;
        private uint _nextRestrictionPointer = 0;
        private int _count = 0;

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public void Add(params uint[] restriction)
        {
            if (restriction == null) { throw new ArgumentNullException("restriction"); }
            if (restriction.Length == 0) { throw new ArgumentException("Restriction should contain one or more vertices.", "restriction"); }

            if (restriction.Length > 1)
            {
                _hasComplexRestrictions = true;
            }

            while (_nextRestrictionPointer + (uint)restriction.Length + 1 >= _restrictions.Length)
            {
                _restrictions.Resize(_restrictions.Length + BLOCKSIZE);
            }

            // add the data.
            _restrictions[_nextRestrictionPointer] = (uint)restriction.Length;
            for (var i = 0; i < restriction.Length; i++)
            {
                _restrictions[_nextRestrictionPointer + i + 1] = restriction[i];
            }

            // add by pointer.
            this.AddByPointer(_nextRestrictionPointer);

            _nextRestrictionPointer = _nextRestrictionPointer + 1 + (uint)restriction.Length;
            _count++;
        }

        /// <summary>
        /// Gets the number of restrictions in this db.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Switches the given two vertices.
        /// </summary>
        public void Switch(uint vertex1, uint vertex2)
        {
            // collect relevant restrictions.
            var restrictions = new HashSet<uint>();
            var hash = CalculateHash(vertex1);
            var indexPointer = _hashes[hash];
            if (indexPointer != NO_DATA)
            {
                var pSize = _index[indexPointer];
                for (var i = 0; i < pSize; i++)
                {
                    var restrictionPointer = _index[indexPointer + i + 1];
                    var restrictionSize = _restrictions[restrictionPointer];
                    for (var j = 0; j < restrictionSize; j++)
                    {
                        if (_restrictions[restrictionPointer + 1 + j] == vertex1)
                        {
                            restrictions.Add(restrictionPointer);
                            break;
                        }
                    }
                }
            }
            hash = CalculateHash(vertex2);
            indexPointer = _hashes[hash];
            if (indexPointer != NO_DATA)
            {
                var pSize = _index[indexPointer];
                for (var i = 0; i < pSize; i++)
                {
                    var restrictionPointer = _index[indexPointer + i + 1];
                    var restrictionSize = _restrictions[restrictionPointer];
                    for (var j = 0; j < restrictionSize; j++)
                    {
                        if (_restrictions[restrictionPointer + 1 + j] == vertex2)
                        {
                            restrictions.Add(restrictionPointer);
                            break;
                        }
                    }
                }
            }

            // switch vertices and add/remove restrictions to update index/hashes.
            foreach(var restrictionPointer in restrictions)
            {
                this.RemoveByPointer(restrictionPointer);

                var restrictionSize = _restrictions[restrictionPointer];
                for (var i = 0; i < restrictionSize; i++)
                {
                    if (_restrictions[restrictionPointer + 1 + i] == vertex2)
                    {
                        _restrictions[restrictionPointer + 1 + i] = vertex1;
                    }
                    else if (_restrictions[restrictionPointer + 1 + i] == vertex1)
                    {
                        _restrictions[restrictionPointer + 1 + i] = vertex2;
                    }
                }

                this.AddByPointer(restrictionPointer);
            }
        }

        /// <summary>
        /// Adds a restriction using just it's pointer.
        /// </summary>
        private void AddByPointer(uint pointer)
        {
            var size = _restrictions[pointer];

            for(var i = 0; i < size; i++)
            {
                this.AddPointer(_restrictions[pointer + 1 + i], pointer);
            }
        }

        /// <summary>
        /// Adds a pointer for the given vertex.
        /// </summary>
        private void AddPointer(uint vertex, uint pointer)
        {
            var hash = CalculateHash(vertex);
            var hashPointer = _hashes[hash];
            if (hashPointer == NO_DATA)
            { // add at the end.
                _hashes[hash] = _nextIndexPointer;
                while(_index.Length <= _nextIndexPointer + 1)
                {
                    _index.Resize(_index.Length + BLOCKSIZE);
                }
                _index[_nextIndexPointer] = 1;
                _index[_nextIndexPointer + 1] = pointer;
                _nextIndexPointer += 2;
            }
            else
            { // add to the existing structure.
                var size = _index[hashPointer];

                // check if pointer is not already there.
                for (var i = 0; i < size; i++)
                {
                    if (_index[hashPointer + i + 1] == pointer)
                    { // pointer is already there.
                        return;
                    }
                }

                // add the pointer.
                if ((size & (size - 1)) == 0)
                { // a power of two, copy to the end.
                    var newSpace = size * 2;
                    while (_index.Length <= _nextIndexPointer + newSpace + 1)
                    {
                        _index.Resize(_index.Length + BLOCKSIZE);
                    }
                    _index[_nextIndexPointer] = size;
                    for(var i = 0; i < size; i++)
                    {
                        _index[_nextIndexPointer + 1 + i] = _index[hashPointer + 1 + i];
                    }
                    hashPointer = _nextIndexPointer;
                    _hashes[hash] = hashPointer;
                    _nextIndexPointer += newSpace + 1;
                }
                _index[hashPointer + 1 + size] = pointer;
                size++;
                _index[hashPointer] = size;
            }
        }
        
        /// <summary>
        /// Removes a restriction by pointer.
        /// </summary>
        private void RemoveByPointer(uint pointer)
        {
            var size = _restrictions[pointer];
            //_restrictions[pointer] = NO_DATA;
            
            for(var i = 0; i < size; i++)
            {
                var v = _restrictions[pointer + i + 1];
                //_restrictions[pointer + i + 1] = NO_DATA;

                var hash = CalculateHash(v);
                var hashPointer = _hashes[hash];
                if (hashPointer != NO_DATA)
                { // there is data.
                    var pSize = _index[hashPointer];
                    var j = 0;
                    for(; j < pSize; j++)
                    {
                        if (_index[hashPointer + j + 1] == pointer)
                        {
                            break;
                        }
                    }
                    for (; j < pSize; j++)
                    {
                        if (hashPointer + j + 2 < _index.Length)
                        {
                            _index[hashPointer + j + 1] = _index[hashPointer + j + 2];
                        }
                    }
                    _index[hashPointer] = pSize - 1;
                    if (pSize == 1)
                    {
                        _hashes[hash] = NO_DATA;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public RestrictionEnumerator GetEnumerator()
        {
            return new RestrictionEnumerator(this);
        }

        /// <summary>
        /// Returns true if this db has complex restrictions.
        /// </summary>
        public bool HasComplexRestrictions
        {
            get
            {
                return _hasComplexRestrictions;
            }
        }

        /// <summary>
        /// Calculates a hashcode with a fixed size.
        /// </summary>
        private int CalculateHash(uint vertex)
        {
            var hash = (vertex.GetHashCode() % (_hashes.Length / 2)) * 2;
            if (hash > 0)
            {
                return (int)hash;
            }
            return (int)-hash;
        }

        /// <summary>
        /// An enumerator to access restrictions.
        /// </summary>
        public class RestrictionEnumerator
        {
            private readonly RestrictionsDb _db;

            /// <summary>
            /// Creates a new restriction enumerator.
            /// </summary>
            internal RestrictionEnumerator(RestrictionsDb db)
            {
                _db = db;
            }

            private uint _vertex;
            private uint _indexPointer;
            private uint _indexPosition;
            private uint _restrictionPointer;
            
            /// <summary>
            /// Moves to restrictions for the given vertex.
            /// </summary>
            public bool MoveTo(uint vertex)
            {
                _vertex = vertex;
                var hash = _db.CalculateHash(vertex);
                _indexPointer = _db._hashes[hash];
                if (_indexPointer == NO_DATA)
                { // a quick negative answer is the important part here!
                    _restrictionPointer = NO_DATA;
                    _indexPosition = NO_DATA;
                    return false;
                }

                var pSize = _db._index[_indexPointer];
                for (_indexPosition = 0; _indexPosition < pSize; _indexPosition++)
                { // check if this restriction has the given vertex as it's first.
                    _restrictionPointer = _db._index[_indexPointer + 1 + _indexPosition];
                    var restrictionSize = _db._restrictions[_restrictionPointer];
                    for (var i = 0; i < restrictionSize; i++)
                    {
                        if (_db._restrictions[_restrictionPointer + i + 1] == _vertex)
                        {
                            _restrictionPointer = NO_DATA; // we need to move here again.
                            return true;
                        }
                    }
                }
                _restrictionPointer = NO_DATA;
                _indexPointer = NO_DATA;
                _indexPosition = NO_DATA;
                return false;
            }

            /// <summary>
            /// Move to the next restriction.
            /// </summary>
            public bool MoveNext()
            {
                if (_indexPointer == NO_DATA)
                {
                    throw new Exception("Cannot move to next restriction when there is not current vertex.");
                }

                if (_restrictionPointer == NO_DATA)
                {
                    _restrictionPointer = _db._index[_indexPointer + 1 + _indexPosition];
                    return true;
                }

                var pSize = _db._index[_indexPointer];
                _indexPosition++;
                for (; _indexPosition < pSize; _indexPosition++)
                { // check if this restriction has the given vertex as it's first.
                    _restrictionPointer = _db._index[_indexPointer + 1 + _indexPosition];
                    var restrictionSize = _db._restrictions[_restrictionPointer];
                    for (var i = 0; i < restrictionSize; i++)
                    {
                        if (_db._restrictions[_restrictionPointer + i + 1] == _vertex)
                        {
                            return true;
                        }
                    }
                }
                _restrictionPointer = NO_DATA;
                _indexPointer = NO_DATA;
                _indexPosition = NO_DATA;
                return false;
            }

            /// <summary>
            /// Gets the number of vertices in the current restriction.
            /// </summary>
            public uint Count
            {
                get
                {
                    if (_restrictionPointer == NO_DATA)
                    {
                        throw new InvalidOperationException("No current data available.");
                    }
                    
                    return _db._restrictions[_restrictionPointer];
                }
            }

            /// <summary>
            /// Returns the vertex at the given position.
            /// </summary>
            public uint this[int i]
            {
                get
                {
                    if (_restrictionPointer == NO_DATA)
                    {
                        throw new InvalidOperationException("No current data available.");
                    }
                    return _db._restrictions[_restrictionPointer + 1 + i];
                }
            }
        }

        private class LinkedNode
        {
            public uint Pointer { get; set; }
            public LinkedNode Next { get; set; }
        }
    }
}