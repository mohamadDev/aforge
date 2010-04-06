﻿// AForge Math Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2007-2010
// andrew.kirillov@aforgenet.com
//

namespace AForge.Math.Geometry
{
    using System;
    using System.Collections.Generic;
    using AForge;

    /// <summary>
    /// A class for checking simple geometrical shapes.
    /// </summary>
    /// 
    /// <remarks><para>The class performs checking/detection of some simple geometrical
    /// shapes for provided set of points (shape's edge points). During the check
    /// the class goes through the list of all provided points and checks how accurately
    /// they fit into assumed shape.</para>
    /// 
    /// <para>All the shape checks allow some deviation of
    /// points from the shape with assumed parameters. In other words it is allowed
    /// that specified set of points may form a little bit distorted shape, which may be
    /// still recognized. The allowed amount of distortion is controlled by two
    /// properties (<see cref="MinAcceptableDistortion"/> and <see cref="RelativeDistortionLimit"/>),
    /// which allow higher distortion level for bigger shapes and smaller amount of
    /// distortion for smaller shapes. Checking specified set of points, the class
    /// calculates mean distance between specified set of points and edge of the assumed
    /// shape. If the mean distance is equal to or less than maximum allowed distance,
    /// then a shape is recognized. The maximum allowed distance is calculated as:
    /// <code lang="none">
    /// maxDitance = max( minAcceptableDistortion, relativeDistortionLimit * ( width * height ) / 2 )
    /// </code>
    /// , where <b>width</b> and <b>height</b> is the size of bounding rectangle for the
    /// specified points.
    /// </para>
    /// 
    /// </remarks>
    /// 
    public class SimpleShapeChecker
    {
        private FlatAnglesOptimizer shapeOptimizer = new FlatAnglesOptimizer( 160 );

        private float minAcceptableDistortion = 0.5f;
        private float relativeDistortionLimit = 0.03f;

        /// <summary>
        /// Minimum value of allowed shapes' distortion.
        /// </summary>
        /// 
        /// <remarks><para>The property sets minimum value for allowed shapes'
        /// distortion. See documentation to <see cref="SimpleShapeChecker"/>
        /// class for more details about this property.</para>
        /// 
        /// <para>Default value is set to <b>0.5</b>.</para>
        /// </remarks>
        /// 
        public float MinAcceptableDistortion
        {
            get { return minAcceptableDistortion; }
            set { minAcceptableDistortion = Math.Max( 0, value ); }
        }

        /// <summary>
        /// Maximum value of allowed shapes' distortion, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>The property sets maximum value for allowed shapes'
        /// distortion. The value is measured in [0, 1] range, which corresponds
        /// to [0%, 100%] range, which means that maximum allowed shapes'
        /// distortion is calculated relatively to shape's size. This results to
        /// higher allowed distortion level for bigger shapes and smaller allowed
        /// distortion for smaller shapers. See documentation to <see cref="SimpleShapeChecker"/>
        /// class for more details about this property.</para>
        /// 
        /// <para>Default value is set to <b>0.03</b> (3%).</para>
        /// </remarks>
        /// 
        public float RelativeDistortionLimit
        {
            get { return relativeDistortionLimit; }
            set { relativeDistortionLimit = Math.Max( 0, Math.Min( 1, value ) ); }
        }

        /// <summary>
        /// Check type of the shape formed by specified points.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// 
        /// <returns>Returns type of the detected shape.</returns>
        /// 
        public ShapeType CheckShapeType( List<IntPoint> edgePoints )
        {
            if ( IsCircle( edgePoints ) )
            {
                return ShapeType.Circle;
            }

            // check for convex polygon
            List<IntPoint> corners;

            if ( IsConvexPolygon( edgePoints, out corners ) )
            {
                return ( corners.Count == 4 ) ? ShapeType.Quadrilateral : ShapeType.Triangle;
            }

            return ShapeType.Unknown;
        }

        /// <summary>
        /// Check if the specified set of points form a circle shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// circle shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsCircle( List<IntPoint> edgePoints )
        {
            DoublePoint center;
            float radius;

            return IsCircle( edgePoints, out center, out radius );
        }

        /// <summary>
        /// Check if the specified set of points form a circle shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="center">Receives circle's center on successful return.</param>
        /// <param name="radius">Receives circle's radius on successful return.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// circle shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsCircle( List<IntPoint> edgePoints, out DoublePoint center, out float radius )
        {
            // get bounding rectangle of the points list
            IntPoint minXY, maxXY;
            PointsCloud.GetBoundingRectangle( edgePoints, out minXY, out maxXY );
            // get cloud's size
            IntPoint cloudSize = maxXY - minXY ;
            // calculate center point
            center = minXY + (DoublePoint) cloudSize / 2;

            radius = ( (float) cloudSize.X + cloudSize.Y ) / 4;

            // calculate radius as mean distance between edge points and center
            float[] distances = new float[edgePoints.Count];
            float meanDistance = 0;

            for ( int i = 0, n = edgePoints.Count; i < n; i++ )
            {
                distances[i] = Math.Abs( (float) center.DistanceTo( edgePoints[i] ) - radius );
                meanDistance += distances[i];
            }
            meanDistance /= distances.Length;

            float maxDitance = Math.Max( minAcceptableDistortion,
                ( (float) cloudSize.X + cloudSize.Y ) / 2 * relativeDistortionLimit );

            return ( meanDistance <= maxDitance );
        }

        /// <summary>
        /// Check if the specified set of points form a quadrilateral shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// quadrilateral shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsQuadrilateral( List<IntPoint> edgePoints )
        {
            List<IntPoint> corners;
            return IsQuadrilateral( edgePoints, out corners );
        }

        /// <summary>
        /// Check if the specified set of points form a quadrilateral shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of quadrilateral corners on successful return.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// quadrilateral shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsQuadrilateral( List<IntPoint> edgePoints, out List<IntPoint> corners )
        {
            corners = GetShapeCorners( edgePoints );

            if ( corners.Count != 4 )
                return false;

            return CheckIfPointsFitShape( edgePoints, corners );
        }

        /// <summary>
        /// Check if the specified set of points form a triangle shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// triangle shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsTriangle( List<IntPoint> edgePoints )
        {
            List<IntPoint> corners;
            return IsTriangle( edgePoints, out corners );
        }

        /// <summary>
        /// Check if the specified set of points form a triangle shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of triangle corners on successful return.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// triangle shape or <see langword="false"/> otherwise.</returns>
        /// 
        public bool IsTriangle( List<IntPoint> edgePoints, out List<IntPoint> corners )
        {
            corners = GetShapeCorners( edgePoints );

            if ( corners.Count != 3 )
                return false;

            return CheckIfPointsFitShape( edgePoints, corners );
        }

        /// <summary>
        /// Check if the specified set of points form a convex polygon shape.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of polygon corners on successful return.</param>
        /// 
        /// <returns>Returns <see langword="true"/> if the specified set of points form a
        /// convex polygon shape or <see langword="false"/> otherwise.</returns>
        /// 
        /// <remarks><para><note>The method is able to detect only triangles and quadrilaterals
        /// for now. Check number of detected corners to resolve type of the detected polygon.
        /// </note></para></remarks>
        /// 
        public bool IsConvexPolygon( List<IntPoint> edgePoints, out List<IntPoint> corners )
        {
            corners = GetShapeCorners( edgePoints );
            return CheckIfPointsFitShape( edgePoints, corners );
        }

        /// <summary>
        /// Check if a shape specified by the set of points fits a convex polygon
        /// specified by the set of corners.
        /// </summary>
        /// 
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">Corners of convex polygon to check fitting into.</param>
        ///
        /// <returns>Returns <see langword="true"/> if the specified shape fits
        /// the specified convex polygon or <see langword="false"/> otherwise.</returns>
        /// 
        /// <remarks><para>The method checks if the set of specified points form the same shape
        /// as the set of provided corners.</para></remarks>
        /// 
        public bool CheckIfPointsFitShape( List<IntPoint> edgePoints, List<IntPoint> corners )
        {
            int cornersCount = corners.Count;

            // lines coefficients (for representation as y(x)=k*x+b)
            double[] k = new double[cornersCount];
            double[] b = new double[cornersCount];
            double[] div = new double[cornersCount]; // precalculated divisor
            bool[] isVert = new bool[cornersCount];

            for ( int i = 0; i < cornersCount; i++ )
            {
                IntPoint currentPoint = corners[i];
                IntPoint nextPoint = ( i + 1 == cornersCount ) ? corners[0] : corners[i + 1];

                if ( !( isVert[i] = nextPoint.X == currentPoint.X ) )
                {
                    k[i] = (double) ( nextPoint.Y - currentPoint.Y ) / ( nextPoint.X - currentPoint.X );
                    b[i] = currentPoint.Y - k[i] * currentPoint.X;
                    div[i] = Math.Sqrt( k[i] * k[i] + 1 );
                }
            }

            // calculate distances between edge points and polygon sides
            float[] distances = new float[edgePoints.Count];
            float meanDistance = 0;

            for ( int i = 0, n = edgePoints.Count; i < n; i++ )
            {
                float minDistance = float.MaxValue;

                for ( int j = 0; j < cornersCount; j++ )
                {
                    float distance = 0;

                    if ( !isVert[j] )
                    {
                        distance = (float) Math.Abs( ( k[j] * edgePoints[i].X + b[j] - edgePoints[i].Y ) / div[j] );
                    }
                    else
                    {
                        distance = Math.Abs( edgePoints[i].X - corners[j].X );
                    }

                    if ( distance < minDistance )
                        minDistance = distance;
                }

                distances[i] = minDistance;
                meanDistance += minDistance;
            }
            meanDistance /= distances.Length;

            // get bounding rectangle of the corners list
            IntPoint minXY, maxXY;
            PointsCloud.GetBoundingRectangle( corners, out minXY, out maxXY );
            IntPoint rectSize = maxXY - minXY;

            float maxDitance = Math.Max( minAcceptableDistortion,
                ( (float) rectSize.X + rectSize.Y ) / 2 * relativeDistortionLimit );

            return ( meanDistance <= maxDitance );
        }

        // Get optimized quadrilateral area
        private List<IntPoint> GetShapeCorners( List<IntPoint> edgePoints )
        {
            return shapeOptimizer.OptimizeShape( PointsCloud.FindQuadrilateralCorners( edgePoints ) );
        }
    }
}
