﻿// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using System.Collections.Generic;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Contains an enumeration of vehicle types.
    /// </summary>
    public static class VehicleType
    {
        /// <summary>
        /// Represents any vehicle, excluding movements without a vehicle, like 'pedestrian', 'ski', 'horse',...
        /// </summary>
        public static string Vehicle = "vehicle";
        /// <summary>
        /// Represents a pedestrian.
        /// </summary>
        public static string Pedestrian = "pedestrian";
        /// <summary>
        /// Represents a bicycle.
        /// </summary>
        public static string Bicycle = "bicycle";
        /// <summary>
        /// Represents any motorized vehicle.
        /// </summary>
        public static string MotorVehicle = "motor_vehicle";
        /// <summary>
        /// Represents a moped.
        /// </summary>
        public static string Moped = "moped";
        /// <summary>
        /// Represents a motorcycle.
        /// </summary>
        public static string MotorCycle = "motorcycle";
        /// <summary>
        /// Represents a motorcar.
        /// </summary>
        public static string MotorCar = "motorcar";
        /// <summary>
        /// Represents a light goods truck.
        /// </summary>
        public static string Goods = "goods";
        /// <summary>
        /// Represents a heavy goods truck.
        /// </summary>
        public static string Hgv = "hgv";
        /// <summary>
        /// Represents a bus that is not used in public transport.
        /// </summary>
        public static string TouristBus = "tourist_bus";
        /// <summary>
        /// Represents any vehicle other than the above.
        /// </summary>
        public static string Other = "other";
    }
}