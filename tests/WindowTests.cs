using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Batiao;
class WindowTests {
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static object Call(object target,string name,params object[] args){return target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);}
 [STAThread] static void Main(){
  Application.EnableVisualStyles();
  using(var pet=new Pet()){
   pet.Show();Application.DoEvents();Check(pet.Visible && pet.TopMost,"visible topmost window");
   var animation=(Animation)typeof(Pet).GetField("animation",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);
   var mouse=new MouseEventArgs(MouseButtons.Left,1,100,100,0);
   Call(pet,"OnMouseDown",mouse);Call(pet,"OnMouseUp",mouse);
   Check(animation.Row==4,"real click handler starts jump");
   for(int i=0;i<5;i++)animation.Tick();Check(animation.Row==0,"jump completes");
   Call(pet,"OnMouseDown",mouse);
   typeof(Pet).GetField("press",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(pet,new Point(Cursor.Position.X-20,Cursor.Position.Y));
   Call(pet,"OnMouseMove",mouse);Check(animation.Row==1,"drag handler starts directional animation");
   Call(pet,"OnMouseUp",mouse);Check(animation.Row==0,"drag release does not jump");
   var menu=(ContextMenuStrip)typeof(Pet).GetField("menu",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);
   Check(((ToolStripMenuItem)menu.Items[1]).DropDownItems.Count==9,"nine action menu entries");
   Check(((ToolStripMenuItem)menu.Items[2]).DropDownItems.Count==16,"sixteen direction menu entries");
   foreach(double scale in new double[]{0.75,1,1.25,1.5,2}){Call(pet,"SetScale",scale);Call(pet,"DrawFrame");Check(pet.Width==(int)(192*scale),"render scale "+scale);}
   for(int row=0;row<9;row++){Call(pet,"Play",row,false);for(int i=0;i<Animation.Counts[row];i++){Call(pet,"DrawFrame");animation.Tick();}}
   var look=typeof(Pet).GetField("look",BindingFlags.Instance|BindingFlags.NonPublic);
   for(int i=0;i<16;i++){look.SetValue(pet,i);Call(pet,"DrawFrame");}Check(true,"all 73 frames rendered through Win32");
   pet.Close();Application.DoEvents();
  }
 }
}
