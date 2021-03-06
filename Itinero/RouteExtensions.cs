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

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Itinero
{
    /// <summary>
    /// Contains extensions for the route object.
    /// </summary>
    public static class RouteExtensions
    {        
        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        public static Route Concatenate(this Route route1, Route route2)
        {
            return route1.Concatenate(route2, true);
        }

        /// <summary>
        /// Concatenates two routes.
        /// </summary>
        public static Route Concatenate(this Route route1, Route route2, bool clone)
        {
            if (route1 == null) return route2;
            if (route2 == null) return route1;
            if (route1.Shape == null || route1.Shape.Length == 0) return route2;
            if (route2.Shape == null || route2.Shape.Length == 0) return route1;

            var timeoffset = route1.TotalTime;
            var distanceoffset = route1.TotalDistance;
            var shapeoffset = route1.Shape.Length - 1;

            // merge shape.
            var shapeLength = route1.Shape.Length + route2.Shape.Length - 1;
            var shape = new Coordinate[route1.Shape.Length + route2.Shape.Length - 1];
            route1.Shape.CopyTo(shape, 0);
            route2.Shape.CopyTo(shape, route1.Shape.Length - 1);

            // merge metas.
            var metas1 = route1.ShapeMeta != null ? route1.ShapeMeta.Length : 0;
            var metas2 = route2.ShapeMeta != null ? route2.ShapeMeta.Length : 0;
            var metas = new Route.Meta[metas1 + metas2 - 1];
            if (metas1 + metas2 - 1 > 0)
            {
                if (route1.ShapeMeta != null)
                {
                    for (var i = 0; i < route1.ShapeMeta.Length; i++)
                    {
                        metas[i] = new Route.Meta()
                        {
                            Attributes = new AttributeCollection(route1.ShapeMeta[i].Attributes),
                            Shape = route1.ShapeMeta[i].Shape
                        };
                    }
                }
                if (route2.ShapeMeta != null)
                {
                    for (var i = 1; i < route2.ShapeMeta.Length; i++)
                    {
                        metas[metas1 + i - 1] = new Route.Meta()
                        {
                            Attributes = new AttributeCollection(route2.ShapeMeta[i].Attributes),
                            Shape = route2.ShapeMeta[i].Shape + shapeoffset,
                            Distance = route2.ShapeMeta[i].Distance + distanceoffset,
                            Time = route2.ShapeMeta[i].Time + timeoffset
                        };
                    }
                }
            }

            // merge stops.
            var stops1 = route1.Stops != null ? route1.Stops.Length : 0;
            var stops2 = route2.Stops != null ? route2.Stops.Length : 0;
            var stops = new Route.Stop[stops1 + stops2];
            if (stops1 + stops2 > 0)
            {
                if (route1.Stops != null)
                {
                    for (var i = 0; i < route1.Stops.Length; i++)
                    {
                        var stop = route1.Stops[i];
                        stops[i] = new Route.Stop()
                        {
                            Attributes = new AttributeCollection(stop.Attributes),
                            Coordinate = stop.Coordinate,
                            Shape = stop.Shape
                        };
                    }
                }
                if (route2.Stops != null)
                {
                    for (var i = 0; i < route2.Stops.Length; i++)
                    {
                        var stop = route2.Stops[i];
                        stops[stops1 + i] = new Route.Stop()
                        {
                            Attributes = new AttributeCollection(stop.Attributes),
                            Coordinate = stop.Coordinate,
                            Shape = stop.Shape + shapeoffset
                        };
                    }
                }
            }

            // merge branches.
            var branches1 = route1.Branches != null ? route1.Branches.Length : 0;
            var branches2 = route2.Branches != null ? route2.Branches.Length : 0;
            var branches = new Route.Branch[branches1 + branches2];
            if (branches1 + branches2 > 0)
            {
                if (route1.Branches != null)
                {
                    for (var i = 0; i < route1.Branches.Length; i++)
                    {
                        var branch = route1.Branches[i];
                        branches[i] = new Route.Branch()
                        {
                            Attributes = new AttributeCollection(branch.Attributes),
                            Coordinate = branch.Coordinate,
                            Shape = branch.Shape
                        };
                    }
                }
                if (route2.Branches != null)
                {
                    for (var i = 0; i < route2.Branches.Length; i++)
                    {
                        var branch = route2.Branches[i];
                        branches[branches1 + i] = new Route.Branch()
                        {
                            Attributes = new AttributeCollection(branch.Attributes),
                            Coordinate = branch.Coordinate,
                            Shape = branch.Shape + shapeoffset
                        };
                    }
                }
            }

            // merge attributes.
            var attributes = new AttributeCollection(route1.Attributes);
            attributes.AddOrReplace(route2.Attributes);
            var profile = route1.Profile;
            if (route2.Profile != profile)
            {
                attributes.RemoveKey("profile");
            }
            
            // update route.
            var route =  new Route()
            {
                Attributes = attributes,
                Branches = branches,
                Shape = shape,
                ShapeMeta = metas,
                Stops = stops
            };
            route.TotalDistance = route1.TotalDistance + route2.TotalDistance;
            route.TotalTime = route1.TotalTime + route1.TotalTime;
            return route;
        }

        /// <summary>
        /// Calculates the position on the route after the given distance from the starting point.
        /// </summary>
        public static Coordinate? PositionAfter(this Route route, float distanceInMeter)
        {
            var distanceMeter = 0.0f;
            if (route.Shape == null)
            {
                return null;
            }

            for (int i = 0; i < route.Shape.Length - 1; i++)
            {
                var currentDistance = Coordinate.DistanceEstimateInMeter(route.Shape[i], route.Shape[i + 1]);
                if (distanceMeter + currentDistance >= distanceInMeter)
                {
                    var segmentDistance = distanceInMeter - distanceMeter;
                    var diffLat = route.Shape[i + 1].Latitude - route.Shape[i].Latitude;
                    var diffLon = route.Shape[i + 1].Longitude - route.Shape[i].Longitude;
                    var lat = route.Shape[i].Latitude + diffLat * (segmentDistance / currentDistance);
                    var lon = route.Shape[i].Longitude + diffLon * (segmentDistance / currentDistance);
                    return new Coordinate(lat, lon);
                }
                distanceMeter += currentDistance;
            }
            return null;
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected)
        {
            int segment;
            float distanceToProjectedInMeter;
            float timeToProjectedInSeconds;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceToProjectedInMeter, 
                out timeToProjectedInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected, 
            out float distanceToProjectedInMeter, out float timeToProjectedInSeconds)
        {
            int segment;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceToProjectedInMeter, 
                out timeToProjectedInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out float distanceFromStartInMeter)
        {
            int segment;
            Coordinate projected;
            float timeFromStartInSeconds;
            return route.ProjectOn(coordinate, out projected, out segment, out distanceFromStartInMeter, out timeFromStartInSeconds);
        }

        /// <summary>
        /// Calculates the closest point on the route relative to the given coordinate.
        /// </summary>
        public static bool ProjectOn(this Route route, Coordinate coordinate, out Coordinate projected, out int segment, 
            out float distanceFromStartInMeter, out float timeFromStartInSeconds)
        {
            float distance = float.MaxValue;
            distanceFromStartInMeter = 0;
            timeFromStartInSeconds = 0;
            projected = new Coordinate();
            segment = -1;

            if (route.Shape == null)
            {
                return false;
            }
            
            Coordinate currentProjected;
            float currentDistanceFromStart = 0;
            float currentDistance;
            var shape = route.Shape;
            for (int i = 0; i < shape.Length - 1; i++)
            {
                var line = new Line(shape[i], shape[i + 1]);
                var projectedPoint = line.ProjectOn(coordinate);
                if (projectedPoint != null)
                { // there was a projected point.
                    currentProjected = new Coordinate(projectedPoint.Value.Latitude, projectedPoint.Value.Longitude);
                    currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
                    if (currentDistance < distance)
                    { // this point is closer.
                        projected = currentProjected;
                        segment = i;
                        distance = currentDistance;

                        // calculate distance/time.
                        var localDistance = Coordinate.DistanceEstimateInMeter(currentProjected, shape[i]);
                        distanceFromStartInMeter = currentDistanceFromStart + localDistance;
                    }
                }

                // check first point.
                currentProjected = shape[i];
                currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
                if (currentDistance < distance)
                { // this point is closer.
                    projected = currentProjected;
                    segment = i;
                    distance = currentDistance;
                    distanceFromStartInMeter = currentDistanceFromStart;
                }

                // update distance from start.
                currentDistanceFromStart = currentDistanceFromStart + Coordinate.DistanceEstimateInMeter(shape[i], shape[i + 1]);
            }

            // check last point.
            currentProjected = shape[shape.Length - 1];
            currentDistance = Coordinate.DistanceEstimateInMeter(coordinate, currentProjected);
            if (currentDistance < distance)
            { // this point is closer.
                projected = currentProjected;
                segment = shape.Length - 1;
                distance = currentDistance;
                distanceFromStartInMeter = currentDistanceFromStart;
            }
            return true;
        }
        
        /// <summary>
        /// Returns the turn direction for the shape point at the given index.
        /// </summary>
        public static RelativeDirection RelativeDirectionAt(this Route route, int i)
        {
            if (i < 0 || i >= route.Shape.Length) { throw new ArgumentOutOfRangeException("i"); }

            if (i == 0 || i == route.Shape.Length - 1)
            { // not possible to calculate a relative direction for the first or last segment.
                throw new ArgumentOutOfRangeException("i", "It's not possible to calculate a relative direction for the first or last segment.");
            }
            return DirectionCalculator.Calculate(
                new Coordinate(route.Shape[i - 1].Latitude, route.Shape[i - 1].Longitude),
                new Coordinate(route.Shape[i].Latitude, route.Shape[i].Longitude),
                new Coordinate(route.Shape[i + 1].Latitude, route.Shape[i + 1].Longitude));
        }

        /// <summary>
        /// Returns the direction to the next shape segment.
        /// </summary>
        public static DirectionEnum DirectionToNext(this Route route, int i)
        {
            if (i < 0 || i >= route.Shape.Length - 1) { throw new ArgumentOutOfRangeException("i"); }

            return DirectionCalculator.Calculate(
                new Coordinate(route.Shape[i].Latitude, route.Shape[i].Longitude),
                new Coordinate(route.Shape[i + 1].Latitude, route.Shape[i + 1].Longitude));
        }

        /// <summary>
        /// Returns this route as json.
        /// </summary>
        public static string ToJson(this Route route)
        {
            var stringWriter = new StringWriter();
            route.WriteJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as json.
        /// </summary>
        public static void WriteJson(this Route route, Stream stream)
        {
            route.WriteJson(new StreamWriter(stream));
        }

        /// <summary>
        /// Writes the route as json.
        /// </summary>
        public static void WriteJson(this Route route, TextWriter writer)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            if (route.Shape != null)
            {
                jsonWriter.WritePropertyName("Shape");
                jsonWriter.WriteArrayOpen();
                for(var i = 0; i < route.Shape.Length; i++)
                {
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Shape[i].Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Shape[i].Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.ShapeMeta != null)
            {
                jsonWriter.WritePropertyName("ShapeMeta");
                jsonWriter.WriteArrayOpen();
                for(var i = 0; i < route.ShapeMeta.Length; i++)
                {
                    var meta = route.ShapeMeta[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(meta.Shape.ToInvariantString());

                    if (meta.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach(var attribute in meta.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.Stops != null)
            {
                jsonWriter.WritePropertyName("Stops");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < route.Stops.Length; i++)
                {
                    var stop = route.Stops[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(stop.Shape.ToInvariantString());
                    jsonWriter.WritePropertyName("Coordinates");
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Stops[i].Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Stops[i].Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    if (stop.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach (var attribute in stop.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (route.Branches != null)
            {
                jsonWriter.WritePropertyName("Branches");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < route.Branches.Length; i++)
                {
                    var stop = route.Branches[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WritePropertyName("Shape");
                    jsonWriter.WritePropertyValue(stop.Shape.ToInvariantString());
                    jsonWriter.WritePropertyName("Coordinates");
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Branches[i].Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Branches[i].Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    if (stop.Attributes != null)
                    {
                        jsonWriter.WritePropertyName("Attributes");
                        jsonWriter.WriteOpen();
                        foreach (var attribute in stop.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                        jsonWriter.WriteClose();
                    }
                    jsonWriter.WriteClose();
                }
                jsonWriter.WriteArrayClose();
            }

            jsonWriter.WriteClose();
        }

        /// <summary>
        /// Returns this route as xml.
        /// </summary>
        public static string ToXml(this Route route)
        {
            var stringWriter = new StringWriter();
            route.WriteXml(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as xml.
        /// </summary>
        public static void WriteXml(this Route route, Stream stream)
        {
            route.WriteXml(new StreamWriter(stream));
        }

        /// <summary>
        /// Writes the route as xml.
        /// </summary>
        public static void WriteXml(this Route route, TextWriter writer)
        {
            var ser = new XmlSerializer(typeof(Route));
            ser.Serialize(writer, route);
            writer.Flush();
        }

        /// <summary>
        /// Reads a route in xml.
        /// </summary>
        public static Route ReadXml(Stream stream)
        {
            var ser = new XmlSerializer(typeof(Route));
            return ser.Deserialize(stream) as Route;
        }

        /// <summary>
        /// Returns this route as geojson.
        /// </summary>
        public static string ToGeoJson(this Route route)
        {
            var stringWriter = new StringWriter();
            route.WriteGeoJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the route as geojson.
        /// </summary>
        public static void WriteGeoJson(this Route route, Stream stream)
        {
            route.WriteGeoJson(new StreamWriter(stream));
        }

        /// <summary>
        /// Writes the route as geojson.
        /// </summary>
        public static void WriteGeoJson(this Route route, TextWriter writer)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            if (route.Shape != null)
            {
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "Feature", true, false);
                jsonWriter.WriteProperty("name", "Shape", true, false);
                jsonWriter.WritePropertyName("properties");
                jsonWriter.WriteOpen();
                jsonWriter.WriteClose();
                jsonWriter.WritePropertyName("geometry", false);


                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "LineString", true, false);
                jsonWriter.WritePropertyName("coordinates", false);
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < route.Shape.Length; i++)
                {
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(route.Shape[i].Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(route.Shape[i].Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                }
                jsonWriter.WriteArrayClose();
                jsonWriter.WriteClose();

                jsonWriter.WriteClose();
            }

            if (route.ShapeMeta != null)
            {
                for (var i = 0; i < route.ShapeMeta.Length; i++)
                {
                    var meta = route.ShapeMeta[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WriteProperty("name", "ShapeMeta", true, false);
                    jsonWriter.WriteProperty("Shape", meta.Shape.ToInvariantString(), true, false);
                    jsonWriter.WritePropertyName("geometry", false);

                    var coordinate = route.Shape[meta.Shape];

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Point", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    if (meta.Attributes != null)
                    {
                        foreach (var attribute in meta.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                    }
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }
            }

            if (route.Stops != null)
            {
                for (var i = 0; i < route.Stops.Length; i++)
                {
                    var stop = route.Stops[i];

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WriteProperty("name", "Stop", true, false);
                    jsonWriter.WriteProperty("Shape", stop.Shape.ToInvariantString(), true, false);
                    jsonWriter.WritePropertyName("geometry", false);
                    
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Point", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(stop.Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(stop.Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    if (stop.Attributes != null)
                    {
                        foreach (var attribute in stop.Attributes)
                        {
                            jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                        }
                    }
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
        }
    }
}