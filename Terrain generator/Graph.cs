using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Terrain_generator
{
    public class Graph<NodeType, LinkType>
        where NodeType : Graph<NodeType, LinkType>.Node
        where LinkType : Graph<NodeType, LinkType>.Link
    {
        // http://msdn.microsoft.com/en-us/library/ms379574%28v=vs.80%29.aspx#datastructures20_5_topic3
        List<NodeType> nodes = new List<NodeType>();
        public List<NodeType> Nodes { get { return nodes; } }

        public class Node
        {
            List<LinkType> links = new List<LinkType>();
            public List<LinkType> LinkedNodes { get { return links; } }
        }

        public class Link
        {
            public NodeType Node { get; set; }
            
            public Link(NodeType destination)
            {
                Node = destination;
            }
        }
    }
}
