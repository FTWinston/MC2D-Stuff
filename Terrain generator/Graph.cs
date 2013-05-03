using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Terrain_generator
{
    public class Graph
    {
        // http://msdn.microsoft.com/en-us/library/ms379574%28v=vs.80%29.aspx#datastructures20_5_topic3
        List<Node> nodes = new List<Node>();
        public List<Node> Nodes { get { return nodes; } }

        public class Node
        {
            public Node(int x, int y) { X = x; Y = y; }
            List<Link> links = new List<Link>();
            public List<Link> LinkedNodes { get { return links; } }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Link
        {
            public Node Node { get; set; }
            public bool Rightwards { get; set; }
            public bool Leftwards { get { return !Rightwards; } set { Rightwards = !value; } }

            public Link(Node destination, bool rightwards)
            {
                Node = destination;
                Rightwards = rightwards;
            }
        }
    }
}
