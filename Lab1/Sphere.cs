using System;

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
            var b = (line.Dx * line.X0) * 2 -(line.Dx * Center) * 2;
            var c = (line.X0 * line.X0) + (Center * Center) - (Radius * Radius) - (line.X0 * Center) * 2;
            var delta = (b * b) - (a * c * 4.0); //the discriminant that we need to determine the solutions
            if (delta < 0.001) //check wether there are no real solutions -> we have no intersection
                return new Intersection(false, false, this, line, 0);
            var sol1 = (-b - Math.Sqrt(delta))/2.0*a;
            var sol2 = (-b + Math.Sqrt(delta))/2.0*a; //calculated the two solutions for the discriminant
            var validT1 = sol1 >= minDist && sol1 <= maxDist;
            var validT2 = sol2 >= minDist && sol2 <= maxDist;
            if (!(sol1 >= minDist && sol1 <= maxDist) && !(sol2 >= minDist && sol2 <= maxDist)) //no solution is valid->no intersection point
            {
                return new Intersection(false, false, this, line, 0);
            }

            if ((sol1 >= minDist && sol1 <= maxDist) && !(sol2 >= minDist && sol2 <= maxDist)) //sol1 is valid
            {
                return new Intersection(true, true, this, line, sol1); 
            }

            if (!(sol1 >= minDist && sol1 <= maxDist) && (sol2 >= minDist && sol2 <= maxDist)) //sol2 is valid
            {
                return new Intersection(true, true, this, line, sol2);
            }

            if (sol1 < sol2) //if both points are valid, we choose the closest one
            {
                return new Intersection(true, true, this, line, sol1); 
            }
            return new Intersection(true, true, this, line, sol2);
            
        }
        

        public override Vector Normal(Vector v)
        {
            var n = v - Center;
            n.Normalize();
            return n;
        }
    }
}