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

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents a BigTruck
    /// </summary>
    public class BigTruck : MotorVehicle
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BigTruck()
        {
            // http://wiki.openstreetmap.org/wiki/Key:hgv#Land-based_transportation
            VehicleTypes.Add("hgv");
        }

        /// <summary>
        /// Returns a unique name this vehicle type.
        /// </summary>
        public override string UniqueName
        {
            get { return "BigTruck"; }
        }
    }
}