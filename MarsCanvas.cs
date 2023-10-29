using MagmaDataMiner;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MarsExplorer
{
    public partial class MarsCanvas : Control
    {
        private Font TitleFont;
        public MarsCanvas()
        {
            DoubleBuffered = true;
            InitializeComponent();

            TitleFont = new(Font.FontFamily, Font.Size - 1);
        }

        private Point Center;
        private int CanvasScale = 16;

        class MouseButtonState
        {
            public bool Down;
            public Point Prev;
            public Point Start;
            public void Reset(Point prev)
            {
                Down = true;
                Prev = prev;
                Start = prev;
            }

            public void Set(Point prev)
            {
                Down = true;
                Prev = prev;
            }
            internal Point GlobalDelta(Point location)
            {
                return new(location.X - Start.X, location.Y - Start.Y);
            }

            internal Point Delta(Point location)
            {
                return new(location.X - Prev.X, location.Y - Prev.Y);
            }
            internal Point DeltaLatch(Point location)
            {
                var delta = Delta(location);
                Prev = location;
                return delta;
            }

            public float GlobalDistance(Point location)
            {
                float dx = location.X - Start.X;
                float dy = location.Y - Start.Y;
                return MathF.Sqrt(dx * dx + dy * dy);
            }
        }

        private MouseButtonState MouseLeftState = new();
        private MouseButtonState MouseRightState = new();
        private MouseButtonState MouseMiddleState = new();

        private MouseButtonState MouseMiscState = new();

        private MouseButtonState ForButton(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => MouseLeftState,
                MouseButtons.Right => MouseRightState,
                MouseButtons.Middle => MouseMiddleState,
                _ => MouseMiscState,
            };
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            ForButton(e.Button).Reset(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            ForButton(e.Button).Down = false;

            if (e.Button == MouseButtons.Left && MouseLeftState.GlobalDistance(e.Location) < 3)
            {
                if (Model == null)
                {
                    return;
                }

                if (PortGrabSource.IsValid)
                {
                    if (PortGrabPortDest.IsValid)
                    {
                        MarsNode.ConnectPort(PortGrabSource, PortGrabPortDest);
                        PortGrabSource.Reset();
                        PortGrabPortDest.Reset();
                        Invalidate();
                    }
                }
                else if (PortHighlight.IsValid)
                {
                    PortGrabSource = PortHighlight;
                    PortHighlight.Reset();
                    Invalidate();
                }
                else
                {
                    Point click = ViewToCanvas(e.Location);
                    foreach (var node in Model.Nodes)
                    {
                        if (node.View.Contains(click))
                        {
                            OnNodeSelected?.Invoke(node);
                            break;
                        }
                    }
                }

            }
        }

        private Point ViewToCanvas(Point p)
        {
            Point click = p;
            click.Offset(-Center.X, -Center.Y);
            click.X = (int)(click.X / (CanvasScale / 16.0f));
            click.Y = (int)(click.Y / (CanvasScale / 16.0f));
            return click;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                CanvasScale -= 2;
                if (CanvasScale < 2)
                    CanvasScale = 2;
            }
            else if (e.Delta > 0)
            {
                CanvasScale += 2;
                if (CanvasScale > 64)
                    CanvasScale = 64;
            }

            Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (PortGrabSource.Reset())
                {
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (MouseRightState.Down)
            {
                Center.Offset(MouseRightState.DeltaLatch(e.Location));

                Invalidate();
            }

            if (Model == null)
            {
                return;
            }

            if (MouseLeftState.Down && Model.Selected != null)
            {
                Model.Selected.View.Offset(MouseLeftState.DeltaLatch(e.Location));

                Invalidate();
            }

            if (!MouseLeftState.Down)
            {
                Point click = ViewToCanvas(e.Location);
                bool shouldInvalidate = false;

                if (PortGrabSource.IsValid)
                {
                    PortGrabPointDest = click;
                }

                string? portName = null;
                MarsNode? foundPort = null;

                foreach (var node in Model.Nodes)
                {
                    foreach (var port in node.Ports)
                    {
                        var c = node.PortCenter(port.Name);
                        c.Offset(node.View.Location);
                        float distSq = BubbleMath.DistanceSquared(c, click);

                        if (distSq < MarsNode.PortSizeSingleSquared)
                        {
                            foundPort = node;
                            portName = port.Name;
                            break;
                        }

                    }

                    if (portName != null)
                        break;
                }

                if (!PortHighlight.Matches(foundPort, portName))
                {
                    PortHighlight.Port = portName;
                    PortHighlight.Node = foundPort;
                    shouldInvalidate = true;
                }

                if (PortGrabSource.IsValid)
                {
                    if (PortHighlight.IsValid && IsValidConnection(PortGrabSource, PortHighlight))
                    {
                        PortGrabPortDest = PortHighlight;
                    }
                    else
                    {
                        PortGrabPortDest.Reset();
                    }
                    shouldInvalidate = true;
                }

                if (shouldInvalidate)
                {
                    Invalidate();
                }
            }


        }

        private bool IsValidConnection(PortSelection a, PortSelection b)
        {
            if (!a.IsValid || !b.IsValid)
            {
                return false;
            }

            if (a.Node == b.Node)
            {
                return false;
            }

            var portA = a.GetPort();
            var portB = b.GetPort();

            if (portA.Direction == portB.Direction)
            {
                return false;
            }

            if (portA.IsStrict || portB.IsStrict)
            {
                if (portA.TypeName != portB.TypeName)
                {
                    return false;
                }
            }

            //TODO: type checking

            return true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);


            if (Model == null)
            {
                return;
            }

            var g = pe.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.TranslateTransform(Center.X, Center.Y);
            g.ScaleTransform(CanvasScale / 16.0f, CanvasScale / 16.0f);

            foreach (var node in Model.Nodes)
            {
                var ot = g.Transform.Clone();
                DrawNode(g, node);
                g.Transform = ot;

            }

            foreach (var node in Model.Nodes)
            {
                foreach (var output in node.Outputs)
                {
                    DrawOutput(g, node, output);
                }
            }

            if (PortGrabSource.IsValid)
            {
                var src = PortCenter(PortGrabSource.Node!, PortGrabSource.Port!);
                Point dst;
                Pen pen;
                if (PortGrabPortDest.IsValid)
                {
                    dst = PortCenter(PortGrabPortDest.Node!, PortGrabPortDest.Port!);
                    pen = NewConnectionPen;
                }
                else
                {
                    dst = PortGrabPointDest;
                    pen = NewWirePen;
                }

                if (PortGrabSource.Node!.IsInput(PortGrabSource.Port!))
                {
                    (src, dst) = (dst, src);
                }

                DrawWire(g, pen, src, dst);
            }
        }

        private GraphicsPath path = new();
        private static readonly Size arcSize = new(12, 12);
        private Rectangle arc = new(Point.Empty, arcSize);

        private PortSelection PortHighlight;
        private PortSelection PortGrabSource;
        private Point PortGrabPointDest;
        private PortSelection PortGrabPortDest;

        private void PrepareNodePath(bool resetArc)
        {
            if (resetArc)
                arc.Location = Point.Empty;

            path.Reset();
            path.StartFigure();
        }

        private void PathNodeTop(MarsNode node)
        {
            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = node.View.Width - arcSize.Width;
            path.AddArc(arc, 270, 90);
        }
        private void PathNodeBottom(MarsNode node)
        {
            // bottom right arc  
            arc.Y = node.View.Height - arcSize.Height;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = 0;
            path.AddArc(arc, 90, 90);
        }

        private void DrawNode(Graphics g, MarsNode node)
        {
            g.TranslateTransform(node.View.Left, node.View.Top);

            PrepareNodePath(true);

            PathNodeTop(node);
            path.AddRectangle(new(0, arcSize.Height / 2, node.View.Width, MarsNode.TitleHeight - arcSize.Height / 2));
            path.CloseFigure();
            g.FillPath(Brushes.DarkSlateGray, path);

            PrepareNodePath(false);

            PathNodeBottom(node);
            path.AddRectangle(new(0, MarsNode.TitleHeight, node.View.Width, node.View.Height - MarsNode.TitleHeight - arcSize.Height / 2));
            path.CloseFigure();
            g.FillPath(new SolidBrush(node.Color), path);

            if (node.Highlight)
            {
                PrepareNodePath(true);
                PathNodeTop(node);
                PathNodeBottom(node);
                path.CloseFigure();
                g.DrawPath(new(Color.GreenYellow, 5), path);
            }

            g.DrawString(node.Name.Replace("Node", ""), TitleFont, Brushes.White, new Rectangle(4, 0, node.View.Width, 32));

            foreach (var input in node.Inputs)
            {
                var topLeft = node.PortTopLeft(input.Name);
                bool isValidTarget = DrawPortCircle(g, node, input, topLeft);

                if (PortGrabSource.Matches(node, input) || isValidTarget || (!PortGrabSource.IsValid && input.Links.Count > 0))
                {
                    var label = g.MeasureString(input.Name, Font);
                    topLeft.Offset(MarsNode.PortSize.Width + 4, -(int)(label.Height / 2) + MarsNode.PortSize.Height / 2);
                    g.DrawString(input.Name, Font, node.IsDark ? Brushes.White : Brushes.Black, topLeft);
                }
            }

            foreach (var output in node.Outputs)
            {
                var topLeft = node.PortTopLeft(output.Name);
                bool isValidTarget = DrawPortCircle(g, node, output, topLeft);

                if (PortGrabSource.Matches(node, output) || isValidTarget || (!PortGrabSource.IsValid && output.Links.Count > 0))
                {
                    var label = g.MeasureString(output.Name, Font);
                    topLeft.Offset(-(int)label.Width - 4, -(int)(label.Height / 2) + MarsNode.PortSize.Height / 2);
                    g.DrawString(output.Name, Font, node.IsDark ? Brushes.White : Brushes.Black, topLeft);
                }
            }
        }

        private bool DrawPortCircle(Graphics g, MarsNode node, MinedPort input, Point topLeft)
        {
            Rectangle ellipse = new(topLeft, MarsNode.PortSize);
            g.FillEllipse(Brushes.Black, ellipse);

            if (PortHighlight.Matches(node, input))
            {
                g.FillEllipse(Brushes.GreenYellow, ellipse);
            }

            if (PortGrabSource.IsValid)
            {
                if (IsValidConnection(PortGrabSource, new(node, input.Name)))
                {
                    return true;
                }

                g.FillEllipse(Brushes.DarkRed, ellipse);
            }

            return false;
        }

        private static readonly Pen HiPen = new Pen(Color.Orange, 4);
        private static readonly Pen NewWirePen = new Pen(Color.RebeccaPurple, 4);
        private static readonly Pen NewConnectionPen = new Pen(Color.Sienna, 4);


        private void DrawWire(Graphics g, Pen pen, Point origin, Point dst)
        {
            int fromDst = dst.X - origin.X;
            int switchDist = 50;
            if (fromDst < (switchDist - 10))
            {
                Point dist = new(dst.X - origin.X, dst.Y - origin.Y);
                Point[] bPoints =
                {
                        origin,
                        new(origin.X + switchDist, origin.Y), new(origin.X + switchDist, origin.Y + switchDist * Math.Sign(dist.Y)),
                        new(origin.X + dist.X / 2, origin.Y + dist.Y / 2),
                        new(dst.X - switchDist, dst.Y - switchDist * Math.Sign(dist.Y)), new(dst.X - switchDist, dst.Y),
                        dst
                    };
                g.DrawBeziers(pen, bPoints);
            }
            else
            {
                g.DrawBezier(pen, origin, new(dst.X, origin.Y), new(origin.X, dst.Y), dst);
            }
        }

        private Point PortCenter(MarsNode node, string port)
        {
            var origin = node.View.Location;
            origin.Offset(node.PortCenter(port));
            return origin;
        }

        private void DrawOutput(Graphics g, MarsNode node, MinedPort output)
        {
            var origin = PortCenter(node, output.Name);

            Pen linePen = Pens.Blue;

            foreach (var link in output.Links)
            {
                var to = link.Node;
                var dstNode = (to.UserValue as MarsNode)!;

                Pen pen = (dstNode.Highlight || node.Highlight) ? HiPen : linePen;

                var dst = PortCenter(dstNode, link.Target);

                DrawWire(g, pen, origin, dst);
            }

        }


        public MarsModel? Model { get; set; }

        public event OnNodeSelected? OnNodeSelected;
    }

    public delegate void OnNodeSelected(MarsNode node);

    public struct PortSelection : IEquatable<PortSelection>
    {
        public MarsNode? Node;
        public string? Port;

        public PortSelection(MarsNode node, string port)
        {
            Node = node;
            Port = port;
        }

        public bool IsValid => Port != null;

        public bool IsInput => Node!.IsInput(Port!);

        internal bool Matches(MarsNode node, MinedPort port) => Matches(node, port.Name);

        internal bool Matches(MarsNode? node, string? portName)
        {
            return Node == node && Port == portName;
        }

        public bool Reset()
        {
            bool ret = IsValid;
            Port = null;
            Node = null;
            return ret;
        }

        public override bool Equals(object? obj)
        {
            return obj is PortSelection selection && Equals(selection);
        }

        public bool Equals(PortSelection other)
        {
            return Port == other.Port &&
                   EqualityComparer<MarsNode?>.Default.Equals(Node, other.Node) &&
                   IsValid == other.IsValid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Port, Node, IsValid);
        }

        internal MinedPort GetPort()
        {
            if (!IsValid)
            {
                throw new NotSupportedException();
            }
            return Node!.GetPort(Port!);
        }

        public static bool operator ==(PortSelection left, PortSelection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PortSelection left, PortSelection right)
        {
            return !(left == right);
        }
    }

}
