using System;
using System.Drawing;
using Batiao;
class Tests {
 static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); }
 static void Main() {
  var a = new Animation();
  for (int row=0; row<9; row++) {
   a.Play(row, false);
   for(int i=0;i<Animation.Counts[row];i++) { Check(a.Row==row && a.Frame==i, "frame " + row + ":" + i); a.Tick(); }
   Check(a.Row==0 && a.Frame==0, "return idle " + row);
  }
  a.Play(2,true); for(int i=0;i<25;i++)a.Tick(); Check(a.Row==2 && a.Frame==1,"loop");
  Check(!Interaction.IsDrag(new Point(10,10),new Point(13,13)),"small jitter is click");
  Check(Interaction.IsDrag(new Point(10,10),new Point(18,10)),"movement is drag");
  var area=new Rectangle(-1920,0,1920,1080);
  Check(Interaction.Clamp(new Point(-2200,1200),new Size(192,208),area)==new Point(-1920,872),"negative monitor clamp");
  Check(Interaction.Clamp(new Point(50,50),new Size(500,500),new Rectangle(0,0,100,100))==Point.Empty,"small screen");
  try { a.Play(9,false); throw new Exception("invalid row accepted"); } catch(ArgumentOutOfRangeException) { Console.WriteLine("PASS invalid row"); }
 }
}
