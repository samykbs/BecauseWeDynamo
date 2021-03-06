﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Fabrication;

namespace Topology
{
    /// <summary>
    /// HalfEdge: ordered vertex array with reference to face, edge, normal, angle, and length 
    /// </summary>
    public class HalfEdge
    {
        //**FIELD
        internal Vertex[] V;

        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// gets angle for halfedge at edge
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// gets Length
        /// </summary>
        public double Length { get { return Math.Sqrt(Math.Pow(V[0].X - V[1].X, 2) + Math.Pow(V[0].Y - V[1].Y, 2) + Math.Pow(V[0].Z - V[1].Z, 2)); } }
        /// <summary>
        /// get normal vector of halfedge face
        /// </summary>
        public Vector Normal { get { return Face.Normal; } }
        /// <summary>
        /// gets Edge that contains this halfedge
        /// </summary>
        public Edge Edge { get; private set; }
        /// <summary>
        /// get Face that contins this halfedge
        /// </summary>
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

        //**METHODS** //**CREATE**
        /// <summary>
        /// creates HalfEdge instance
        /// </summary>
        /// <param name="Vertices">Vertices</param>
        /// <returns>HalfEdge</returns>
        public static HalfEdge ByVertices(IEnumerable<Vertex> Vertices) { return new HalfEdge(Vertices); }

        //**METHODS** //**ACTION**
        /// <summary>
        /// returns halfedge as vector
        /// </summary>
        /// <returns>Vector</returns>
        public Vector GetVector()
        {
            Point A = V[0].Point;
            Point B = V[1].Point;
            Vector output = Vector.ByTwoPoints(A, B);
            A.Dispose(); B.Dispose();
            return output;
        }
        /// <summary>
        /// adds reference edge if halfedge is part of edge
        /// and adds edge to vertices
        /// </summary>
        /// <param name="Edge">Mesh Edge</param>
        /// <returns>true if succeeded, false if failed</returns>
        public bool AddEdge(Edge Edge)
        {
            if (Edge.E.Contains(this))
            {
                this.Edge = Edge;
                V[0].AddEdge(Edge);
                V[1].AddEdge(Edge);
                return true;
            }
            return false;
        }
        /// <summary>
        /// adds reference face if halfedge is part of face
        /// and adds face to vertices
        /// </summary>
        /// <param name="Face">Mesh Face</param>
        /// <returns>>true if succeeded, false if failed</returns>
        public bool AddFace(Face Face)
        {
            if (Face.E.Contains(this))
            {
                this.Face = Face;
                V[0].AddFace(Face);
                V[1].AddFace(Face);
                return true;
            }
            return false;
        }
        /// <summary>
        /// flips direction of halfedge
        /// ie used when fliping face normals
        /// </summary>
        public void FlipDirection()
        {
            List<Vertex> temp = new List<Vertex>(V);
            V[0] = temp[1];
            V[1] = temp[0];
            temp = null;
        }
    }

    /// <summary>
    /// Vertex: coordinate list with reference to face and edge sets
    /// </summary>
    public class Vertex : IEquatable<Vertex>
    {
        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// XYZ coordinates
        /// </summary>
        public double[] Coordinates { get; private set; }
        /// <summary>
        /// X-coordinate
        /// </summary>
        public double X { get { return Coordinates[0]; } }
        /// <summary>
        /// Y-coordinate
        /// </summary>
        public double Y { get { return Coordinates[1]; } }
        /// <summary>
        /// Z-coordinate
        /// </summary>
        public double Z { get { return Coordinates[2]; } }
        /// <summary>
        /// Vertex Geometry as Point Object
        /// </summary>
        public Point Point { get { return Point.ByCoordinates(X, Y, Z); } }
        /// <summary>
        /// Edges connected to Vertex
        /// </summary>
        public HashSet<Edge> Edges { get; private set; }
        /// <summary>
        /// Faces connected to Vertex
        /// </summary>
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

        //**METHODS** //**CREAT**
        /// <summary>
        /// creates empty vertex at point
        /// </summary>
        /// <param name="Point">Point</param>
        /// <returns>Vertex</returns>
        public static Vertex ByPoint(Point Point) { return new Vertex(Point); }

        //**METHODS** //**ACTION**
        /// <summary>
        /// return XYZ coordinates of Vertex
        /// </summary>
        /// <returns>XYZ Coordinates</returns>
        [MultiReturn(new[] { "X", "Y", "Z" })]
        public Dictionary<string, double> GetCoordinates() { return new Dictionary<string, double> { { "X", X }, { "Y", Y }, { "Z", Z } }; }
        /// <summary>
        /// adds edge to vertex if connected
        /// </summary>
        /// <param name="Edge">Edge to be added</param>
        public void AddEdge(Edge Edge) { if (Edge.Vertices.Contains(this) && !Edges.Contains(Edge)) Edges.Add(Edge); }
        /// <summary>
        /// adds edges to vertex if connected
        /// </summary>
        /// <param name="Edges">Edges</param>
        public void AddEdges(IEnumerable<Edge> Edges) { for (int i = 0; i < Edges.Count(); i++) AddEdge(Edges.ElementAt(i)); }
        /// <summary>
        /// adds face to vertex if connected
        /// </summary>
        /// <param name="Face">Face</param>
        public void AddFace(Face Face) { if (Face.Vertices.Contains(this) && !Faces.Contains(Face)) Faces.Add(Face); }
        /// <summary>
        /// adds faces to vertex if connected
        /// </summary>
        /// <param name="Faces">Faces</param>
        public void AddFaces(IEnumerable<Face> Faces) { for (int i = 0; i < Faces.Count(); i++) AddFace(Faces.ElementAt(i)); }
        /// <summary>
        /// returns distance to given vertex
        /// </summary>
        /// <param name="Vertex">Vertex</param>
        /// <returns>Distance</returns>
        public double DistanceTo(Vertex Vertex)
        {
            double x = X - Vertex.X;
            double y = Y - Vertex.Y;
            double z = Z - Vertex.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// returns distance to given point
        /// </summary>
        /// <param name="Point">Point</param>
        /// <returns>Distance</returns>
        public double DistanceTo(Point Point)
        {
            double x = X - Point.X;
            double y = Y - Point.Y;
            double z = Z - Point.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// checks is vertex is located at point
        /// </summary>
        /// <param name="Point">Point</param>
        /// <returns>Boolean</returns>
        public bool IsAtPoint(Point Point) { return (X == Point.X && Y == Point.Y && Z == Point.Z); }

        //**METHODS**IEQUATABLE
        public override bool Equals(Object Object) { return this.Equals(Object as Vertex); }
        public bool Equals(Vertex Vertex)
        {
            if (Object.ReferenceEquals(Vertex, null)) return false;
            if (Object.ReferenceEquals(this, Vertex)) return true;
            if (this.GetType() != Vertex.GetType()) return false;
            return (X == Vertex.X && Y == Vertex.Y && Z == Vertex.Z && Vertex.Edges == Edges && Vertex.Faces == Faces);
        }
        public override int GetHashCode() { return string.Format("{0}-{1}-{2}-{3}-{4}", X, Y, Z, Edges.GetHashCode(), Faces.GetHashCode()).GetHashCode(); }
    }

    /// <summary>
    /// Edge: HalfEdge List with reference to faces, vertices, length, name, angle, normal, midpoint, and length
    /// </summary>
    public class Edge
    {
        //**FIELDS**
        internal List<HalfEdge> E;
        internal double[] N;

        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// Edge Length
        /// </summary>
        public double Length { get { return E.ElementAt(0).Length; } }
        /// <summary>
        /// Edge Name for Label
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Angle between Faces at Edge
        /// </summary>
        public double[] Angle { get; set; }
        /// <summary>
        /// HalfEdge Array
        /// </summary>
        public HalfEdge[] HalfEdges { get { return E.ToArray(); } }
        /// <summary>
        /// Normal Vector
        /// </summary>
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
        /// <summary>
        /// Midpoint
        /// </summary>
        public Point MidPoint
        {
            get
            {
                Vertex[] V = Vertices; if (V.Length < 2 || V.Equals(null)) return null;
                return Point.ByCoordinates(V[0].X / 2 + V[1].X / 2, V[0].Y / 2 + V[1].Y / 2, V[0].Z / 2 + V[1].Z / 2);
            }
        }
        /// <summary>
        /// Face List
        /// </summary>
        public List<Face> Faces { get { List<Face> F = new List<Face>(E.Count); E.ToList().ForEach(e => F.Add(e.Face)); return F; } }
        /// <summary>
        /// Vertex Array
        /// </summary>
        public Vertex[] Vertices { get { if (E.Count > 0) return E.ElementAt(0).V; return null; } }

        //**CONSTRUCTOR**
        internal Edge() { E = new List<HalfEdge>(); Name = ""; Angle = new double[] { 360 }; N = null; }
        internal Edge(IEnumerable<HalfEdge> HalfEdges) : this() { E = new List<HalfEdge>(HalfEdges); Vertices.ForEach(v => v.AddEdge(this)); }
        internal Edge(IEnumerable<HalfEdge> HalfEdges, string Name) : this(HalfEdges) { this.Name = Name; }

        //**METHODS**CREATE
        /// <summary>
        /// create and Edge object as an array of HalfEdges
        /// </summary>
        /// <param name="HalfEdges">HalfEdges</param>
        /// <returns>Edge</returns>
        public static Edge ByHalfEdges(IEnumerable<HalfEdge> HalfEdges) { return new Edge(HalfEdges); }

        //**METHODS** //**ACTION**
        /// <summary>
        /// get other vertex
        /// </summary>
        /// <param name="Vertex">Vertex</param>
        /// <returns>Other Vertex</returns>
        public Vertex GetOtherVertex(Vertex Vertex)
        {
            if (Vertices[0].Equals(Vertex)) return Vertices[1];
            if (Vertices[1].Equals(Vertex)) return Vertices[0];
            return null;
        }
        /// <summary>
        /// get normal vector to two faces that create an angle
        /// </summary>
        /// <param name="eA">HalfEdge A</param>
        /// <param name="eB">HalfEdge B</param>
        /// <returns></returns>
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
        /// <summary>
        /// creates line geometry based on edge
        /// </summary>
        /// <returns>Line</returns>
        public Line GetLine()
        {
            Point a = Vertices[0].Point;
            Point b = Vertices[1].Point;
            Line output = Line.ByStartPointEndPoint(a, b);
            a.Dispose(); b.Dispose();
            return output;
        }
        /// <summary>
        /// checks to see if edge has same geometric properties as given line
        /// </summary>
        /// <param name="Line">Line</param>
        /// <returns>Boolean</returns>
        public bool IsAtCurve(Curve Line)
        {
            return (Vertices[0].IsAtPoint(Line.EndPoint) && Vertices[1].IsAtPoint(Line.StartPoint)) || (Vertices[0].IsAtPoint(Line.StartPoint) && Vertices[1].IsAtPoint(Line.EndPoint));
        }
    }

    /// <summary>
    /// Spline: HalfEdge List with reference to vertices
    /// </summary>
    public class Spline
    {
        /// <summary>
        /// Hlaf Edge List
        /// </summary>
        public List<Edge> Edges { get; set; }
        /// <summary>
        /// Vertex List
        /// </summary>
        public List<Vertex> Vertices { get; set; }

        internal Spline() { Edges = new List<Edge>(); Vertices = new List<Vertex>(); }
        internal Spline(Edge[] Edges) : base()
        {
            this.Edges.AddRange(Edges);
            for (int i=0; i<Edges.Length; i++)
            {
                if (!Vertices.Contains(Edges[i].Vertices[0])) Vertices.Add(Edges[i].Vertices[0]);
                if (!Vertices.Contains(Edges[i].Vertices[1])) Vertices.Add(Edges[i].Vertices[1]);

            }
        }

        //**METHODS**CREATE**
        /// <summary>
        /// creates spline from HalfEdgeList
        /// </summary>
        /// <param name="Edges"></param>
        /// <returns></returns>
        public static Spline ByHalfEdges(Edge[] Edges)
        {
            return new Spline(Edges);
        }
    }

    /// <summary>
    /// Face: HalfEdge List with references to edges, vertices, center, name, angle, normal, midpoint, and length
    /// </summary>
    public class Face : IDisposable
    {
        //**FIELDS**
        internal bool disposed = false;
        internal List<HalfEdge> E;

        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// Name: face label
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// CS: context coordinate system
        /// </summary>
        public CoordinateSystem CS { get; set; }
        /// <summary>
        /// Center: centerpoint of face
        /// </summary>
        public Point Center { get { return CS.Origin; } }
        /// <summary>
        /// Normal: face normal that defines outside
        /// </summary>
        public Vector Normal { get { return CS.ZAxis.Normalized(); } }
        /// <summary>
        /// Parameters: dictionary keyed by strings that stores metadata
        /// </summary>
        public Dictionary<string, Object> Parameters { get; set; }
        /// <summary>
        /// HalfEdges: halfedge list
        /// </summary>
        public List<HalfEdge> HalfEdges { get { return E; } }
        /// <summary>
        /// Vertices: veterx array
        /// </summary>
        public Vertex[] Vertices
        {
            get
            {
                Vertex[] output = new Vertex[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].V[0];
                return output;
            }
        }
        /// <summary>
        /// Edges: associated edge array
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                Edge[] output = new Edge[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].Edge;
                return output;
            }
        }
        /// <summary>
        /// VertexPoints: array of vertex point geometry 
        /// </summary>
        public Point[] VertexPoints
        {
            get
            {
                Point[] output = new Point[E.Count];
                for (int i = 0; i < E.Count; i++) output[i] = E[i].V[0].Point;
                return output;
            }
        }
        /// <summary>
        /// Vertex Vector Array:
        /// V1 is normalized vector from vertex in right-hand rule
        /// V2 is normalized vector from vertex in other direction
        /// N is face normal ie. V1 x V2 (cross product).
        /// returns array {V1, V2, V1+V2 (vertex bisector), V1-V2 (ON vector to biscetor and normal), NxV1 + V2xN(exterior bisector)}
        /// </summary>
        public Vector[][] VertexVectors
        {
            get
            {
                List<Vector[]> eV = new List<Vector[]>(E.Count);
                for (int i = 0; i < E.Count; i++)
                {
                    int j = (i + E.Count - 1) % E.Count;
                    Vector[] V = VectorMath.NormalizedVertexVectors(E[i].GetVector(), E[j].GetVector().Reverse(), Normal);
                    eV.Add(V.ToArray());
                }
                return eV.ToArray();
            }
        }
        /// <summary>
        /// Angles: interior angles for face
        /// </summary>
        public double[] Angles
        {
            get
            {
                Point O = Point.ByCoordinates(0, 0, 0);
                double[] angles = new double[E.Count];
                for (int i = 0; i < E.Count; i++)
                {
                    Point A = O.Add(VertexVectors[i][1]);
                    Point B = O.Add(VertexVectors[i][0]);
                    Point M = O.Add(VertexVectors[i][4]);
                    Arc arc = Arc.ByThreePoints(A, M, B);
                    angles[i] = arc.SweepAngle - arc.StartAngle;
                    A.Dispose(); B.Dispose(); M.Dispose(); arc.Dispose();
                }
                O.Dispose();
                return angles;
            }
        }
        /// <summary>
        /// MinEdgeAngle: sharpest corner made with adjacent face
        /// </summary>
        public double MinEdgeAngle
        {
            get
            {
                double min = 360;
                for (int i = 0; i < E.Count; i++) if (min > E[i].Angle) min = E[i].Angle;
                return min;
            }
        }

        //**CONSTRUCTOR**
        public Face() { Name = ""; Parameters = new Dictionary<string, object>(); }
        public Face(IEnumerable<Vertex> Vertices,Vector Normal)
            : this()
        {
            Vertex[] V = Vertices.ToArray();
            E = new List<HalfEdge>(Vertices.ToList().Count);

            for (int i = 0; i < V.Length; i++)
            {
                E.Add(new HalfEdge(V[i], V[(i + 1) % E.Capacity]));
                V[i].AddFace(this as Face);
            }
            E.ForEach(he => he.AddFace(this));
            double[] xyz = { 0, 0, 0 };
            for (int i = 0; i < E.Count; i++)
            {
                xyz[0] += V[i].X / E.Count;
                xyz[1] += V[i].Y / E.Count;
                xyz[2] += V[i].Z / E.Count;
            }
            Point Center = Point.ByCoordinates(xyz[0], xyz[1], xyz[2]);
            SetCS(Center, Normal);
            Center.Dispose();
        }

        //**METHODS**CREATE
        /// <summary>
        /// creates face from ordered vertices and normal
        /// </summary>
        /// <param name="Vertices"></param>
        /// <param name="Normal"></param>
        /// <returns></returns>
        public static Face ByVertices(IEnumerable<Vertex> Vertices, Vector Normal) { return new Face(Vertices, Normal); }

        //**METHODS** //**ACTION**
        /// <summary>
        /// reorders vertices based on given start vertex
        /// </summary>
        /// <param name="Start"></param>
        /// <returns></returns>
        public Face ReOrderVertices(Vertex Start)
        {
            if (E[0].V[0].Equals(Start)) return null;
            int index = 0;
            HalfEdge[] temp = new HalfEdge[E.Count];
            E.CopyTo(temp);
            for (int i = 1; i < E.Count; i++) if (E[i].Equals(Start)) { index = i; break; }
            for (int i = 0; i < E.Count; i++) E[i] = temp[(index + 1) % E.Count];
            temp = null;
            SetCS(CS.Origin, CS.ZAxis);
            return this;
        }
        /// <summary>
        /// adds parameter to object
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Object"></param>
        public void AddParameter(string Name, Object Object)
        {
            Parameters.Add(Name, Object);
        }
        /// <summary>
        /// retrieves data by parameter name
        /// returns null if parameter name doesn't exist
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public Object GetParameter(string Name)
        {
            if (Parameters.Keys.Contains(Name)) return Parameters[Name];
            return null;
        }

        //**METHODS**INTERNAL
        internal void SetCS(Point Center, Vector Normal)
        {
            Vector Z = Normal;
            Vector X = E[0].GetVector();
            Vector Y = Z.Cross(X);
            X = Y.Cross(Z);
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

    /// <summary>
    /// traingle mesh face
    /// </summary>
    public class Triangle : Face
    {
        //**CONSTRUCTOR**
        public Triangle() : base() { }
        public Triangle(IEnumerable<Vertex> Vertices, Vector Normal) : base(Vertices.Take(3), Normal) { }

        public static Triangle ByVerticesNormal(IEnumerable<Vertex> Vertices, Vector Normal) { return new Triangle(Vertices, Normal); }
        public static Triangle ByVertices(IEnumerable<Vertex> Vertices) 
        { 
            Vector X = Vector.ByTwoPoints(Vertices.ElementAt(0).Point,Vertices.ElementAt(1).Point);
            Vector Y = Vector.ByTwoPoints(Vertices.ElementAt(0).Point,Vertices.ElementAt(2).Point);
            Vector N = X.Cross(Y);
            X.Dispose(); Y.Dispose();
            return new Triangle(Vertices, N);
        }

        public Point GetCircumcenter()
        {
            Point[] pts = { Vertices.ElementAt(0).Point, Vertices.ElementAt(1).Point, Vertices.ElementAt(2).Point };
            Circle c = Circle.ByBestFitThroughPoints(pts);
            Point Circumcenter = c.CenterPoint;
            c.Dispose(); pts.ForEach(p => p.Dispose());
            return Circumcenter;
        }
        public Point GetIncenter()
        {
            double D = E[0].Length + E[1].Length + E[2].Length;
            double X = E[1].Length * E[0].V[0].X + E[2].Length * E[1].V[0].X + E[0].Length * E[2].V[0].X;
            double Y = E[1].Length * E[0].V[0].Y + E[2].Length * E[1].V[0].Y + E[0].Length * E[2].V[0].Y;
            double Z = E[1].Length * E[0].V[0].Z + E[2].Length * E[1].V[0].Z + E[0].Length * E[2].V[0].Z;
            return Point.ByCoordinates(X / D, Y / D, Z / D);
        }
        public Vertex GetOtherVertex(Edge Edge)
        {
            List<Vertex> V = new List<Vertex>(Vertices);
            V.RemoveAll(v => Edge.Vertices.Contains(v));
            if (V.Count > 1) return null;
            return V[0];
        }
    }

    /// <summary>
    /// quad mesh face
    /// </summary>
    public class Quad : Face
    {
        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// diagonal
        /// </summary>
        public int Diagonal { get; private set; }

        //**CONSTRUCTOR**
        internal Quad(IEnumerable<Vertex> Vertices, Vector Normal)
            : base(Vertices.Take(4), Normal)
        {
            Diagonal = 0;
            if (E[0].V[0].DistanceTo(E[2].V[0]) > E[1].V[0].DistanceTo(E[3].V[0])) Diagonal = 1;
        }

        //**METHODS**CREATE
        public static Quad ByVertices(IEnumerable<Vertex> Vertices, Vector Normal) { return new Quad(Vertices, Normal); }

        //**METHODS**ACTION
        public void FlipDiagonal()
        {
            Diagonal = (Diagonal + 1) % 2;
        }
        public Line GetDiagonal()
        {
            Point a = E[Diagonal].V[0].Point;
            Point b = E[Diagonal + 2].V[0].Point;
            Line L = Line.ByStartPointEndPoint(a, b);
            a.Dispose(); b.Dispose();
            return L;
        }
    }
}
