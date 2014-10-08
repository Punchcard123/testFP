﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ArdupilotMega;
using GMap.NET;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace MissionPlanner.Utilities
{

    public class PointLatLngAlt
    {
        public static readonly PointLatLngAlt Zero;
        public double Lat = 0;
        public double Lng = 0;
        public double Alt = 0;
        public string Tag = "";
        public Color color = Color.White;

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        static CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
        static GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

        public PointLatLngAlt(double lat, double lng, double alt, string tag)
        {
            this.Lat = lat;
            this.Lng = lng;
            this.Alt = alt;
            this.Tag = tag;
        }

        public PointLatLngAlt()
        {

        }

        public PointLatLngAlt(GMap.NET.PointLatLng pll)
        {
            this.Lat = pll.Lat;
            this.Lng = pll.Lng;
        }

        public PointLatLngAlt(double lat, double lng)
        {
            this.Lat = lat;
            this.Lng = lng;
        }

        public PointLatLngAlt(Locationwp locwp)
        {
            this.Lat = locwp.lat;
            this.Lng = locwp.lng;
            this.Alt = locwp.alt;
        }

        public PointLatLngAlt(PointLatLngAlt plla)
        {
            this.Lat = plla.Lat;
            this.Lng = plla.Lng;
            this.Alt = plla.Alt;
            this.color = plla.color;
            this.Tag = plla.Tag;
        }

        public PointLatLng Point()
        {
            return new PointLatLng(Lat, Lng);
        }

        public static implicit operator PointLatLngAlt(PointLatLng a)
        {
            return new PointLatLngAlt(a);
        }

        public static implicit operator PointLatLng(PointLatLngAlt a)
        {
            return a.Point();
        }

        public static implicit operator double[](PointLatLngAlt a)
        {
            return new double[] { a.Lng, a.Lat };
        }

        public static implicit operator PointLatLngAlt(double[] a)
        {
            return new PointLatLngAlt() { Lng =  a[0], Lat = a[1] };
        }

        public override bool Equals(Object pllaobj)
        {
            PointLatLngAlt plla = (PointLatLngAlt)pllaobj;

            if (plla == null)
                return false;

            if (this.Lat == plla.Lat &&
            this.Lng == plla.Lng &&
            this.Alt == plla.Alt &&
            this.color == plla.color &&
            this.Tag == plla.Tag)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)((Lat + (Lng * 100) + (Alt * 10000)) * 100);
        }

        public override string ToString()
        {
            return Lat + "," + Lng + "," + Alt;
        }

        public int GetUTMZone()
        {
            int zone = (int)((Lng - -183.0) / 6.0);
            if (Lat < 0)
                zone *= -1;

            return zone;
        }

        public double[] ToUTM(int utmzone)
        {
            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(Math.Abs(utmzone), Lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            double[] pll = { Lng, Lat };

            // get leader utm coords
            double[] utmxy = trans.MathTransform.Transform(pll);

            return utmxy;
        }

        public static List<double[]> ToUTM(int utmzone, List<PointLatLngAlt> list)
        {
            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(Math.Abs(utmzone), list[0].Lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            List<double[]> data = new List<double[]>();

            list.ForEach(x => { data.Add((double[])x); });

            return trans.MathTransform.TransformList(data);
        }

        /// <summary>
        /// Calc Distance in M
        /// </summary>
        /// <param name="p2"></param>
        /// <returns>Distance in M</returns>
        public double GetDistance(PointLatLngAlt p2)
        {
            double d = Lat * 0.017453292519943295;
            double num2 = Lng * 0.017453292519943295;
            double num3 = p2.Lat * 0.017453292519943295;
            double num4 = p2.Lng * 0.017453292519943295;
            double num5 = num4 - num2;
            double num6 = num3 - d;
            double num7 = Math.Pow(Math.Sin(num6 / 2.0), 2.0) + ((Math.Cos(d) * Math.Cos(num3)) * Math.Pow(Math.Sin(num5 / 2.0), 2.0));
            double num8 = 2.0 * Math.Atan2(Math.Sqrt(num7), Math.Sqrt(1.0 - num7));
            return (6371 * num8) * 1000.0; // M
        }

        public double GetDistance2(PointLatLngAlt p2)
        {
            //http://www.movable-type.co.uk/scripts/latlong.html
            var R = 6371.0; // 6371 km
            var dLat = (p2.Lat - Lat) * deg2rad;
            var dLon = (p2.Lng - Lng) * deg2rad;
            var lat1 = Lat * deg2rad;
            var lat2 = p2.Lat * deg2rad;

            var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                    Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            var d = R * c * 1000.0; // M

            return d;
        }
    }

}
