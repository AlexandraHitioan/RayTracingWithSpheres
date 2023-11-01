﻿using System;

namespace rt
{
    public class Sphere : Geometry
    {
        private Vector Center { get; set; }
        private double Radius { get; set; }

        public Sphere(Vector center, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
             //ADD CODE HERE: Calculate the intersection between the given line and this sphere
            var a = line.Dx * line.Dx;
            var b = (line.Dx * line.X0) * 2;
            b -= (line.Dx * Center) * 2;
            var c = (line.X0 * line.X0) + (Center * Center) - (Radius * Radius) - (line.X0 * Center) * 2;
            var discr = (b * b) - (a * c * 4.0);
            if (discr < 0.001)
                return new Intersection(false, false, this, line, 0);
            var t1 = -b - Math.Sqrt(discr);
            var t2 = -b + Math.Sqrt(discr);
            t1 /= 2.0 * a;
            t2 /= 2.0 * a;
            var validT1 = t1 >= minDist && t1 <= maxDist;
           var validT2 = t2 >= minDist && t2 <= maxDist;
            if (!validT1 && !validT2)
                return new Intersection(false, false, this, line, 0);
            if (validT1 && !validT2)
                return new Intersection(true, true, this, line, t1); 
            if (!validT1)
                return new Intersection(true, true, this, line, t2);
            var mn = t1;
            if (t2 < mn)
                mn = t2;
            return new Intersection(true, true, this, line, mn);
           // return new Intersection();
        }

        public override Vector Normal(Vector v)
        {
            var n = v - Center;
            n.Normalize();
            return n;
        }
    }
}