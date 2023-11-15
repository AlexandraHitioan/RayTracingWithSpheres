using System;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            var u = n * viewPlaneSize / imgSize;
            u -= viewPlaneSize / 2;
            return u;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = new Intersection();

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            // ADD CODE HERE: Detect whether the given point has a clear line of sight to the
            var ray = new Line(light.Position, point); //a ray pointing from our light source towards our point
            var inter = FindFirstIntersection(ray, 0, 100000); //first inter between the ray and any object in a range of [0,100000] surface
            if (inter.Valid == false)
            {
                return true; //we have a clear line of sight
            }
            if (inter.Visible == false)
            {
                return true; //the object is either transparent or doesn't exist
            }

            if (inter.T > (light.Position - point).Length() - 0.001) // we check wether the dist between the source of light and the inter point is greater than the one between the light and the given point
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
                var background = new Color();
                var viewParallel = (camera.Up ^ camera.Direction).Normalize();
                var image = new Image(width, height);

            var vecW = camera.Direction * camera.ViewPlaneDistance;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // ADD CODE HERE: Implement pixel color calculation
                    image.SetPixel(i, j, background);
                    var pixelAsPoint = camera.Position + camera.Direction * camera.ViewPlaneDistance + 
                                           (camera.Up ^ camera.Direction) * ImageToViewPlane(i, width, camera.ViewPlaneWidth) + 
                                           camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight); //the ray starting from the camera position towards the point we just calculated
                    var lightRay = new Line(camera.Position, pixelAsPoint);//the ray starting from the camera position towards the point we just calculated
                    var inter = FindFirstIntersection(lightRay, camera.FrontPlaneDistance, camera.BackPlaneDistance); // determines the first intersection point between the ray and other potential objects
                    if (inter.Valid==true && inter.Visible==true)
                    {
                        var pixelColor = new Color(); //color of current pixel
                        foreach (var light in lights)
                        {
                            var lightColor = new Color(); //the shade of the lights that affects the color of the current pixel
                            lightColor += inter.Geometry.Material.Ambient * light.Ambient; //adding the ambient light
                            if (IsLit(inter.Position, light)) //checking wether the point is directly hit by the light
                            {
                                var interPoint = inter.Position; // the position of the intersection point
                                var vCamInter = (camera.Position - interPoint).Normalize(); // normal vector pointing from camera to intersection point
                                var dirLightInter = ((Sphere) inter.Geometry).Normal(inter.Position);// surface normal vector pointing from light source to inter point
                                var vLightInter = (light.Position - interPoint).Normalize(); // normal vector pointing from light source to inter point
                                var reflLight = (dirLightInter * (dirLightInter * vLightInter) * 2 - vLightInter).Normalize(); // unit vector of reflected light
                                
                                if (dirLightInter * vLightInter > 0)//we check wether the light is hitting the object
                                    lightColor += inter.Geometry.Material.Diffuse * light.Diffuse * (dirLightInter * vLightInter);
                                
                                if (vCamInter * reflLight > 0)//we check if the reflection of the light is visible
                                    lightColor += inter.Geometry.Material.Specular * light.Specular *
                                                      Math.Pow(vCamInter * reflLight, inter.Geometry.Material.Shininess);
                                lightColor *= light.Intensity; //amplyifying the color by intensity
                            }

                            pixelColor += lightColor; //we add the colors from the light sources to the overall color of the pixel
                        }
                        image.SetPixel(i, j, pixelColor);
                    }
                    else image.SetPixel(i, j, background); //if there is no intersection with an object, the pixel will just get the bg color
                }
                
            }
            image.Store(filename);
        }
        
    }
}