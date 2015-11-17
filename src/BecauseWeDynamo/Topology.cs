﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Topology
{
    public class HalfEdge
    {
        //**FIELD
        internal Vertex[] V;

        //**PROPERTIES** //**QUERY**
        public double Angle { get; set; }
        public double Length { get { return Math.Sqrt(Math.Pow(V[0].X - V[1].X, 2) + Math.Pow(V[0].Y - V[1].Y, 2) + Math.Pow(V[0].Z - V[1].Z, 2)); } }
        public Vector Normal { get { return Face.Normal; } }
        public Edge Edge { get; private set; }
        public Face Face { get; private set; }

        //**CONSTRUCTOR**
        internal HalfEdge(Vertex A, Vertex B)
        {
            Angle = 360;
            V = new Vertex[] { A, B };
        }
        internal HalfEdge(Vertex A, Vertex B, Edge Edge, Face Face)
            : this(A, B)
        { this.Edge = Edge; this.Face = Face; }
        internal HalfEdge(IEnumerable<Vertex> Vertices) : this(Vertices.ElementAt(0), Vertices.ElementAt(1)) { }
        internal HalfEdge(IEnumerable<Vertex> Vertices, Edge Edge, Face Face)
            : this(Vertices)
        { this.Edge = Edge; this.Face = Face; }

        //**METHODS** //**ACTION**
        public Vector GetVector()
        {
            Point A = V[0].Point;
            Point B = V[1].Point;
            Vector output = Vector.ByTwoPoints(A, B);
            A.Dispose(); B.Dispose();
            return output;
        }
        public HalfEdge FlipDirection()
        {
            List<Vertex> temp = new List<Vertex>(V);
            V[0] = temp[1];
            V[1] = temp[0];
            temp = null;
            return this;
        }
        public bool AddEdge(Edge Edge)
        {
            if (this.Edge == null)
            {
                this.Edge = Edge;
                V[0].AddEdge(Edge);
                V[1].AddEdge(Edge);
                return true;
            }
            return false;
        }
        public bool AddFace(Face Face)
        {
            if (this.Face == null)
            {
                this.Face = Face;
                return true;
            }
            return false;
        }
    }

    public class Vertex : IEquatable<Vertex>
    {
        //**PROPERTIES** //**QUERY**
        public double[] Coordinates { get; private set; }
        public double X { get { return Coordinates[0]; } }
        public double Y { get { return Coordinates[1]; } }
        public double Z { get { return Coordinates[2]; } }
        public Point Point { get { return Point.ByCoordinates(X, Y, Z); } }
        public HashSet<Edge> Edges { get; private set; }
        public HashSet<Face> Faces { get; private set; }

        //**CONSTRUCTOR**
        internal Vertex(Point Point)
        {
            Coordinates = new double[] { Point.X, Point.Y, Point.Z };
            Edges = new HashSet<Edge>();
            Faces = new HashSet<Face>();
        }
        internal Vertex(Point Point, IEnumerable<Edge> Edges, IEnumerable<Face> Faces)
        {
            Coordinates = new double[] { Point.X, Point.Y, Point.Z };
            this.Edges = new HashSet<Edge>(Edges);
            this.Faces = new HashSet<Face>(Faces);
        }
        internal Vertex(double X, double Y, double Z)
        {
            Coordinates = new double[] { X, Y, Z };
            Edges = new HashSet<Edge>();
            Faces = new HashSet<Face>();
        }
        internal Vertex(double X, double Y, double Z, IEnumerable<Edge> Edges, IEnumerable<Face> Faces)
        {
            Coordinates = new double[] { X, Y, Z };
            this.Edges = new HashSet<Edge>(Edges);
            this.Faces = new HashSet<Face>(Faces);
        }

        //**METHODS** //**ACTION**
        [MultiReturn(new[] { "X", "Y", "Z" })]
        public Dictionary<string, double> GetCoordinates() { return new Dictionary<string, double> { { "X", X }, { "Y", Y }, { "Z", Z } }; }
        public void AddEdge(Edge Edge) { if (Edge.Vertices.Contains(this) && !Edges.Contains(Edge)) Edges.Add(Edge); }
        public void AddEdges(IEnumerable<Edge> Edges) { for (int i = 0; i < Edges.Count(); i++) AddEdge(Edges.ElementAt(i)); }
        public void AddFace(Face Face) { if (Face.Vertices.Contains(this) && !Faces.Contains(Face)) Faces.Add(Face); }
        public void AddFaces(IEnumerable<Face> Faces) { for (int i = 0; i < Faces.Count(); i++) AddFace(Faces.ElementAt(i)); }
        public double DistanceTo(Vertex Vertex)
        {
            double x = X - Vertex.X;
            double y = Y - Vertex.Y;
            double z = Z - Vertex.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public double DistanceTo(Point Point)
        {
            double x = X - Point.X;
            double y = Y - Point.Y;
            double z = Z - Point.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public bool IsAtPoint(Point Point) { return (X == Point.X && Y == Point.Y && Z == Point.Z); }

        //**METHODS**IEQUATABLE
        public override bool Equals(Object Object) { return this.Equals(Object as Vertex); }
        public bool Equals(Vertex Vertex)
        {
            if (Object.ReferenceEquals(Vertex, null)) return false;
            if (Object.ReferenceEquals(this, Vertex)) return true;
            if (this.GetType() != Vertex.GetType()) return false;
            return (X == Vertex.X && Y == Vertex.Y && Z == Vertex.Z);
        }
        public override int GetHashCode() { return string.Format("{0}-{1}-{2}", X, Y, Z).GetHashCode(); }
    }

    public class Edge
    {
        //**FIELDS**
        internal List<HalfEdge> E;
        internal double[] N;

        //**PROPERTIES** //**QUERY**
        public double Length { get { return E.ElementAt(0).Length; } }
        public string Name { get; set; }
        public double[] Angle { get; set; }
        public HalfEdge[] HalfEdges { get { return E.ToArray(); } }
        public Vector[] Normal
        {
            get
            {
                if (!(N.Length > 2)) return null;
                List<Vector> V = new List<Vector>();
                for (int i = 0; i < N.Length / 3; i++) V.Add(Vector.ByCoordinates(N[i * 3], N[i * 3 + 1], N[i * 3 + 2]));
                return V.ToArray();
            }
        }
        public Point MidPoint
        {
            get
            {
                Vertex[] V = Vertices; if (V.Length < 2 || V.Equals(null)) return null;
                return Point.ByCoordinates(V[0].X / 2 + V[1].X / 2, V[0].Y / 2 + V[1].Y / 2, V[0].Z / 2 + V[1].Z / 2);
            }
        }
        public List<Face> Faces { get { List<Face> F = new List<Face>(E.Count); E.ToList().ForEach(e => F.Add(e.Face)); return F; } }
        public Vertex[] Vertices { get { if (E.Count > 0) return E.ElementAt(0).V; return null; } }

        //**CONSTRUCTOR**
        internal Edge() { E = new List<HalfEdge>(); Name = ""; Angle = new double[] { 360 }; N = null; }
        internal Edge(IEnumerable<HalfEdge> HalfEdges) : this() { E = new List<HalfEdge>(HalfEdges); Vertices.ForEach(v => v.AddEdge(this)); }
        internal Edge(IEnumerable<HalfEdge> HalfEdges, string Name) : this(HalfEdges) { this.Name = Name; }

        //**METHODS** //**ACTION**
        public Vertex GetOtherVertex(Vertex Vertex)
        {
            if (Vertices[0].Equals(Vertex)) return Vertices[1];
            if (Vertices[1].Equals(Vertex)) return Vertices[0];
            return null;
        }
        internal double[] GetAngleNormal(HalfEdge eA, HalfEdge eB)
        {
            if (!E.Contains(eA) || !E.Contains(eB)) return null;
            Vector aN = eA.Face.Normal; Vector aX = eA.GetVector(); Vector aY = aN.Cross(aX).Normalized();
            Vector bN = eB.Face.Normal; Vector bX = eB.GetVector(); Vector bY = bN.Cross(bX).Normalized();
            Vector eN = aN.Add(bN).Normalized(); Vector eY = eN.Reverse();
            Point M = MidPoint; Point Ma = M.Add(aY); Point Mb = M.Add(bY); Point Me = M.Add(eY);
            Arc arc = Arc.ByThreePoints(Ma, Me, Mb);
            double[] result = new double[] { arc.SweepAngle - arc.StartAngle, eN.X, eN.Y, eN.Z };
            aN.Dispose(); aX.Dispose(); aY.Dispose(); bN.Dispose(); bX.Dispose(); bY.Dispose();
            eN.Dispose(); eY.Dispose(); M.Dispose(); Ma.Dispose(); Mb.Dispose(); Me.Dispose(); arc.Dispose();
            return result;
        }
        public Line GetLine()
        {
            Point a = Vertices[0].Point;
            Point b = Vertices[1].Point;
            Line output = Line.ByStartPointEndPoint(a, b);
            a.Dispose(); b.Dispose();
            return output;
        }
        public bool IsAtCurve(Curve Line)
        {
            return (Vertices[0].IsAtPoint(Line.EndPoint) && Vertices[1].IsAtPoint(Line.StartPoint)) || (Vertices[0].IsAtPoint(Line.StartPoint) && Vertices[1].IsAtPoint(Line.EndPoint));
        }
    }

    public class Spline
    {
        public List<Edge> Edges { get; set; }
        public List<Vertex> Vertices { get; set; }

        internal Spline() { Edges = new List<Edge>(); Vertices = new List<Vertex>(); }
    }

    public class Face : IDisposable
    {
        //**FIELDS**
        internal bool disposed = false;
        internal List<HalfEdge> E;

        //**PROPERTIES** //**QUERY**
        public string Name { get; set; }
        public CoordinateSystem CS { get; set; }
        public Point Center { get { return CS.Origin; } }
        public Vector Normal { get { return CS.ZAxis; } }
        public Dictionary<string, Object> Parameters { get; set; }
        public List<HalfEdge> HalfEdges { get { return E; } }
        public Vertex[] Vertices
        {
            get
            {
                Vertex[] output = new Vertex[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].V[0];
                return output;
            }
        }
        public Edge[] Edges
        {
            get
            {
                Edge[] output = new Edge[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].Edge;
                return output;
            }
        }
        public Point[] VertexPoints
        {
            get
            {
                Point[] output = new Point[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].V[0].Point;
                return output;
            }
        }
        public Vector[][] VertexVectors
        {
            get
            {
                List<Vector[]> eV = new List<Vector[]>(3);
                for (int i = 0; i < 3; i++)
                {
                    List<Vector> V = new List<Vector> { E[i].GetVector().Normalized(), E[(i + 2) % 3].GetVector().Normalized().Reverse() };
                    V.Add(V[0].Add(V[1]).Normalized());
                    V.Add(V[0].Subtract(V[1]).Normalized());
                    eV.Add(V.ToArray());
                }
                return eV.ToArray();
            }
        }

        //**CONSTRUCTOR**
        internal Face() { Name = ""; Parameters = new Dictionary<string, object>(); }
        internal Face(IEnumerable<Vertex> Vertices)
            : this()
        {
            E = new List<HalfEdge>(Vertices.ToList().Count);
            for (int i = 0; i < Vertices.Count(); i++)
            {
                E.Add(new HalfEdge(Vertices.ElementAt(i), Vertices.ElementAt((i + 1) % E.Capacity)));
                Vertices.ElementAt(i).AddFace(this as Face);
            }
            E.ForEach(he => he.AddFace(this));
            double[] xyz = { 0, 0, 0 };
            for (int i = 0; i < E.Count; i++)
            {
                xyz[0] += Vertices.ElementAt(i).X / E.Count;
                xyz[1] += Vertices.ElementAt(i).Y / E.Count;
                xyz[2] += Vertices.ElementAt(i).Z / E.Count;
            }
            Point Center = Point.ByCoordinates(xyz[0], xyz[1], xyz[2]);
            SetCS(Center);
            Center.Dispose();
        }

        //**METHODS** //**ACTION**
        public Face ReOrderVertices(Vertex Start)
        {
            if (E[0].V[0].Equals(Start)) return null;
            int index = 0;
            HalfEdge[] temp = new HalfEdge[E.Count];
            E.CopyTo(temp);
            for (int i = 1; i < E.Count; i++) if (E[i].Equals(Start)) { index = i; break; }
            for (int i = 0; i < E.Count; i++) E[i] = temp[(index + 1) % E.Count];
            temp = null;
            SetCS(CS.Origin);
            return this;
        }
        public void AddParameter(string Name, Object Object)
        {
            Parameters.Add(Name, Object);
        }
        public Object GetParameter(string Name)
        {
            if (Parameters.Keys.Contains(Name)) return Parameters[Name];
            return null;
        }

        //**METHODS**INTERNAL
        internal void SetCS(Point Center)
        {
            Vector Y = E[E.Count - 1].GetVector().Reverse();
            Vector X = E[0].GetVector();
            Vector Z = X.Cross(Y);
            Y = Z.Cross(X);
            CS = CoordinateSystem.ByOriginVectors(Center, X, Y);
            X.Dispose(); Y.Dispose(); Z.Dispose();
        }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                E.ForEach(e => e.Edge.E.Remove(e));
                E.Clear();
                if (Center != null) Center.Dispose();
                if (Normal != null) Normal.Dispose();
                if (CS != null) CS.Dispose();
                if (Parameters != null) for (int i = 0; i < Parameters.Count; i++)
                        if (Parameters.Values.ToArray()[i] is IDisposable) ((IDisposable)Parameters.Values.ToArray()[i]).Dispose();
            }
            disposed = true;
        }
    }

    public class Triangle : Face
    {
        //**PROPERTIES** //**QUERY**
        public Point Circumcenter { get; private set; }
        public Point Incenter { get; private set; }
        public double[] Angles
        {
            get
            {
                return new double[]{ 
                                 Math.Acos((E[2].Length * E[2].Length + E[0].Length * E[0].Length - E[1].Length * E[1].Length) / (2 * E[2].Length * E[0].Length)), 
                                 Math.Acos((E[0].Length * E[0].Length + E[1].Length * E[1].Length - E[2].Length * E[2].Length) / (2 * E[0].Length * E[1].Length)),
                                 Math.Acos((E[1].Length * E[1].Length + E[2].Length * E[2].Length - E[0].Length * E[0].Length) / (2 * E[1].Length * E[2].Length))
                             };
            }
        }
        public double MinEdgeAngle
        {
            get
            {
                double min = 360;
                for (int i = 0; i < 3; i++) if (min > E[i].Angle) min = E[i].Angle;
                return min;
            }
        }

        //**CONSTRUCTOR**
        internal Triangle(IEnumerable<Vertex> Vertices)
            : base(Vertices)
        {
            Point[] pts = { Vertices.ElementAt(0).Point, Vertices.ElementAt(1).Point, Vertices.ElementAt(2).Point };
            Circle c = Circle.ByBestFitThroughPoints(pts);
            Circumcenter = c.CenterPoint;
            c.Dispose(); pts[0].Dispose(); pts[1].Dispose(); pts[2].Dispose(); pts = null;
        }
        public Vertex GetOtherVertex(Edge Edge)
        {
            List<Vertex> V = new List<Vertex>(Vertices);
            V.RemoveAll(v => Edge.Vertices.Contains(v));
            if (V.Count > 1) return null;
            return V[0];
        }


        //**METHODS** //**ACTION**
        protected new virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                if (Circumcenter != null) Circumcenter.Dispose();
                if (Incenter != null) Incenter.Dispose();
                base.Dispose();
            }
            disposed = true;
        }
    }

    public class Quad : Face
    {
        //**FIELDS**
        protected new virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                base.Dispose();
            }
            disposed = true;
        }
    }
}
