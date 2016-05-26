﻿using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Fabrication
{
    public static class VectorMath
    {
        /// <summary>
        /// PI as double
        /// </summary>
        public static double PI = 3.14159265358979323846;

        /// <summary>
        /// returns modulo operation on a double
        /// with given input number and modulus
        /// also the remainder after Euclidean division
        /// </summary>
        /// <param name="Number">Input Number</param>
        /// <param name="Modulus">Modulus</param>
        /// <returns>Remainder > 0</returns>
        public static double Mod(double Number, double Modulus)
        {
            return Number - Math.Abs(Modulus) * Math.Floor(Number / Math.Abs(Modulus));
        }

        /// <summary>
        /// takes angle in degrees and converts into radians
        /// </summary>
        /// <param name="Angle">Angle in Degrees</param>
        /// <returns>Angle in Radians</returns>
        public static double toRadians(double Angle)
        {
            return Angle * PI / 180;
        }

        /// <summary>
        /// takes angle in radians and converts into degrees
        /// </summary>
        /// <param name="Angle">Angle in Radians</param>
        /// <returns>Angle in Degrees</returns>
        public static double toDegrees(double Angle)
        {
            return Angle * 180 / PI;
        }

        /// <summary>
        /// returns angle between vectors in radians
        /// angle is equal to or less than 180
        /// </summary>
        /// <param name="V1">Vector 1</param>
        /// <param name="V2">Vector 2</param>
        /// <returns>Angle in Radians</returns>
        public static double AngleBetween(Vector V1, Vector V2)
        {
            Vector N1 = V1.Normalized();
            Vector N2 = V2.Normalized();
            double a = Math.Acos(VectorMath.Dot(N1, N2));
            N1.Dispose(); N2.Dispose();
            return a;
        }

        /// <summary>
        /// returns dot product between two vectors
        /// </summary>
        /// <param name="V1">Vector 1</param>
        /// <param name="V2">Vector 2</param>
        /// <returns>Dot Product</returns>
        public static double Dot(Vector V1, Vector V2)
        {
            return V1.X * V2.X + V1.Y * V2.Y + V1.Z * V2.Z;
        }

        /// <summary>
        /// takes an orthogonal basis or object coordinate system (OCS) 
        /// and returns orthonormal basis as object coordinate
        /// </summary>
        /// <param name="CS">Object Coordinate System</param>
        /// <returns>Normalized OCS</returns>
        public static Vector[] NormalizeCS(Vector[] CS)
        {
            return new Vector[] { CS[0].Normalized(), CS[1].Normalized(), CS[2].Normalized() };
        }
        
        /// <summary>
        /// return normalized cross vector of two vectors
        /// </summary>
        /// <param name="V1">Vector 1</param>
        /// <param name="V2">Vector 2</param>
        /// <returns></returns>
        public static Vector NormalizedCross(Vector V1, Vector V2)
        {
            double v = Math.Sqrt(Math.Pow(V1.Y * V2.Z - V2.Y * V1.Z, 2) + Math.Pow(V1.Z * V2.X - V2.Z * V1.X, 2) + Math.Pow(V1.X * V2.Y - V2.X * V1.Y, 2));
            Vector V = Vector.ByCoordinates((V1.Y * V2.Z - V2.Y * V1.Z)/v, (V1.Z * V2.X - V2.Z * V1.X)/v, (V1.X * V2.Y - V2.X * V1.Y)/v);
            return V;
        }

        /// <summary>
        /// creates right-handed orthogonal vector to normal
        /// that lies in the global XY plane
        /// </summary>
        /// <param name="Normal"></param>
        /// <returns></returns>
        public static Vector NormalizedZeroVector(Vector Normal)
        {
            return Vector.ByCoordinates(-Math.Sign(Normal.Z) * Normal.Y, Math.Sign(Normal.Z) * Normal.X, 0).Normalized();
        }

        /// <summary>
        /// creates vector basis based on normal vector with normal as Z-axis
        /// where the X-Axis is the right-handed orthogonal vector to normal
        /// that lies in the global XY plane with the normal being the Z-axis;
        /// the Y-axis is generated by right-hand rule from Z cross X
        /// </summary>
        /// <param name="Normal"></param>
        /// <returns></returns>
        public static Vector[] NormalizedBasisXY(Vector Normal)
        {
            Vector[] result = new Vector[3];
            result[2] = Normal.Normalized();
            result[0] = NormalizedZeroVector(Normal);
            result[1] = NormalizedCross(result[2],result[0]);
            return result;
        }

        /// <summary>
        /// creates orthonormal vector basis
        /// based on given X-Axis and Y-Axis Vectors
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static Vector[] NormalizedBasis(Vector X, Vector Y)
        {
            Vector[] result = new Vector[3];
            result[0] = X.Normalized();
            result[2] = NormalizedCross(X, Y);
            result[1] = NormalizedCross(result[2], result[0]);
            return result;
        }

        /// <summary>
        /// creates vector basis based on normal vector as defined in dxf OCS
        /// if x-coordinate and y-coordinate of normal vector is within 1/64 of 0
        /// then X-Axis is Vector(0,1,0) cross normal vector
        /// otherwise, X-Axis is (0,0,1) cross normal vector
        /// the Y-axis is generated by right-hand rule from Z cross X
        /// </summary>
        /// <param name="Normal"></param>
        /// <returns></returns>
        public static Vector[] NormalizedBasisDXF(Vector Normal)
        {
            Vector[] XYZ = new Vector[3];
            XYZ[2] = Normal.Normalized();
            if (Math.Abs(XYZ[2].X) < 1.0/64 && Math.Abs(XYZ[2].Y) < 1.0/ 64) XYZ[0] = NormalizedCross(Vector.YAxis(), XYZ[2]);
            else XYZ[0] = NormalizedCross(Vector.ZAxis(), XYZ[2]);
            XYZ[1] = NormalizedCross(XYZ[2], XYZ[0]);
            return XYZ;
        }

        /// <summary>
        /// calculates coefficients of vector from universal basis
        /// to vector basis with given vectors as basis
        /// </summary>
        /// <param name="V">Vector in Universal Basis</param>
        /// <param name="X">X-axis of Basis</param>
        /// <param name="Y">Y-axis of Basis</param>
        /// <param name="Z">Z-axis of Basis</param>
        /// <returns>Coefficients in Given Basis</returns>
        public static double[] ChangeBasis(Vector V, Vector X, Vector Y, Vector Z)
        {
            if (X.Length * Y.Length * Z.Length == 0) return new double[] { 0, 0, 0 };
            return new double[] { Dot(V, X) / Dot(X, X), Dot(V, Y) / Dot(Y, Y), Dot(V, Z) / Dot(Z, Z) };
        }

        /// <summary>
        /// calculates coefficients of vector from universal basis
        /// to vector basis with given vectors as basis
        /// </summary>
        /// <param name="V">Vector to Decompose</param>
        /// <param name="XYZ">Orthogonal Basis as Vector Array [X,Y,Z]</param>
        /// <returns>Coefficients for Decomposition (a, b, c) where V = aX + bY + cZ</returns>
        public static double[] ChangeBasis(Vector V, Vector[] XYZ)
        {
            return ChangeBasis(V, XYZ[0], XYZ[1], XYZ[2]);
        }

        /// <summary>
        /// calculates coefficients of vector from universal basis
        /// to vector basis provided by given coordinate system
        /// </summary>
        /// <param name="V">Vector in Universal Basis</param>
        /// <param name="CS">Coordinate System</param>
        /// <returns>Coefficients in Coordinate System Basis</returns>
        public static double[] ChangeBasis(Vector V, CoordinateSystem CS)
        {
            return ChangeBasis(V, CS.XAxis, CS.YAxis, CS.ZAxis);
        }

        /// <summary>
        /// calculates new coordinates of point from universal basis
        /// to vector basis with given vectors as basis
        /// </summary>
        /// <param name="Point">Point in Universal Basis</param>
        /// <param name="XYZ">Orthogonal Basis as Vector Array [X,Y,Z]</param>
        /// <returns>Point Coordinates in Orthogonal Basis (a, b, c) where Point = aX + bY + cZ</returns>
        public static double[] ChangeBasis(Point Point, Vector[] XYZ)
        {
            Vector V = Vector.ByTwoPoints(Point.Origin(), Point);
            double[] result = ChangeBasis(V, XYZ);
            V.Dispose();
            return result;
        }

        /// <summary>
        /// calculates new coordinates of point from universal basis
        /// to vector basis provided by given coordinate system
        /// </summary>
        /// <param name="Point">Point in Universal Basis</param>
        /// <param name="CS">Coordinate System</param>
        /// <returns>Point Coordinates in Coordinate System Basis</returns>
        public static double[] ChangeBasis(Point Point, CoordinateSystem CS)
        {
            Vector V = Vector.ByTwoPoints(Point.Origin(), Point);
            double[] result = ChangeBasis(V, CS);
            V.Dispose();
            return result;
        }

        /// <summary>
        /// calculates new coordinates of point from universal basis
        /// to vector basis derived from given normal using NormalizedBasisDXF
        /// </summary>
        /// <param name="Point">Point in Universal Basis</param>
        /// <param name="Normal">Normal Vector</param>
        /// <returns>Point Coordinates in Coordinate System Basis</returns>
        public static double[] ChangeBasisDXF(Point Point, Vector Normal)
        {
            return ChangeBasis(Vector.ByCoordinates(Point.X,Point.Y,Point.Z), NormalizedBasisDXF(Normal));
        }

        /// <summary>
        /// calculates new coordinates of point from universal basis
        /// to vector basis derived from given normal using NormalizedBasisDXF
        /// </summary>
        /// <param name="V">Vector in Universal Basis</param>
        /// <param name="Normal">Normal Vector</param>
        /// <returns>Coefficients in Coordinate System Basis</returns>
        public static double[] ChangeBasisDXF(Vector Vector, Vector Normal)
        {
            return ChangeBasis(Vector, NormalizedBasisDXF(Normal));
        }

        /// <summary>
        /// checks if two vectors are parallel
        /// using methods in cross product
        /// </summary>
        /// <param name="V1">Vector 1</param>
        /// <param name="V2">Vector 2</param>
        /// <returns></returns>
        public static bool IsParallel(Vector V1, Vector V2)
        {
            double x = V1.Y * V2.Z - V2.Y * V1.Z;
            double y = V1.Z * V2.X - V2.Z * V1.X;
            double z = V1.X * V2.Y - V2.X * V1.Y;
            return (x + y + z).Equals(0);
        }

        /// <summary>
        /// checks if two vectors are perpendicular using dot product
        /// (dot product of perpendicular vectors is 0)
        /// </summary>
        /// <param name="V1">Vector 1</param>
        /// <param name="V2">Vector 2</param>
        /// <returns></returns>
        public static bool IsPerpendicular(Vector V1, Vector V2)
        {
            return Dot(V1, V2).Equals(0);
        }
    }


    public class MaptoXY
    {
        /// <summary>
        /// map polycurves in index to XY plane
        /// with given spacing and at given y-coordinate
        /// </summary>
        public static List<List<PolyCurve>> MapPolyCurves(List<List<PolyCurve>> Curves, List<CoordinateSystem> CoordinateSystem, List<int> index, double Xspacing, double Ycoordinate)
        {
            List<List<PolyCurve>> result = new List<List<PolyCurve>>(index.Count);
            for (int i = 0; i < index.Count; i++)
            {
                List<PolyCurve> temp = new List<PolyCurve>();
                for (int j = 0; j < Curves[index[i]].Count; j++)
                {
                    CoordinateSystem CS = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(Xspacing * i, Ycoordinate);
                    temp.Add((PolyCurve)Curves[index[i]][j].Transform(CoordinateSystem[index[i]], CS));
                    CS.Dispose();
                }
                result.Add(temp);
            }
            return result;
        }

        /// <summary>
        /// map circles in index to XY plane
        /// with given spacing and at given y-coordinate
        /// </summary>
        public static List<List<Circle>> MapCircles(List<List<Circle>> Circles, List<CoordinateSystem> CoordinateSystem, List<int> index, double X, double Y)
        {
            CoordinateSystem CS = null;
            List<List<Circle>> result = new List<List<Circle>>(index.Count);
            for (int i = 0; i < index.Count; i++)
            {
                List<Circle> temp = new List<Circle>();
                for (int j = 0; j < Circles[index[i]].Count; j++)
                {
                    CS = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(X * i, Y);
                    temp.Add((Circle)Circles[index[i]][j].Transform(CoordinateSystem[index[i]], CS));
                }
                result.Add(temp);
            }
            if (CS != null) CS.Dispose();
            return result;
        }
        /// <summary>
        /// creates a grid of polycurves on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<PolyCurve> MapPolyCurve(PolyCurve[] PolyCurves, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<PolyCurve> result = new List<PolyCurve>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                result.Add((PolyCurve)PolyCurves[Index[i]].Transform(PolyCurves[Index[i]].ContextCoordinateSystem, targetCS));
                targetCS.Dispose();
            }
            return result;
        }
        /// <summary>
        /// creates a grid of curves on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<Curve> MapCurve(Curve[] Curves, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<Curve> result = new List<Curve>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                result.Add((Curve)Curves[Index[i]].Transform(Curves[Index[i]].ContextCoordinateSystem, targetCS));
                targetCS.Dispose();
            }
            return result;
        }
        /// <summary>
        /// creates a grid of geometry on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<Geometry> MapGeometry(Geometry[] Geometry, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<Geometry> result = new List<Geometry>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                result.Add(Geometry[Index[i]].Transform(Geometry[Index[i]].ContextCoordinateSystem, targetCS));
                targetCS.Dispose();
            }
            return result;
        }
        /// <summary>
        /// creates a grid of polycurves on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<List<PolyCurve>> MapPolyCurves(PolyCurve[][] PolyCurves, CoordinateSystem[] CS, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<List<PolyCurve>> result = new List<List<PolyCurve>>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                List<PolyCurve> temp = new List<PolyCurve>(PolyCurves[Index[i]].Length);
                for (int j = 0; j < PolyCurves[Index[i]].Length; j++)
                {
                    temp.Add((PolyCurve)PolyCurves[Index[i]][j].Transform(CS[Index[i]], targetCS));
                }
                targetCS.Dispose();
                result.Add(temp);
            }
            return result;
        }
        /// <summary>
        /// creates a grid of curves on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<List<Curve>> MapCurves(Curve[][] Curves, CoordinateSystem[] CS, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<List<Curve>> result = new List<List<Curve>>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                List<Curve> temp = new List<Curve>(Curves[Index[i]].Length);
                for (int j = 0; j < Curves[Index[i]].Length; j++)
                {
                    temp.Add((Curve)Curves[Index[i]][j].Transform(CS[Index[i]], targetCS));
                }
                targetCS.Dispose();
                result.Add(temp);
            }
            return result;
        }
        /// <summary>
        /// creates a grid of circles on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<List<Circle>> MapCircles(Circle[][] Circles, CoordinateSystem[] CS, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<List<Circle>> result = new List<List<Circle>>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                List<Circle> temp = new List<Circle>(Circles[Index[i]].Length);
                for (int j = 0; j < Circles[Index[i]].Length; j++)
                {
                    temp.Add((Circle)Circles[Index[i]][j].Transform(CS[Index[i]], targetCS));
                }
                targetCS.Dispose();
                result.Add(temp);
            }
            return result;
        }
        /// <summary>
        /// creates a grid of geometry on XY-plane 
        /// from an array of curves with a designated coordinate system
        /// </summary>
        public static List<List<Geometry>> MapGeometry(Geometry[][] Geometry, CoordinateSystem[] CS, int[] Index, double Xspacing = 0, int XmaxCount = 0, double Yspacing = 0, int YmaxCount = 0)
        {
            int Length = Index.Length;
            if (XmaxCount * YmaxCount > 0 && XmaxCount * YmaxCount < Index.Length) Length = XmaxCount * YmaxCount;
            double[,] position = new double[Length, 2];
            for (int i = 0; i < Length; i++)
            {
                if (XmaxCount + YmaxCount == 0)
                {
                    position[i, 0] = i;
                    position[i, 1] = 0;
                }
                else if (XmaxCount == 0)
                {
                    position[i, 1] = i % YmaxCount;
                    position[i, 0] = Math.Floor((double)i / YmaxCount);
                }
                else
                {
                    position[i, 0] = i % XmaxCount;
                    position[i, 1] = Math.Floor((double)i / XmaxCount);
                }
            }
            List<List<Geometry>> result = new List<List<Geometry>>();
            for (int i = 0; i < Length; i++)
            {
                CoordinateSystem targetCS = CoordinateSystem.ByOrigin(Xspacing * position[i, 0], Yspacing * position[i, 1]);
                List<Geometry> temp = new List<Geometry>(Geometry[Index[i]].Length);
                for (int j = 0; j < Geometry[Index[i]].Length; j++)
                {
                    temp.Add(Geometry[Index[i]][j].Transform(CS[Index[i]], targetCS));
                }
                targetCS.Dispose();
                result.Add(temp);
            }
            return result;
        }

    }
}
