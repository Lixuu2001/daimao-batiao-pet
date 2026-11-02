using System;
using System.Drawing;
namespace Batiao {
 public sealed class Animation {
  public static readonly int[] Counts = {6,8,8,4,5,8,6,6,6};
  public int Row { get; private set; }
  public int Frame { get; private set; }
  bool loop = true;
  public void Play(int row, bool repeat) {
   if(row<0 || row>=Counts.Length) throw new ArgumentOutOfRangeException("row");
   Row=row; Frame=0; loop=repeat;
  }
  public void Tick() {
   Frame++;
   if(Frame>=Counts[Row]) { if(loop) Frame=0; else Play(0,true); }
  }
 }
 public static class Interaction {
  public static bool IsDrag(Point start,Point now) { return Math.Abs(now.X-start.X)>5 || Math.Abs(now.Y-start.Y)>5; }
  public static Point Clamp(Point p,Size size,Rectangle area) {
   return new Point(Math.Max(area.Left,Math.Min(p.X,Math.Max(area.Left,area.Right-size.Width))),Math.Max(area.Top,Math.Min(p.Y,Math.Max(area.Top,area.Bottom-size.Height))));
  }
 }
}
