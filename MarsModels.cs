using MagmaDataMiner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsExplorer
{
    public class MarsNode
    {
        public Rectangle View;
        public int Order = -1;
        public string? HighlightPort;
        private readonly MinedNode Model;
        private readonly MinedPort[] inputs;
        private readonly MinedPort[] outputs;

        public const int PortSizeSingle = 14;
        public const int PortSizeSingleSquared = PortSizeSingle * PortSizeSingle;
        public static readonly Size PortSize = new(PortSizeSingle, PortSizeSingle);
        public const int PortGap = 6;
        public const int TitleHeight = 26;

        private readonly Dictionary<string, Point> PortPos = new();

        public MarsNode(MinedNode node)
        {
            Model = node;
            Model.UserValue = this;

            inputs = Model.Ports.Values.Where(x => x.Direction == "Input").ToArray();
            outputs = Model.Ports.Values.Where(x => x.Direction == "Output").ToArray();

            View = new(Model.Position, new(220, Math.Max(inputs.Length, outputs.Length) * (PortSizeSingle + PortGap) + PortGap + PortGap + TitleHeight));

            InvalidatePorts();
        }

        public bool IsInput(string port) => Model.Ports[port].Direction == "Input";
        public bool IsOutput(string port) => Model.Ports[port].Direction == "Output";

        private void InvalidatePorts()
        {
            int[] currY = { PortGap + TitleHeight, PortGap + TitleHeight };
            int[] currX = { -PortSizeSingle / 2, View.Width - PortSizeSingle / 2 };

            PortPos.Clear();

            foreach (var (name, port) in Model.Ports)
            {
                int yi = port.Direction == "Input" ? 0 : 1;
                int x = currX[yi];
                int y = currY[yi];

                currY[yi] += PortSizeSingle + PortGap;

                PortPos.Add(name, new(x, y));
            }
        }
        public MinedPort GetPort(string name) => Model.Ports[name];
        public IEnumerable<MinedPort> Inputs => inputs;
        public IEnumerable<MinedPort> Outputs => outputs;

        public Point PortTopLeft(string name) => PortPos[name];
        public Point PortCenter(string name)
        {
            var point = PortTopLeft(name);
            point.Offset(PortSizeSingle / 2, PortSizeSingle / 2);
            return point;
        }

        public static void ConnectPort(PortSelection source, PortSelection target)
        {
            PrepareToConnect(source.Node!.Model, source.GetPort());
            PrepareToConnect(target.Node!.Model, target.GetPort());

            Connect(source.GetPort(), target);
            Connect(target.GetPort(), source);
        }

        private static void PrepareToConnect(MinedNode node, MinedPort port)
        {
            if (port.ConnectionType == "Override")
            {
                var link = port.Links[0];
                link.Node.Ports[link.Target].Links.Remove(new(node, port.Name));
                port.Links.Clear();
            }

        }

        private static void Connect(MinedPort port, PortSelection target)
        {

            port.Links.Add(new(target.Node!.Model, target.Port!));
        }

        public string Name => Model.ShinyType;

        public bool Highlight { get; internal set; }
        public IReadOnlyDictionary<string, object> Props => Model.Parameters;

        public Color Color => Model.Color;

        public bool IsDark => Model.IsDark;

        public IEnumerable<MinedPort> Ports => Model.Ports.Values;
    }

    public class MarsModel
    {
        public List<MarsNode> Nodes = new();

        public MarsNode? Selected = null;


        internal void Reset(MinedGraph graph)
        {
            Nodes.Clear();
            Nodes.AddRange(graph.Nodes.Select(n => new MarsNode(n)));
            Selected = null;
        }
    }
}
