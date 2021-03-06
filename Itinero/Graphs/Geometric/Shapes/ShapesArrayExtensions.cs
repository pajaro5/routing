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

using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Contains extension methods for the shape index.
    /// </summary>
    public static class ShapesArrayExtensions
    {
        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public static void Set(this ShapesArray index, long id, IEnumerable<Coordinate> shape)
        {
            index[id] = new ShapeEnumerable(shape);
        }

        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public static void Set(this ShapesArray index, long id, params Coordinate[] shape)
        {
            index[id] = new ShapeEnumerable(shape);
        }
    }
}