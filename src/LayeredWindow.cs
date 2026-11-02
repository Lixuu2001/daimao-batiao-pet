using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace Batiao {
 public class LayeredWindow : Form {
  [StructLayout(LayoutKind.Sequential)] struct XY { public int X,Y; public XY(int x,int y){X=x;Y=y;} }
  [StructLayout(LayoutKind.Sequential,Pack=1)] struct Blend { public byte Op,Flags,Alpha,Format; }
  [DllImport("user32.dll",SetLastError=true)] static extern bool UpdateLayeredWindow(IntPtr w,IntPtr dst,ref XY pos,ref XY size,IntPtr src,ref XY origin,int key,ref Blend blend,int flags);
  [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr w);
  [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr w,IntPtr dc);
  [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
  [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
  [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
  [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
  protected override CreateParams CreateParams { get { var p=base.CreateParams; p.ExStyle|=0x80000|0x80; return p; } }
  protected void Present(Bitmap bitmap) {
   IntPtr screen=GetDC(IntPtr.Zero), memory=IntPtr.Zero, handle=IntPtr.Zero, old=IntPtr.Zero;
   try {
    memory=CreateCompatibleDC(screen); handle=bitmap.GetHbitmap(Color.FromArgb(0)); old=SelectObject(memory,handle);
    var pos=new XY(Left,Top); var size=new XY(bitmap.Width,bitmap.Height); var origin=new XY(0,0);
    var blend=new Blend{Alpha=255,Format=1};
    if(!UpdateLayeredWindow(Handle,screen,ref pos,ref size,memory,ref origin,0,ref blend,2)) throw new Win32Exception();
   } finally {
    if(old!=IntPtr.Zero)SelectObject(memory,old);
    if(handle!=IntPtr.Zero)DeleteObject(handle);
    if(memory!=IntPtr.Zero)DeleteDC(memory);
    ReleaseDC(IntPtr.Zero,screen);
   }
  }
 }
}
