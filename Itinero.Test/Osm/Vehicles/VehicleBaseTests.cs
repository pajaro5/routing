﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Attributes;
using Itinero.Osm.Vehicles;

namespace Itinero.Test.Osm.Vehicles
{
    /// <summary>
    /// Contains generic testing functionality for the vehicle class.
    /// </summary>
    public abstract class VehicleBaseTests
    {
        /// <summary>
        /// Tests the can traverse functionality.
        /// </summary>
        protected void TestVehicleCanTranverse(Vehicle vehicle, bool result, params string[] tags)
        {
            var tagsCollection = new AttributeCollection();
            for (int idx = 0; idx < tags.Length; idx = idx + 2)
            {
                tagsCollection.AddOrReplace(tags[idx], tags[idx + 1]);
            }

            if (result)
            { // assume the result is true.
                Assert.IsTrue(vehicle.CanTraverse(tagsCollection));
            }
            else
            { // assume the result is false.
                Assert.IsFalse(vehicle.CanTraverse(tagsCollection));
            }
        }

        /// <summary>
        /// Tests the maxspeeds.
        /// </summary>
        protected void TextMaxSpeed(Vehicle vehicle, double speed, params string[] tags)
        {
            // build tags collection.
            var tagsCollection = new AttributeCollection();
            for (int idx = 0; idx < tags.Length; idx = idx + 2)
            {
                tagsCollection.AddOrReplace(tags[idx], tags[idx + 1]);
            }

            Assert.AreEqual(speed, vehicle.MaxSpeedAllowed(tagsCollection), 0.001f);
        }

        /// <summary>
        /// Tests the probable speed.
        /// </summary>
        protected void TextProbableSpeed(Vehicle vehicle, double speed, params string[] tags)
        {
            // build tags collection.
            var tagsCollection = new AttributeCollection();
            for (int idx = 0; idx < tags.Length; idx = idx + 2)
            {
                tagsCollection.AddOrReplace(tags[idx], tags[idx + 1]);
            }

            Assert.AreEqual(speed, vehicle.ProbableSpeed(tagsCollection), 0.001f);
        }
    }
}