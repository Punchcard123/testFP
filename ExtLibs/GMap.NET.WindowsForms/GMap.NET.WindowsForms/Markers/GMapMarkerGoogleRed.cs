﻿
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;

#if !PocketPC
    using GMap.NET.Properties;
#else
   using GMap.NET.WindowsMobile.Properties;
#endif

   public class GMapMarkerGoogleRed : GMapMarker
   {
      public float? Bearing;

      static readonly System.Drawing.Size SizeSt = new System.Drawing.Size(Resources.marker.Width, Resources.marker.Height);

      public GMapMarkerGoogleRed(PointLatLng p)
         : base(p)
      {
         Size = SizeSt;
         Offset = new Point(-10, -34);
      }

      static readonly Point[] Arrow = new Point[] { new Point(-7, 7), new Point(0, -22), new Point(7, 7), new Point(0, 2) };

      public override void OnRender(Graphics g)
      {
#if !PocketPC
         if(!Bearing.HasValue)
         {
            g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X, LocalPosition.Y);
         }
         g.TranslateTransform(ToolTipPosition.X, ToolTipPosition.Y);

         if(Bearing.HasValue)
         {
            g.RotateTransform(Bearing.Value - Overlay.Control.Bearing);
            g.FillPolygon(Brushes.Red, Arrow);
         }

         g.ResetTransform();

         if(!Bearing.HasValue)
         {
            g.DrawImageUnscaled(Resources.marker, LocalPosition.X, LocalPosition.Y);
         }
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X, LocalPosition.Y);
            DrawImageUnscaled(g, Resources.marker, LocalPosition.X, LocalPosition.Y);
#endif
      }
   }
}
