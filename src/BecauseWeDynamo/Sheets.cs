﻿using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Fabrication
{
    public class Sheets: IDisposable
    {
        bool disposed = false;
        public List<List<PolyCurve>> Curves { get; set; }
        public List<List<Circle>> Circles { get; set; }
        public List<CoordinateSystem> CoordinateSystem { get; set; }


        internal Sheets(List<List<PolyCurve>> Curves, List<CoordinateSystem> CoordinateSystem)
        {
            this.CoordinateSystem = CoordinateSystem;
            this.Curves = Curves;
        }
        internal Sheets(List<List<Circle>> Circles, List<CoordinateSystem> CoordinateSystem)
        {
            this.CoordinateSystem = CoordinateSystem;
            this.Circles = Circles;
        }
        
        //**CREATE
        /// <summary>
        /// creates a sheet layout of an array of Polycurves with designated coordinate system
        /// </summary>
        /// <param name="Curves">list of polycurves</param>
        /// <param name="CS">list of polycurves coordinate system</param>
        /// <returns>sheet object</returns>
        public static Sheets ByPolyCurvesAndCS(List<List<PolyCurve>> Curves, List<CoordinateSystem> CS) {  return new Sheets(Curves, CS); }
        /// <summary>
        /// creates a sheet layout of an array of Circles with designated coordinate system
        /// </summary>
        /// <param name="Circles">list of circles</param>
        /// <param name="CS">list of circles coordinate system</param>
        /// <returns>sheet object</returns>
        public static Sheets ByCirclesAndCS(List<List<Circle>> Circles, List<CoordinateSystem> CS) { return new Sheets(Circles, CS); }
        /// <summary>
        /// creates a sheet layout of an array of Curves with designated coordinate system
        /// </summary>
        /// <param name="Curves">list of curves</param>
        /// <param name="CS">list of curves coordinate system</param>
        /// <returns>sheet object</returns>
        public static Sheets ByCurvesAndCS(List<List<Curve>> Curves, List<CoordinateSystem> CS)
        {
            List<List<PolyCurve>> result = new List<List<PolyCurve>>(Curves.Count);
            for (int i = 0; i < Curves.Count; i++) for (int j = 0; j < Curves[i].Count; j++)
                result[i][j] = PolyCurve.ByJoinedCurves( new List<Curve>{Curves[i][j]});
            return new Sheets(result, CS);
        }

        //**ACTIONS
        /// <summary>
        /// returns a row containing given index list
        /// </summary>
        /// <param name="index">index list</param>
        /// <param name="X">spacing in X direction</param>
        /// <param name="Y">Y-coordinate</param>
        /// <returns>row of polycurves as list of polycurves</returns>
        public List<List<PolyCurve>> GetPolyCurveIndexAsRow(List<int> index, double X, double Y)
        {
            CoordinateSystem CS = null;
            List<List<PolyCurve>> result = new List<List<PolyCurve>>(index.Count);
            for (int i = 0; i < index.Count; i++) for (int j = 0; j < Curves[index[i]].Count; j++)
                {
                    CS = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(X * i, Y);
                    result[i][j] = (PolyCurve)Curves[index[i]][j].Transform(CoordinateSystem[index[i]], CS);
                }
            if (CS != null) CS.Dispose();
            return result;
        }
        /// <summary>
        /// returns a row containing given index list
        /// </summary>
        /// <param name="index">index list</param>
        /// <param name="X">spacing in X direction</param>
        /// <param name="Y">Y-coordinate</param>
        /// <returns>row of circles as list of circles</returns>
        public List<List<Circle>> GetCircleIndexAsRow(List<int> index, double X, double Y)
        {
            CoordinateSystem CS = null;
            List<List<Circle>> result = new List<List<Circle>>(index.Count);
            for (int i = 0; i < index.Count; i++) for (int j = 0; j < Circles[index[i]].Count; j++)
                {
                    CS = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(X * i, Y);
                    result[i][j] = (Circle) Circles[index[i]][j].Transform(CoordinateSystem[index[i]], CS);
                }
            if (CS != null) CS.Dispose();
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                Curves.ForEach(a=>a.ForEach(c=>c.Dispose()));
            }
            disposed = true;
        }
    }

    public class Sheet<T>: IDisposable
        where T: Curve
    {
        bool disposed = false;
        public List<List<T>> Curves { get; set; }
        public List<CoordinateSystem> CoordinateSystem { get; set; }


        internal Sheet(List<List<T>> Curves, List<CoordinateSystem> CoordinateSystem)
        {
            this.CoordinateSystem = CoordinateSystem;
            this.Curves = Curves;
        }
        
        //**CREATE
        /// <summary>
        /// creates a sheet layout of an array of Polycurves with designated coordinate system
        /// </summary>
        /// <param name="Curves">list of polycurves</param>
        /// <param name="CS">list of polycurves coordinate system</param>
        /// <returns>sheet object</returns>
        public static Sheet<T> ByCurvesAndCS(List<List<T>> Curves, List<CoordinateSystem> CS) { return new Sheet<T>(Curves, CS); }
        public static Sheet<PolyCurve> ByCurvesAndCS(List<List<Curve>> Curves, List<CoordinateSystem> CS)
        {
            List<List<PolyCurve>> result = new List<List<PolyCurve>>(Curves.Count);
            for (int i = 0; i < Curves.Count; i++) for (int j = 0; j < Curves[i].Count; j++)
                result[i][j] = PolyCurve.ByJoinedCurves( new List<Curve>{Curves[i][j]});
            return new Sheet<PolyCurve>(result, CS);
        }

        //**ACTIONS
        /// <summary>
        /// returns a row containing given index list
        /// </summary>
        /// <param name="index">index list</param>
        /// <param name="X">spacing in X direction</param>
        /// <param name="Y">Y-coordinate</param>
        /// <returns>row of polycurves as list of polycurves</returns>
        public List<List<T>> GetAsRows(List<int> index, double X, double Y)
        {
            CoordinateSystem CS = null;
            List<List<T>> result = new List<List<T>>(index.Count);
            for (int i = 0; i < index.Count; i++) for (int j = 0; j < Curves[index[i]].Count; j++)
                {
                    CS = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(X * i, Y);
                    result[i][j] = (T) Curves[index[i]][j].Transform(CoordinateSystem[index[i]], CS);
                }
            if (CS != null) CS.Dispose();
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                Curves.ForEach(a=>a.ForEach(c=>c.Dispose()));
                CoordinateSystem.ForEach(c => c.Dispose());
            }
            disposed = true;
        }
    }
}
