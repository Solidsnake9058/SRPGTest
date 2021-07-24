using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct HexCoord
{

    /// <summary>
    /// Position on the q axis.
    /// </summary>
    [SerializeField]
    public int m_Q;
    /// <summary>
    /// Position on the r axis.
    /// </summary>
    [SerializeField]
    public int m_R;

    /// <summary>
    /// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoord"/> struct.
    /// </summary>
    /// <param name="q">Position on the q axis.</param>
    /// <param name="r">Position on the r axis.</param>
    public HexCoord(int q, int r)
    {
        this.m_Q = q;
        this.m_R = r;
    }

    /// <summary>
    /// Position on the cubic z axis.
    /// </summary>
    /// <remarks>
    /// The q,r coordinate system is derived from an x,y,z cubic system with the constraint that x + y + z = 0.
    /// Where x = q and y = r, this property derives z as <c>-q-r</c>.
    /// </remarks>
    public int m_Z
    {
        get { return -m_Q - m_R; }
    }

    /// <summary>
    /// Offset x coordinate.
    /// </summary>
    /// <remarks>
    /// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
    /// Where y = r, this property represents the x coordinate as <c>q + r/2</c>.
    /// </remarks>
    public int m_Offset
    {
        get { return m_Q + (m_R >> 1); }
    }

    /// <summary>
    /// Unity position of this hex.
    /// </summary>
    public Vector2 Position()
    {
        return m_Q * Q_XY + m_R * R_XY;
    }

    /// <summary>
    /// Unity position of this hex for cube tiles. Form left upper.
    /// </summary>
    /// <returns></returns>
    public Vector3 PositionSqr()
    {
        return new Vector3((m_Q - m_Z) / 2.0f, 0, -m_R);
    }

    /// <summary>
    /// Get the maximum absolute cubic coordinate.
    /// </summary>
    /// <remarks>
    /// In hexagonal space this is the polar radius, i.e. distance from 0,0.
    /// </remarks>
    public int AxialLength()
    {
        if (m_Q == 0 && m_R == 0) return 0;
        if (m_Q > 0 && m_R >= 0) return m_Q + m_R;
        if (m_Q <= 0 && m_R > 0) return (-m_Q < m_R) ? m_R : -m_Q;
        if (m_Q < 0) return -m_Q - m_R;
        return (-m_R > m_Q) ? -m_R : m_Q;
    }

    /// <summary>
    /// Get the minimum absolute cubic coordinate.
    /// </summary>
    /// <remarks>
    /// This is the number of hexagon steps from 0,0 which are not along the maximum axis.
    /// </remarks>
    public int AxialSkew()
    {
        if (m_Q == 0 && m_R == 0) return 0;
        if (m_Q > 0 && m_R >= 0) return (m_Q < m_R) ? m_Q : m_R;
        if (m_Q <= 0 && m_R > 0) return (-m_Q < m_R) ? Math.Min(-m_Q, m_Q + m_R) : Math.Min(m_R, -m_Q - m_R);
        if (m_Q < 0) return (m_Q > m_R) ? -m_Q : -m_R;
        return (-m_R > m_Q) ? Math.Min(m_Q, -m_Q - m_R) : Math.Min(-m_R, m_Q + m_R);
    }

    /// <summary>
    /// Get the angle from 0,0 to the center of this hex.
    /// </summary>
    public float PolarAngle()
    {
        Vector3 pos = Position();
        return (float)Math.Atan2(pos.y, pos.x);
    }

    /// <summary>
    /// Get the counterclockwise position of this hex in the ring at its distance from 0,0.
    /// </summary>
    public int PolarIndex()
    {
        if (m_Q == 0 && m_R == 0) return 0;
        if (m_Q > 0 && m_R >= 0) return m_R;
        if (m_Q <= 0 && m_R > 0) return (-m_Q < m_R) ? m_R - m_Q : -3 * m_Q - m_R;
        if (m_Q < 0) return -4 * (m_Q + m_R) + m_Q;
        return (-m_R > m_Q) ? -4 * m_R + m_Q : 6 * m_Q + m_R;
    }

    /// <summary>
    /// Get a neighboring hex.
    /// </summary>
    /// <remarks>
    /// Neighbor 0 is to the right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="index">Index of the desired neighbor. Cyclically constrained 0..5.</param>
    public HexCoord Neighbor(int index)
    {
        return NeighborVector(index) + this;
    }

    public HexCoord PolarNeighbor(bool CCW = false)
    {
        if (m_Q > 0)
        {
            if (m_R < 0)
            {
                if (m_Q > -m_R) return this + m_AxialDirections[CCW ? 1 : 4];
                if (m_Q < -m_R) return this + m_AxialDirections[CCW ? 0 : 3];
                return this + m_AxialDirections[CCW ? 1 : 3];
            }
            if (m_R > 0) return this + m_AxialDirections[CCW ? 2 : 5];
            return this + m_AxialDirections[CCW ? 2 : 4];
        }
        if (m_Q < 0)
        {
            if (m_R > 0)
            {
                if (m_R > -m_Q) return this + m_AxialDirections[CCW ? 3 : 0];
                if (m_R < -m_Q) return this + m_AxialDirections[CCW ? 4 : 1];
                return this + m_AxialDirections[CCW ? 4 : 0];
            }
            if (m_R < 0) return this + m_AxialDirections[CCW ? 5 : 2];
            return this + m_AxialDirections[CCW ? 5 : 1];
        }
        if (m_R > 0) return this + m_AxialDirections[CCW ? 3 : 5];
        if (m_R < 0) return this + m_AxialDirections[CCW ? 0 : 2];
        return this;
    }

    /// <summary>
    /// Enumerate this hex's six neighbors.
    /// </summary>
    /// <remarks>
    /// Neighbor 0 is to the right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="first">Index of the first neighbor to enumerate.</param>
    public IEnumerable<HexCoord> Neighbors(int first = 0)
    {
        foreach (HexCoord hex in NeighborVectors(first))
            yield return hex + this;
    }

    /// <summary>
    /// Get the Unity position of a corner vertex.
    /// </summary>
    /// <remarks>
    /// Corner 0 is at the upper right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
    public Vector2 Corner(int index)
    {
        return CornerVector(index) + Position();
    }

    /// <summary>
    /// Enumerate this hex's six corners.
    /// </summary>
    /// <remarks>
    /// Corner 0 is at the upper right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="first">Index of the first corner to enumerate.</param>
    public IEnumerable<Vector2> Corners(int first = 0)
    {
        Vector2 pos = Position();
        foreach (Vector2 v in CornerVectors(first))
            yield return v + pos;
    }

    /// <summary>
    /// Get the polar angle to a corner vertex.
    /// </summary>
    /// <remarks>
    /// This is the angle in radians from the center of 0,0 to the selected corner of this hex.
    /// </remarks>
    /// <param name="index">Index of the desired corner.</param>
    public float CornerPolarAngle(int index)
    {
        Vector2 pos = Corner(index);
        return (float)Math.Atan2(pos.y, pos.x);
    }

    /// <summary>
    /// Get the polar angle to the clockwise bounding corner.
    /// </summary>
    /// <remarks>
    /// The two polar bounding corners are those whose polar angles form the widest arc.
    /// </remarks>
    /// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
    public float PolarBoundingAngle(bool CCW = false)
    {
        return CornerPolarAngle(PolarBoundingCornerIndex(CCW));
    }

    /// <summary>
    /// Get the XY position of the clockwise bounding corner.
    /// </summary>
    /// <remarks>
    /// The two polar bounding corners are those whose polar angles form the widest arc.
    /// </remarks>
    /// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
    public Vector2 PolarBoundingCorner(bool CCW = false)
    {
        return Corner(PolarBoundingCornerIndex(CCW));
    }

    /// <summary>
    /// Get the index of the clockwise bounding corner.
    /// </summary>
    /// <remarks>
    /// The two polar bounding corners are those whose polar angles form the widest arc.
    /// </remarks>
    /// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
    /// <param name="neighbor">If set to <c>true</c>, gets the other corner shared by the same ring-neighbor as normal return.</param>
    public int PolarBoundingCornerIndex(bool CCW = false)
    {
        if (m_Q == 0 && m_R == 0) return 0;
        if (m_Q > 0 && m_R >= 0) return CCW ?
            (m_Q > m_R) ? 1 : 2 :
            (m_Q < m_R) ? 5 : 4;
        if (m_Q <= 0 && m_R > 0) return (-m_Q < m_R) ?
            CCW ?
                (m_R > -2 * m_Q) ? 2 : 3 :
                (m_R < -2 * m_Q) ? 0 : 5 :
            CCW ?
                (m_Q > -2 * m_R) ? 3 : 4 :
                (m_Q < -2 * m_R) ? 1 : 0;
        if (m_Q < 0) return CCW ?
            (m_Q < m_R) ? 4 : 5 :
            (m_Q > m_R) ? 2 : 1;
        return (-m_R > m_Q) ?
            CCW ?
                (m_R < -2 * m_Q) ? 5 : 0 :
                (m_R > -2 * m_Q) ? 3 : 2 :
            CCW ?
                (m_Q < -2 * m_R) ? 0 : 1 :
                (m_Q > -2 * m_R) ? 4 : 3;
    }

    /// <summary>
    /// Get the half sextant of origin containing this hex.
    /// </summary>
    /// <remarks>
    /// CornerSextant is HalfSextant/2. NeighborSextant is (HalfSextant+1)/2.
    /// </remarks>
    public int HalfSextant()
    {
        if (m_Q > 0 && m_R >= 0 || m_Q == 0 && m_R == 0)
            return (m_Q > m_R) ? 0 : 1;
        if (m_Q <= 0 && m_R > 0)
            return (-m_Q < m_R) ?
                (m_R > -2 * m_Q) ? 2 : 3 :
                (m_Q > -2 * m_R) ? 4 : 5;
        if (m_Q < 0)
            return (m_Q < m_R) ? 6 : 7;
        return (-m_R > m_Q) ?
            (m_R < -2 * m_Q) ? 8 : 9 :
            (m_Q < -2 * m_R) ? 10 : 11;
    }

    /// <summary>
    /// Get the corner index of 0,0 closest to this hex's polar vector.
    /// </summary>
    public int CornerSextant()
    {
        if (m_Q > 0 && m_R >= 0 || m_Q == 0 && m_R == 0) return 0;
        if (m_Q <= 0 && m_R > 0) return (-m_Q < m_R) ? 1 : 2;
        if (m_Q < 0) return 3;
        return (-m_R > m_Q) ? 4 : 5;
    }

    /// <summary>
    /// Get the neighbor index of 0,0 through which this hex's polar vector passes.
    /// </summary>
    public int NeighborSextant()
    {
        if (m_Q == 0 && m_R == 0) return 0;
        if (m_Q > 0 && m_R >= 0) return (m_Q <= m_R) ? 1 : 0;
        if (m_Q <= 0 && m_R > 0) return (-m_Q <= m_R) ?
            (m_R <= -2 * m_Q) ? 2 : 1 :
            (m_Q <= -2 * m_R) ? 3 : 2;
        if (m_Q < 0) return (m_Q >= m_R) ? 4 : 3;
        return (-m_R > m_Q) ?
            (m_R >= -2 * m_Q) ? 5 : 4 :
            (m_Q >= -2 * m_R) ? 0 : 5;
    }

    /// <summary>
    /// Rotate around 0,0 in sextant increments.
    /// </summary>
    /// <returns>
    /// A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after rotation.
    /// </returns>
    /// <param name="sextants">How many sextants to rotate by.</param>
    public HexCoord SextantRotation(int sextants)
    {
        if (this == origin) return this;
        sextants = NormalizeRotationIndex(sextants, 6);
        if (sextants == 0) return this;
        if (sextants == 1) return new HexCoord(-m_R, -m_Z);
        if (sextants == 2) return new HexCoord(m_Z, m_Q);
        if (sextants == 3) return new HexCoord(-m_Q, -m_R);
        if (sextants == 4) return new HexCoord(m_R, m_Z);
        return new HexCoord(-m_Z, -m_Q);
    }

    /// <summary>
    /// Mirror across a cubic axis.
    /// </summary>
    /// <remarks>
    /// The cubic axes are "diagonal" to the hexagons, passing through two opposite corners.
    /// </remarks>
    /// <param name="axis">A corner index through which the axis passes.</param>
    /// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after mirroring.</returns>
    public HexCoord Mirror(int axis = 1)
    {
        if (this == origin) return this;
        axis = NormalizeRotationIndex(axis, 3);
        if (axis == 0) return new HexCoord(m_R, m_Q);
        if (axis == 1) return new HexCoord(m_Z, m_R);
        return new HexCoord(m_Q, m_Z);
    }

    /// <summary>
    /// Scale as a vector, truncating result.
    /// </summary>
    /// <returns>This <see cref="Settworks.Hexagons.HexCoord"/> after scaling.</returns>
    public HexCoord Scale(float factor)
    {
        m_Q = (int)(m_Q * factor);
        m_R = (int)(m_R * factor);
        return this;
    }
    /// <summary>
    /// Scale as a vector.
    /// </summary>
    /// <returns>This <see cref="Settworks.Hexagons.HexCoord"/> after scaling.</returns>
    public HexCoord Scale(int factor)
    {
        m_Q *= factor;
        m_R *= factor;
        return this;
    }
    /// <summary>
    /// Scale as a vector.
    /// </summary>
    /// <returns><see cref="UnityEngine.Vector2"/> representing the scaled vector.</returns>
    public Vector2 ScaleToVector(float factor)
    { return new Vector2(m_Q * factor, m_R * factor); }

    /// <summary>
    /// Determines whether this hex is within a specified rectangle.
    /// </summary>
    /// <returns><c>true</c> if this instance is within the specified rectangle; otherwise, <c>false</c>.</returns>
    public bool IsWithinRectangle(HexCoord cornerA, HexCoord cornerB)
    {
        if (m_R > cornerA.m_R && m_R > cornerB.m_R || m_R < cornerA.m_R && m_R < cornerB.m_R)
            return false;
        bool reverse = cornerA.m_Offset > cornerB.m_Offset;   // Travel right to left.
        bool offset = cornerA.m_R % 2 != 0;   // Starts on an odd row, bump alternate rows left.
        bool trim = Math.Abs(cornerA.m_R - cornerB.m_R) % 2 == 0;   // Even height, trim alternate rows.
        bool odd = (m_R - cornerA.m_R) % 2 != 0; // This is an alternate row.
        int width = Math.Abs(cornerA.m_Offset - cornerB.m_Offset);
        bool hasWidth = width != 0;
        if (reverse && (odd && (trim || !offset) || !(trim || offset || odd))
            || !reverse && (trim && odd || offset && !trim && hasWidth))
            width -= 1;
        int x = (m_Offset - cornerA.m_Offset) * (reverse ? -1 : 1);
        if (reverse && odd && !offset
            || !reverse && offset && odd && hasWidth)
            x -= 1;
        return (x <= width && x >= 0);
    }

    /// <summary>
    /// Determines whether this hex is on the infinite line passing through points a and b.
    /// </summary>
    public bool IsOnCartesianLine(Vector2 a, Vector2 b)
    {
        Vector2 AB = b - a;
        bool bias = Vector3.Cross(AB, Corner(0) - a).z > 0;
        for (int i = 1; i < 6; i++)
        {
            if (bias != (Vector3.Cross(AB, Corner(i) - a).z > 0))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether this the is on the line segment between points a and b.
    /// </summary>
    public bool IsOnCartesianLineSegment(Vector2 a, Vector2 b)
    {
        Vector2 AB = b - a;
        float mag = AB.sqrMagnitude;
        Vector2 AC = Corner(0) - a;
        bool within = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
        int sign = Math.Sign(Vector3.Cross(AB, AC).z);
        for (int i = 1; i < 6; i++)
        {
            AC = Corner(i) - a;
            bool newWithin = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
            int newSign = Math.Sign(Vector3.Cross(AB, AC).z);
            if ((within || newWithin) && (sign * newSign <= 0))
                return true;
            within = newWithin;
            sign = newSign;
        }
        return false;
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="Settworks.Hexagons.HexCoord"/>.
    /// </summary>
    /// <remarks>
    /// Matches the formatting of <see cref="UnityEngine.Vector2.ToString()"/>.
    /// </remarks>
    public override string ToString()
    {
        return "(" + m_Q + "," + m_R + ")";
    }

    /*
     * Static Methods
     */

    /// <summary>
    /// HexCoord at (0,0)
    /// </summary>
    public static readonly HexCoord origin = default(HexCoord);

    /// <summary>
    /// Distance between two hexes.
    /// </summary>
    public static int Distance(HexCoord a, HexCoord b)
    {
        return (a - b).AxialLength();
    }

    public static int HexDistance(HexCoord a, HexCoord b)
    {
        return (Mathf.Abs(a.m_Q - b.m_Q)
              + Mathf.Abs(a.m_Q + a.m_R - b.m_Q - b.m_R)
              + Mathf.Abs(a.m_R - b.m_R)) / 2;
    }

    /// <summary>
    /// Normalize a rotation index within 0 <= index < cycle.
    /// </summary>
    public static int NormalizeRotationIndex(int index, int cycle = 6)
    {
        if (index < 0 ^ cycle < 0)
            return (index % cycle + cycle) % cycle;
        else
            return index % cycle;
    }

    /// <summary>
    /// Determine the equality of two rotation indices for a given cycle.
    /// </summary>
    public static bool IsSameRotationIndex(int a, int b, int cycle = 6)
    {
        return 0 == NormalizeRotationIndex(a - b, cycle);
    }

    /// <summary>
    /// Vector from a hex to a neighbor.
    /// </summary>
    /// <remarks>
    /// Neighbor 0 is to the right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="index">Index of the desired neighbor vector. Cyclically constrained 0..5.</param>
    public static HexCoord NeighborVector(int index)
    { return m_AxialDirections[NormalizeRotationIndex(index, 6)]; }

    /// <summary>
    /// Enumerate the six neighbor vectors.
    /// </summary>
    /// <remarks>
    /// Neighbor 0 is to the right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="first">Index of the first neighbor vector to enumerate.</param>
    public static IEnumerable<HexCoord> NeighborVectors(int first = 0)
    {
        first = NormalizeRotationIndex(first, 6);
        for (int i = first; i < 6; i++)
            yield return m_AxialDirections[i];
        for (int i = 0; i < first; i++)
            yield return m_AxialDirections[i];
    }

    /// <summary>
    /// Neighbor index of 0,0 through which a polar angle passes.
    /// </summary>
    public static int AngleToNeighborIndex(float angle)
    { return Mathf.RoundToInt(angle / SEXTANT); }

    /// <summary>
    /// Polar angle for a neighbor of 0,0.
    /// </summary>
    public static float NeighborIndexToAngle(int index)
    { return index * SEXTANT; }

    /// <summary>
    /// Unity position vector from hex center to a corner.
    /// </summary>
    /// <remarks>
    /// Corner 0 is at the upper right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
    public static Vector2 CornerVector(int index)
    {
        return corners[NormalizeRotationIndex(index, 6)];
    }

    /// <summary>
    /// Enumerate the six corner vectors.
    /// </summary>
    /// <remarks>
    /// Corner 0 is at the upper right, others proceed counterclockwise.
    /// </remarks>
    /// <param name="first">Index of the first corner vector to enumerate.</param>
    public static IEnumerable<Vector2> CornerVectors(int first = 0)
    {
        if (first == 0)
        {
            foreach (Vector2 v in corners)
                yield return v;
        }
        else
        {
            first = NormalizeRotationIndex(first, 6);
            for (int i = first; i < 6; i++)
                yield return corners[i];
            for (int i = 0; i < first; i++)
                yield return corners[i];
        }
    }

    /// <summary>
    /// Corner of 0,0 closest to a polar angle.
    /// </summary>
    public static int AngleToCornerIndex(float angle)
    { return Mathf.FloorToInt(angle / SEXTANT); }

    /// <summary>
    /// Polar angle for a corner of 0,0.
    /// </summary>
    public static float CornerIndexToAngle(int index)
    { return (index + 0.5f) * SEXTANT; }

    /// <summary>
    /// Half sextant of 0,0 through which a polar angle passes.
    /// </summary>
    public static int AngleToHalfSextant(float angle)
    { return Mathf.RoundToInt(2 * angle / SEXTANT); }

    /// <summary>
    /// Polar angle at which a half sextant begins.
    /// </summary>
    public static float HalfSextantToAngle(int index)
    { return index * SEXTANT / 2; }


    /// <summary>
    /// <see cref="Settworks.Hexagons.HexCoord"/> containing a Unity position.
    /// </summary>
    public static HexCoord AtPosition(Vector2 position)
    { return FromQRVector(VectorXYtoQR(position)); }

    /// <summary>
    /// <see cref="Settworks.Hexagons.HexCoord"/> from hexagonal polar coordinates.
    /// </summary>
    /// <remarks>
    /// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
    /// </remarks>
    /// <param name="radius">Hex distance from 0,0.</param>
    /// <param name="index">Counterclockwise index.</param>
    public static HexCoord AtPolar(int radius, int index)
    {
        if (radius == 0) return origin;
        if (radius < 0) radius = -radius;
        index = NormalizeRotationIndex(index, radius * 6);
        int sextant = index / radius;
        index %= radius;
        if (sextant == 0) return new HexCoord(radius - index, index);
        if (sextant == 1) return new HexCoord(-index, radius);
        if (sextant == 2) return new HexCoord(-radius, radius - index);
        if (sextant == 3) return new HexCoord(index - radius, -index);
        if (sextant == 4) return new HexCoord(index, -radius);
        return new HexCoord(radius, index - radius);
    }

    /// <summary>
    /// Find the hexagonal polar index closest to angle at radius.
    /// </summary>
    /// <remarks>
    /// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
    /// </remarks>
    /// <param name="radius">Hex distance from 0,0.</param>
    /// <param name="angle">Desired polar angle.</param>
    public static int FindPolarIndex(int radius, float angle)
    {
        return (int)Math.Round(angle * radius * 3 / Mathf.PI);
    }

    /// <summary>
    /// <see cref="Settworks.Hexagons.HexCoord"/> from offset coordinates.
    /// </summary>
    /// <remarks>
    /// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
    /// This conversion assumes an offset of x = q + r/2.
    /// </remarks>
    public static HexCoord AtOffset(int x, int y)
    {
        return new HexCoord(x - (y >> 1), y);
    }

    /// <summary>
    /// <see cref="Settworks.Hexagons.HexCoord"/> from offset coordinates.
    /// </summary>
    /// <remarks>
    /// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
    /// This conversion assumes an offset of x = q + r/2.
    /// </remarks>
    public static HexCoord WithoutOffset(int x, int y)
    {
        return new HexCoord(x + (y >> 1), y);
    }

    public static HexCoord WithoutOffset(HexCoord hex)
    {
        return new HexCoord(hex.m_Q + hex.m_Offset, hex.m_R);
    }

    /// <summary>
    /// <see cref="Settworks.Hexagons.HexCoord"/> containing a floating-point q,r vector.
    /// </summary>
    /// <remarks>
    /// Hexagonal geometry makes normal rounding inaccurate. If working with floating-point
    /// q,r vectors, use this method to accurately convert them back to
    /// <see cref="Settworks.Hexagons.HexCoord"/>.
    /// </remarks>
    public static HexCoord FromQRVector(Vector2 QRvector)
    {
        float z = -QRvector.x - QRvector.y;
        int ix = (int)Math.Round(QRvector.x);
        int iy = (int)Math.Round(QRvector.y);
        int iz = (int)Math.Round(z);
        if (ix + iy + iz != 0)
        {
            float dx = Math.Abs(ix - QRvector.x);
            float dy = Math.Abs(iy - QRvector.y);
            float dz = Math.Abs(iz - z);
            if (dx >= dy && dx >= dz)
                ix = -iy - iz;
            else if (dy >= dz)
                iy = -ix - iz;
        }
        return new HexCoord(ix, iy);
    }

    /// <summary>
    /// Convert an x,y vector to a q,r vector.
    /// </summary>
    public static Vector2 VectorXYtoQR(Vector2 XYvector)
    {
        return XYvector.x * X_QR + XYvector.y * Y_QR;
    }

    /// <summary>
    /// Convert a q,r vector to an x,y vector.
    /// </summary>
    public static Vector2 VectorQRtoXY(Vector2 QRvector)
    {
        return QRvector.x * Q_XY + QRvector.y * R_XY;
    }

    /// <summary>
    /// Get the corners of a QR-space rectangle containing every cell touching an XY-space rectangle.
    /// </summary>
    public static HexCoord[] CartesianRectangleBounds(Vector2 cornerA, Vector2 cornerB)
    {
        Vector2 min = new Vector2(Math.Min(cornerA.x, cornerB.x), Math.Min(cornerA.y, cornerB.y));
        Vector2 max = new Vector2(Math.Max(cornerA.x, cornerB.x), Math.Max(cornerA.y, cornerB.y));
        HexCoord[] results = {
                HexCoord.AtPosition(min),
                HexCoord.AtPosition(max)
            };
        Vector2 pos = results[0].Position();
        if (pos.y - 0.5f >= min.y)
            results[0] += m_AxialDirections[4];
        else if (pos.x >= min.x)
            results[0] += m_AxialDirections[3];
        pos = results[1].Position();
        if (pos.y + 0.5f <= max.y)
            results[1] += m_AxialDirections[1];
        else if (pos.x <= max.x)
            results[1] += m_AxialDirections[0];
        return results;
    }

    /*
     * Operators
     */

    // Cast to Vector2 in QR space. Explicit to avoid QR/XY mix-ups.
    public static explicit operator Vector2(HexCoord h)
    { return new Vector2(h.m_Q, h.m_R); }
    // +, -, ==, !=
    public static HexCoord operator +(HexCoord a, HexCoord b)
    { return new HexCoord(a.m_Q + b.m_Q, a.m_R + b.m_R); }
    public static HexCoord operator -(HexCoord a, HexCoord b)
    { return new HexCoord(a.m_Q - b.m_Q, a.m_R - b.m_R); }
    public static HexCoord operator *(HexCoord a, int b)
    { return new HexCoord(a.m_Q * b, a.m_R * b); }
    public static bool operator ==(HexCoord a, HexCoord b)
    { return a.m_Q == b.m_Q && a.m_R == b.m_R; }
    public static bool operator !=(HexCoord a, HexCoord b)
    { return a.m_Q != b.m_Q || a.m_R != b.m_R; }
    // Mandatory overrides: Equals(), GetHashCode()
    public override bool Equals(object o)
    { return (o is HexCoord) && this == (HexCoord)o; }
    public override int GetHashCode()
    {
        return m_Q & (int)0xFFFF | m_R << 16;
    }

    public static HexCoord HexDirection(int direction)
    {
        return m_AxialDirections[direction];
    }

    public static HexCoord HexNeighbor(HexCoord hex, int direction)
    {
        var dir = HexDirection(direction);
        return hex + dir;
    }

    public HexCoord HexNeighbor(int direction)
    {
        var dir = HexDirection(direction);
        return this + dir;
    }

    public static List<HexCoord> HexRing(HexCoord center, int radius)
    {
        List<HexCoord> results = new List<HexCoord>();
        if (radius <= 0)
        {
            return null;
        }
        var cube = center + HexDirection(4) * radius;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                results.Add(cube);
                cube = HexNeighbor(cube, i);
            }
        }
        return results;
    }

    public static List<HexCoord> HexSpiral(HexCoord center, int radius)
    {
        List<HexCoord> results = new List<HexCoord>();
        for (int i = 1; i <= radius; i++)
        {
            results.AddRange(HexRing(center, i));
        }
        return results;
    }

    /*
     * Constants
     */

    /// <summary>
    /// One sixth of a full rotation (radians).
    /// </summary>
    public static readonly float SEXTANT = Mathf.PI / 3;

    /// <summary>
    /// Square root of 3.
    /// </summary>
    public static readonly float SQRT3 = Mathf.Sqrt(3);

    // The directions array. These are private to prevent overwriting elements.
    static readonly HexCoord[] m_AxialDirections = {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1),
        };

    // Corner locations in XY space. Private for same reason as neighbors.
    static readonly Vector2[] corners = {
            new Vector2(Mathf.Sin(SEXTANT), Mathf.Cos(SEXTANT)),
            new Vector2(0, 1),
            new Vector2(Mathf.Sin(-SEXTANT), Mathf.Cos(-SEXTANT)),
            new Vector2(Mathf.Sin(Mathf.PI + SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT)),
            new Vector2(0, -1),
            new Vector2(Mathf.Sin(Mathf.PI - SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT))
        };

    // Vector transformations between QR and XY space.
    // Private to keep IntelliSense tidy. Safe to make public, but sensible uses are covered above.
    static readonly Vector2 Q_XY = new Vector2(SQRT3, 0);
    static readonly Vector2 R_XY = new Vector2(SQRT3 / 2, 1.5f);
    static readonly Vector2 X_QR = new Vector2(SQRT3 / 3, 0);
    static readonly Vector2 Y_QR = new Vector2(-1 / 3f, 2 / 3f);



}
