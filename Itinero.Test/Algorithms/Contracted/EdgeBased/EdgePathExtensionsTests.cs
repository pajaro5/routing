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

using Itinero.Algorithms;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the edge path extensions.
    /// </summary>
    [TestFixture]
    public class EdgePathExtensionsTests
    {
        /// <summary>
        /// Tests getting the first sequence after the source vertex.
        /// </summary>
        [Test]
        public void TestGetSequence1()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            var e1 = graph.AddEdge(0, 1, 100, null);
            var e2 = graph.AddEdge(1, 2, 100, null);
            var e3 = graph.AddEdge(2, 6, 100, null, 4, new uint[] { 3 }, new uint[] { 5 });
            var e4 = graph.AddEdge(6, 16, 100, null, 11, new uint[] { 7, 8, 9, 10 }, new uint[] { 12, 13, 14, 15 });
            var enumerator = graph.GetEdgeEnumerator();

            // build and test getting sequences from paths.
            var path = new EdgePath<float>(0);
            var s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(0, s.Length);

            path = new EdgePath<float>(1, 100, new EdgePath<float>(0));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(1, s[0]);

            path = new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0)));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(1, s[0]);
            Assert.AreEqual(2, s[1]);

            path = new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0)));
            s = path.GetSequence1(enumerator, 1);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(1, s[0]);

            path = new EdgePath<float>(6, 300, e3 + 1, new EdgePath<float>(2));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(3, s[0]);

            path = new EdgePath<float>(6, 200, e3 + 1, new EdgePath<float>(2, 100, e2 + 1, new EdgePath<float>(1)));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(2, s[0]);
            Assert.AreEqual(3, s[1]);

            path = new EdgePath<float>(6, 200, e3 + 1, new EdgePath<float>(2, 100, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0))));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(3, s.Length);
            Assert.AreEqual(1, s[0]);
            Assert.AreEqual(2, s[1]);
            Assert.AreEqual(3, s[2]);
            s = path.GetSequence1(enumerator, 1);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(1, s[0]);
            s = path.GetSequence1(enumerator, 2);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(1, s[0]);
            Assert.AreEqual(2, s[1]);
            
            path = new EdgePath<float>(16, 400, e4 + 1, new EdgePath<float>(6));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(4, s.Length);
            Assert.AreEqual(7, s[0]);
            Assert.AreEqual(8, s[1]);
            Assert.AreEqual(9, s[2]);
            Assert.AreEqual(10, s[3]);
            s = path.GetSequence1(enumerator, 1);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(10, s[0]);
            s = path.GetSequence1(enumerator, 3);
            Assert.IsNotNull(s);
            Assert.AreEqual(3, s.Length);
            Assert.AreEqual(8, s[0]);
            Assert.AreEqual(9, s[1]);
            Assert.AreEqual(10, s[2]);

            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            e1 = graph.AddEdge(0, 1, 100, null);
            e2 = graph.AddEdge(1, 2, 100, null);
            e3 = graph.AddEdge(2, 10, 100, null, 6, new uint[] { 3, 4, 5 }, new uint[] { 7, 8, 9 });
            enumerator = graph.GetEdgeEnumerator();

            path = new EdgePath<float>(10, 300, e3 + 1, new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 200, e1 + 1, new EdgePath<float>(0))));
            s = path.GetSequence1(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(5, s.Length);
            Assert.AreEqual(1, s[0]);
            Assert.AreEqual(2, s[1]);
            Assert.AreEqual(3, s[2]);
            Assert.AreEqual(4, s[3]);
            Assert.AreEqual(5, s[4]);

            path = new EdgePath<float>(10, 300, e3 + 1, new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 200, e1 + 1, new EdgePath<float>(0))));
            s = path.GetSequence1(enumerator, 3);
            Assert.IsNotNull(s);
            Assert.AreEqual(3, s.Length);
            Assert.AreEqual(1, s[0]);
            Assert.AreEqual(2, s[1]);
            Assert.AreEqual(3, s[2]);
        }

        /// <summary>
        /// Tests getting the last sequence before the target vertex.
        /// </summary>
        [Test]
        public void TestGetSequence2()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            var e1 = graph.AddEdge(0, 1, 100, null);
            var e2 = graph.AddEdge(1, 2, 100, null);
            var e3 = graph.AddEdge(2, 6, 100, null, 4, new uint[] { 3 }, new uint[] { 5 });
            var e4 = graph.AddEdge(6, 16, 100, null, 11, new uint[] { 7, 8, 9, 10 }, new uint[] { 12, 13, 14, 15 });
            var enumerator = graph.GetEdgeEnumerator();

            // build and test getting sequences from paths.
            var path = new EdgePath<float>(0);
            var s = path.GetSequence2(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(0, s.Length);

            path = new EdgePath<float>(1, 100, new EdgePath<float>(0));
            s = path.GetSequence2(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(0, s[0]);

            path = new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0)));
            s = path.GetSequence2(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(0, s[0]);
            Assert.AreEqual(1, s[1]);

            path = new EdgePath<float>(6, 300, e3 + 1, new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0))));
            s = path.GetSequence2(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(5, s[0]);

            path = new EdgePath<float>(16, 400, e4 + 1, new EdgePath<float>(6, 300, e3 + 1, new EdgePath<float>(2, 200, e2 + 1, new EdgePath<float>(1, 100, new EdgePath<float>(0)))));
            s = path.GetSequence2(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(4, s.Length);
            Assert.AreEqual(12, s[0]);
            Assert.AreEqual(13, s[1]);
            Assert.AreEqual(14, s[2]);
            Assert.AreEqual(15, s[3]);
        }
    }
}