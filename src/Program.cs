using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace Batiao {
 public sealed class Pet : LayeredWindow {
  readonly Bitmap atlas;
  readonly Animation animation=new Animation();
  readonly Timer clock=new Timer { Interval=140 };
  readonly ContextMenuStrip menu=new ContextMenuStrip();
  readonly NotifyIcon tray=new NotifyIcon();
  Bitmap[][] frames;
  bool pressed, dragged, paused;
  Point press, offset;
  int look=-1;
  public Pet() {
   Text="呆毛八条 · 桌面宠物"; FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; TopMost=true;
   StartPosition=FormStartPosition.Manual; AutoScaleMode=AutoScaleMode.None; KeyPreview=true;
   using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Batiao.Sprite")) {
    if(stream==null)throw new InvalidOperationException("Missing embedded sprite sheet.");
    using(var original=new Bitmap(stream)) atlas=new Bitmap(original);
   }
   if(atlas.Width!=1536 || atlas.Height!=2288)throw new InvalidOperationException("Expected 1536 x 2288 atlas.");
   SetScale(1.25);
   var area=Screen.PrimaryScreen.WorkingArea;
   Location=new Point(area.Right-Width-36,area.Bottom-Height-24);
   string[] names={"待机 / 眨眼","向右跑","向左跑","挥爪","跳跃","失落","等待","思考","检查"};
   var actions=new ToolStripMenuItem("播放动作");
   for(int i=0;i<names.Length;i++) { int row=i; actions.DropDownItems.Add(names[i],null,delegate { Play(row,row==0 || row==1 || row==2); }); }
   menu.Items.Add("呆毛八条",null,delegate { Play(3,false); }); menu.Items.Add(actions);
   var directions=new ToolStripMenuItem("看看方向");
   for(int i=0;i<16;i++) { int dir=i; directions.DropDownItems.Add((i*22.5).ToString("0.#")+"°",null,delegate { look=dir; DrawFrame(); }); }
   menu.Items.Add(directions);
   var sizes=new ToolStripMenuItem("大小");
   foreach(double value in new double[]{0.75,1,1.25,1.5,2}) { double scale=value; sizes.DropDownItems.Add((value*100)+"%",null,delegate { SetScale(scale); KeepVisible(); DrawFrame(); }); }
   menu.Items.Add(sizes);
   var pause=new ToolStripMenuItem("暂停动画"){CheckOnClick=true}; pause.CheckedChanged+=delegate { paused=pause.Checked; }; menu.Items.Add(pause);
   menu.Items.Add("回到主屏幕",null,delegate { var r=Screen.PrimaryScreen.WorkingArea; Location=new Point(r.Right-Width-36,r.Bottom-Height-24); KeepVisible(); DrawFrame(); });
   menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("退出",null,delegate { Close(); });
   tray.Icon=SystemIcons.Application; tray.Text="呆毛八条：左键跳跃，拖动移动，右键菜单"; tray.ContextMenuStrip=menu; tray.Visible=true;
   tray.DoubleClick+=delegate { Show(); KeepVisible(); Play(3,false); };
   clock.Tick+=delegate { if(!paused && look<0) { animation.Tick(); DrawFrame(); } };
   Shown+=delegate { KeepVisible(); DrawFrame(); clock.Start(); };
  }
  void SetScale(double scale) {
   if(frames!=null)foreach(var row in frames)foreach(var frame in row)frame.Dispose();
   Size=new Size((int)(192*scale),(int)(208*scale)); frames=new Bitmap[11][];
   for(int row=0;row<11;row++) {
    int count=row<9?Animation.Counts[row]:8; frames[row]=new Bitmap[count];
    for(int col=0;col<count;col++) {
     var bitmap=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);
     using(var g=Graphics.FromImage(bitmap)) {
      g.CompositingMode=CompositingMode.SourceCopy; g.InterpolationMode=InterpolationMode.HighQualityBicubic; g.PixelOffsetMode=PixelOffsetMode.HighQuality;
      using(var cell=atlas.Clone(new Rectangle(col*192,row*208,192,208),PixelFormat.Format32bppArgb))
       g.DrawImage(cell,new Rectangle(0,0,Width,Height));
     }
     frames[row][col]=bitmap;
    }
   }
  }
  void KeepVisible(){Location=Interaction.Clamp(Location,Size,Screen.FromPoint(new Point(Left+Width/2,Top+Height/2)).WorkingArea);}
  void Play(int row,bool loop){look=-1;animation.Play(row,loop);DrawFrame();}
  void DrawFrame(){if(!IsHandleCreated)return; Present(look<0?frames[animation.Row][animation.Frame]:frames[9+look/8][look%8]);}
  protected override void OnMouseDown(MouseEventArgs e) {
   base.OnMouseDown(e); if(e.Button!=MouseButtons.Left)return;
   pressed=true;dragged=false;press=Cursor.Position;offset=new Point(press.X-Left,press.Y-Top);Capture=true;
  }
  protected override void OnMouseMove(MouseEventArgs e) {
   base.OnMouseMove(e); if(!pressed)return; var now=Cursor.Position;
   if(!dragged && Interaction.IsDrag(press,now)){dragged=true;Play(now.X<press.X?2:1,true);}
   if(dragged){Location=new Point(now.X-offset.X,now.Y-offset.Y);DrawFrame();}
  }
  protected override void OnMouseUp(MouseEventArgs e) {
   base.OnMouseUp(e);
   if(e.Button==MouseButtons.Right){menu.Show(Cursor.Position);return;}
   if(e.Button!=MouseButtons.Left || !pressed)return;
   bool wasDrag=dragged; pressed=false;Capture=false;KeepVisible();Play(wasDrag?0:4,wasDrag);
  }
  protected override void OnMouseCaptureChanged(EventArgs e){base.OnMouseCaptureChanged(e);if(pressed && !Capture){pressed=false;KeepVisible();Play(0,true);}}
  protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Escape)Close();}
  protected override void Dispose(bool disposing){if(disposing){clock.Dispose();tray.Visible=false;tray.Dispose();menu.Dispose();if(frames!=null)foreach(var row in frames)foreach(var frame in row)frame.Dispose();atlas.Dispose();}base.Dispose(disposing);}
 }
 static class Program {
  [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
  [STAThread] static void Main(string[] args) {
   SetProcessDPIAware(); Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   try { using(var pet=new Pet()) {
    if(args.Length==1 && args[0]=="--smoke-test") { var timer=new Timer{Interval=1800};timer.Tick+=delegate{timer.Stop();pet.Close();};timer.Start();pet.FormClosed+=delegate{timer.Dispose();}; }
    Application.Run(pet);
   }} catch(Exception e){MessageBox.Show(e.Message,"呆毛八条启动失败",MessageBoxButtons.OK,MessageBoxIcon.Error);Environment.ExitCode=1;}
  }
 }
}
