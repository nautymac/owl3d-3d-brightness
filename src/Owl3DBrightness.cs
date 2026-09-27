// Owl3D 3D Brightness — single-file tray app.
// While Owl3D Live 3D (display-mirror.exe) is running, applies a GPU output gamma ramp
// (gamma curve + brightness gain) and keeps re-asserting it; restores the identity ramp when 3D stops.
// Settings: %LOCALAPPDATA%\Owl3D\owl3d-3d-brightness.ini   (so the exe can live anywhere)
// Build:    csc.exe /target:winexe /out:Owl3DBrightness.exe Owl3DBrightness.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

static class Native
{
    [DllImport("gdi32.dll")] public static extern bool SetDeviceGammaRamp(IntPtr hdc, ushort[] ramp);
    [DllImport("gdi32.dll")] public static extern bool GetDeviceGammaRamp(IntPtr hdc, ushort[] ramp);
    [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr h);
    [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr h, IntPtr dc);
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint mods, uint vk);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    public const uint MOD_ALT = 1, MOD_CONTROL = 2, MOD_NOREPEAT = 0x4000;
    public const int WM_HOTKEY = 0x0312;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public int cbSize; public IntPtr hWnd; public int uID; public int uFlags; public int uCallbackMessage; public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szTip;
        public int dwState, dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string szInfo;
        public int uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string szInfoTitle;
        public int dwInfoFlags; public Guid guidItem; public IntPtr hBalloonIcon;
    }
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] public static extern bool Shell_NotifyIcon(int msg, ref NOTIFYICONDATA d);
    public const int NIM_MODIFY = 1, NIF_INFO = 0x10, NIIF_USER = 4, NIIF_LARGE_ICON = 0x20;

    // Balloon/toast with our own icon instead of the blue "i". Falls back to the standard balloon.
    public static void Balloon(NotifyIcon tray, Icon icon, string title, string text)
    {
        try
        {
            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            int id = (int)typeof(NotifyIcon).GetField("id", bf).GetValue(tray);
            var win = (NativeWindow)typeof(NotifyIcon).GetField("window", bf).GetValue(tray);
            var d = new NOTIFYICONDATA();
            d.cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)); d.hWnd = win.Handle; d.uID = id; d.uFlags = NIF_INFO;
            d.szInfo = text; d.szInfoTitle = title; d.dwInfoFlags = NIIF_USER | NIIF_LARGE_ICON; d.hBalloonIcon = icon.Handle;
            if (Shell_NotifyIcon(NIM_MODIFY, ref d)) return;
        }
        catch { }
        tray.ShowBalloonTip(1500, title, text, ToolTipIcon.None);
    }
}

// Small always-on-top overlay that shows the current values for ~1.5 s after a hotkey press.
class Osd : Form
{
    Label l; System.Windows.Forms.Timer t;
    public Osd()
    {
        FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black; Opacity = 0.85; ClientSize = new Size(640, 56);
        l = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Malgun Gothic", 14, FontStyle.Bold) };
        Controls.Add(l);
        t = new System.Windows.Forms.Timer { Interval = 1500 }; t.Tick += (o, e) => { t.Stop(); Hide(); };
    }
    protected override bool ShowWithoutActivation { get { return true; } }
    protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= 0x08000000 | 0x00000080; return cp; } } // NOACTIVATE | TOOLWINDOW
    public void ShowText(string text)
    {
        l.Text = text;
        var wa = Screen.PrimaryScreen.WorkingArea;
        Location = new Point(wa.Left + (wa.Width - Width) / 2, wa.Top + 60);
        if (!Visible) Show(); t.Stop(); t.Start();
    }
}

// UI language: Korean when the Windows display language is Korean, English otherwise.
// "--lang en" / "--lang ko" on the command line forces one (for testing).
static class T
{
    public static bool Ko = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ko";
    public static string L(string ko, string en) { return Ko ? ko : en; }
}

// Toast header icon/name come from a Start Menu shortcut carrying the same AppUserModelID as the process.
static class Aumid
{
    public const string Id = "Owl3D.Brightness";
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] static extern int SetCurrentProcessExplicitAppUserModelID(string id);

    [ComImport, Guid("00021401-0000-0000-C000-000000000046")] class CShellLink { }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder f, int cch, IntPtr pfd, uint flags);
        void GetIDList(out IntPtr ppidl); void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder s, int cch);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string s);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder s, int cch);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string s);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder s, int cch);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string s);
        void GetHotkey(out short w); void SetHotkey(short w);
        void GetShowCmd(out int c); void SetShowCmd(int c);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder s, int cch, out int i);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string s, int i);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string s, uint r);
        void Resolve(IntPtr hwnd, uint flags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string s);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)] struct PROPERTYKEY { public Guid fmtid; public uint pid; }
    [StructLayout(LayoutKind.Explicit, Size = 24)] struct PROPVARIANT { [FieldOffset(0)] public ushort vt; [FieldOffset(8)] public IntPtr p; }
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    interface IPropertyStore
    {
        void GetCount(out uint c); void GetAt(uint i, out PROPERTYKEY k);
        void GetValue(ref PROPERTYKEY k, out PROPVARIANT v); void SetValue(ref PROPERTYKEY k, ref PROPVARIANT v); void Commit();
    }

    public static string ShortcutPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), T.L("Owl3D 3D 밝기.lnk", "Owl3D 3D Brightness.lnk")); } }

    public static string Setup()
    {
        string note = "";
        try
        {
            string exe = Application.ExecutablePath;
            var link = (IShellLinkW)new CShellLink();
            link.SetPath(exe); link.SetWorkingDirectory(Path.GetDirectoryName(exe)); link.SetIconLocation(exe, 0);
            link.SetDescription("Owl3D Live 3D brightness / gamma");
            var key = new PROPERTYKEY { fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), pid = 5 };
            var pv = new PROPVARIANT { vt = 31, p = Marshal.StringToCoTaskMemUni(Id) };   // VT_LPWSTR
            var store = (IPropertyStore)link; store.SetValue(ref key, ref pv); store.Commit();
            Marshal.FreeCoTaskMem(pv.p);
            ((System.Runtime.InteropServices.ComTypes.IPersistFile)link).Save(ShortcutPath, true);
            note = "shortcut ok";
        }
        catch (Exception ex) { note = "shortcut failed: " + ex.Message; }
        int hr = SetCurrentProcessExplicitAppUserModelID(Id);
        return note + " aumid hr=" + hr;
    }
}

class Settings
{
    public double Gamma = 0.7, Gain = 1.4, Contrast = 1.0;
    public int Sat = 70;          // NVIDIA digital vibrance level while 3D is on (driver default is 50)
    public int SatRestore = -1;   // level to put back when 3D stops; -1 = nothing applied
    // user-editable "Default" preset, in slider units (gamma/gain/contrast x100)
    public int DefGamma = 70, DefGain = 140, DefCon = 100, DefSat = 70;
    public static string Dir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Owl3D"); } }
    public static string File_ { get { return Path.Combine(Dir, "owl3d-3d-brightness.ini"); } }
    public static string LogFile { get { return Path.Combine(Dir, "owl3d-3d-brightness.log"); } }
    public void Load()
    {
        try
        {
            if (!File.Exists(File_)) return;
            foreach (var line in File.ReadAllLines(File_))
            {
                var kv = line.Split('=');
                if (kv.Length != 2) continue;
                double v;
                if (!double.TryParse(kv[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v)) continue;
                switch (kv[0].Trim())
                {
                    case "gamma": Gamma = v; break;
                    case "gain": Gain = v; break;
                    case "contrast": Contrast = v; break;
                    case "saturation": Sat = (int)v; break;
                    case "saturation_restore": SatRestore = (int)v; break;
                    case "default_gamma": DefGamma = (int)Math.Round(v * 100); break;
                    case "default_gain": DefGain = (int)Math.Round(v * 100); break;
                    case "default_contrast": DefCon = (int)Math.Round(v * 100); break;
                    case "default_saturation": DefSat = (int)v; break;
                }
            }
        }
        catch { }
    }
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            var c = CultureInfo.InvariantCulture;
            File.WriteAllText(File_, "gamma=" + Gamma.ToString("0.00", c) + "\r\ngain=" + Gain.ToString("0.00", c)
                + "\r\ncontrast=" + Contrast.ToString("0.00", c) + "\r\nsaturation=" + Sat + "\r\nsaturation_restore=" + SatRestore
                + "\r\ndefault_gamma=" + (DefGamma / 100.0).ToString("0.00", c) + "\r\ndefault_gain=" + (DefGain / 100.0).ToString("0.00", c)
                + "\r\ndefault_contrast=" + (DefCon / 100.0).ToString("0.00", c) + "\r\ndefault_saturation=" + DefSat + "\r\n");
        }
        catch { }
    }
}

static class Ramp
{
    public const int Identity128 = 32896;
    // gamma follows the usual convention (sView / NVIDIA): 1.0 = none, larger = brighter midtones.
    // Implemented as out = in ^ (1/gamma).
    // contrast: 1.0 = none; scales around mid grey (out = (y-0.5)*contrast + 0.5), applied last.
    public static ushort[] Build(double gamma, double gain, double contrast = 1.0)
    {
        var r = new ushort[768];
        double exp = gamma <= 0 ? 1.0 : 1.0 / gamma;
        if (contrast <= 0) contrast = 1.0;
        for (int i = 0; i < 256; i++)
        {
            double x = i / 255.0;
            double y = Math.Pow(x, exp);
            if (gain > 0) y = Math.Min(1.0, y * gain);
            y = Math.Max(0.0, Math.Min(1.0, (y - 0.5) * contrast + 0.5));
            ushort v = (ushort)Math.Max(0, Math.Min(65535, Math.Round(y * 65535.0)));
            r[i] = v; r[256 + i] = v; r[512 + i] = v;
        }
        return r;
    }
    // largest distance from the identity ramp, in 8-bit levels, in either direction
    public static int MaxDeviation(double gamma, double gain, double contrast = 1.0)
    {
        var r = Build(gamma, gain, contrast); int m = 0;
        for (int i = 0; i < 256; i++) { int d = Math.Abs((int)Math.Round(r[i] / 257.0) - i); if (d > m) m = d; }
        return m;
    }
    public static bool Apply(ushort[] r)
    {
        IntPtr dc = Native.GetDC(IntPtr.Zero);
        try { return Native.SetDeviceGammaRamp(dc, r); } finally { Native.ReleaseDC(IntPtr.Zero, dc); }
    }
    public static int Read128()
    {
        var r = new ushort[768]; IntPtr dc = Native.GetDC(IntPtr.Zero);
        try { Native.GetDeviceGammaRamp(dc, r); } finally { Native.ReleaseDC(IntPtr.Zero, dc); }
        return r[128];
    }
    public static bool Is3DOn() { return Process.GetProcessesByName("display-mirror").Length > 0; }
}

// Colour saturation through the NVIDIA driver ("digital vibrance"). A gamma ramp cannot change saturation
// because it works on R, G and B separately. Not available on non-NVIDIA outputs (Available = false).
static class Nv
{
    [DllImport("nvapi64.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)] static extern IntPtr QI(uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int InitD();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int EnumD(int i, out IntPtr h);
    [StructLayout(LayoutKind.Sequential)] struct DVCEX { public uint version; public int cur, min, max, def; }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int DvcD(IntPtr h, int outputId, ref DVCEX info);
    static EnumD en; static DvcD get, set; static bool tried;
    public static bool Available; public static int Min = 0, Max = 100, Default = 50;

    static T F<T>(uint id) where T : class { IntPtr p = QI(id); return p == IntPtr.Zero ? null : (T)(object)Marshal.GetDelegateForFunctionPointer(p, typeof(T)); }
    static DVCEX New() { var d = new DVCEX(); d.version = (uint)(Marshal.SizeOf(typeof(DVCEX)) | 0x10000); return d; }
    public static void Init()
    {
        if (tried) return; tried = true;
        try
        {
            var init = F<InitD>(0x0150E828); if (init == null || init() != 0) return;
            en = F<EnumD>(0x9ABDD40D); get = F<DvcD>(0x0E45002D); set = F<DvcD>(0x4A82C2B1);
            if (en == null || get == null || set == null) return;
            IntPtr h; if (en(0, out h) != 0) return;
            var d = New(); if (get(h, 0, ref d) != 0) return;
            Min = d.min; Max = d.max; Default = d.def; Available = true;
        }
        catch { Available = false; }
    }
    public static int Get()
    {
        if (!Available) return -1;
        try { IntPtr h; if (en(0, out h) != 0) return -1; var d = New(); return get(h, 0, ref d) == 0 ? d.cur : -1; } catch { return -1; }
    }
    public static bool Set(int level)
    {
        if (!Available) return false;
        try { IntPtr h; if (en(0, out h) != 0) return false; var d = New(); d.cur = Math.Max(Min, Math.Min(Max, level)); return set(h, 0, ref d) == 0; } catch { return false; }
    }
}

class MainForm : Form
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", RunName = "Owl3D3DBrightness";
    Settings s = new Settings();
    TrackBar tg, tk, tc, ts; Label lg, lk, lc, ls, st; NotifyIcon tray; System.Windows.Forms.Timer poll, save; Icon bigIcon = SystemIcons.Application;
    bool applied = false; int target128 = Ramp.Identity128; DateTime onSince = DateTime.MinValue; bool reallyExit = false;

    public MainForm()
    {
        s.Load(); Nv.Init();
        // a previous run ended (crash / power off) while saturation was applied: put the original level back
        if (Nv.Available && s.SatRestore >= 0 && !Ramp.Is3DOn()) { Nv.Set(s.SatRestore); Log("startup: restored saturation " + s.SatRestore); s.SatRestore = -1; s.Save(); }
        Text = T.L("Owl3D 3D 밝기 조절", "Owl3D 3D Brightness"); TopMost = true; StartPosition = FormStartPosition.Manual;
        Location = new Point(40, 40); ClientSize = new Size(540, 448); FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Font = new Font(T.L("Malgun Gothic", "Segoe UI"), T.Ko ? 10f : 9.5f);

        lg = new Label { Location = new Point(12, 12), Size = new Size(516,22) }; Controls.Add(lg);
        tg = new TrackBar { Location = new Point(12, 36), Size = new Size(516,45), Minimum = 50, Maximum = 250, TickFrequency = 10, SmallChange = 1, LargeChange = 5 };
        tg.Value = Clamp((int)Math.Round(s.Gamma * 100), 50, 250); Controls.Add(tg);
        lk = new Label { Location = new Point(12, 88), Size = new Size(516,22) }; Controls.Add(lk);
        tk = new TrackBar { Location = new Point(12, 112), Size = new Size(516,45), Minimum = 100, Maximum = 200, TickFrequency = 10, SmallChange = 1, LargeChange = 5 };
        tk.Value = Clamp((int)Math.Round(Math.Max(1.0, s.Gain) * 100), 100, 200); Controls.Add(tk);
        lc = new Label { Location = new Point(12, 240), Size = new Size(516,22) }; Controls.Add(lc);
        tc = new TrackBar { Location = new Point(12, 264), Size = new Size(516,45), Minimum = 50, Maximum = 150, TickFrequency = 10, SmallChange = 1, LargeChange = 5 };
        tc.Value = Clamp((int)Math.Round(s.Contrast * 100), 50, 150); Controls.Add(tc);
        ls = new Label { Location = new Point(12, 164), Size = new Size(516,22) }; Controls.Add(ls);
        ts = new TrackBar { Location = new Point(12, 188), Size = new Size(516,45), Minimum = Nv.Min, Maximum = Nv.Max, TickFrequency = 10, SmallChange = 1, LargeChange = 5, Enabled = Nv.Available };
        ts.Value = Clamp(s.Sat, ts.Minimum, ts.Maximum); Controls.Add(ts);
        st = new Label { Location = new Point(12, 316), Size = new Size(516,48) }; Controls.Add(st);
        var bOff = new Button { Text = T.L("보정 끄기", "Off"), Location = new Point(12, 372), Size = new Size(130, 30) }; Controls.Add(bOff);
        var bDef = new Button { Text = T.L("기본값", "Default"), Location = new Point(150, 372), Size = new Size(130, 30) }; Controls.Add(bDef);
        var bSave = new Button { Text = T.L("현재 값을 기본값으로 저장", "Save current as default"), Location = new Point(288, 372), Size = new Size(240, 30) }; Controls.Add(bSave);
        bSave.Click += (o, e) => SaveAsDefault();
        var cAuto = new CheckBox { Text = T.L("로그인 시 자동 실행", "Start at login"), Location = new Point(14, 412), Size = new Size(300, 24), Checked = IsAutoStart() }; Controls.Add(cAuto);
        cAuto.CheckedChanged += (o, e) => SetAutoStart(cAuto.Checked);
        bOff.Click += (o, e) => SetAll(100, 100, 100, Nv.Default);
        bDef.Click += (o, e) => SetAll(DEF_GAMMA, DEF_GAIN, DEF_CON, DEF_SAT);

        save = new System.Windows.Forms.Timer { Interval = 400 };
        save.Tick += (o, e) =>
        {
            save.Stop();
            s.Gamma = tg.Value / 100.0; s.Gain = tk.Value <= 100 ? 0 : tk.Value / 100.0; s.Contrast = tc.Value / 100.0; s.Sat = ts.Value;
            s.Save(); if (applied) ApplyNow();
        };
        EventHandler changed = (o, e) => { RefreshLabels(); save.Stop(); save.Start(); };
        tg.ValueChanged += changed; tk.ValueChanged += changed; tc.ValueChanged += changed; ts.ValueChanged += changed;

        // use the icon embedded in this exe (/win32icon at build time); fall back to the generic one
        Icon appIcon = SystemIcons.Application;
        try { appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? appIcon; } catch { }
        Icon = appIcon; bigIcon = appIcon;
        tray = new NotifyIcon { Icon = appIcon, Visible = true, Text = T.L("Owl3D 3D 밝기", "Owl3D 3D Brightness") };
        var menu = new ContextMenu();
        menu.MenuItems.Add(T.L("조절 창 열기", "Open controls"), (o, e) => ShowWindow());
        menu.MenuItems.Add(T.L("종료", "Exit"), (o, e) => { reallyExit = true; Close(); });
        tray.ContextMenu = menu; tray.DoubleClick += (o, e) => ShowWindow();

        poll = new System.Windows.Forms.Timer { Interval = 2000 }; poll.Tick += (o, e) => Poll(); poll.Start();
        RefreshLabels(); Log("start gamma=" + s.Gamma + " gain=" + s.Gain + " contrast=" + s.Contrast + " saturation=" + s.Sat + " nvidia=" + Nv.Available);
    }

    // the "Default" preset (slider units: x100 except saturation); shown next to each value in the window
    // The preset is user-editable ("save current as default") and lives in the ini file.
    int DEF_GAMMA { get { return s.DefGamma; } }
    int DEF_GAIN { get { return s.DefGain; } }
    int DEF_CON { get { return s.DefCon; } }
    int DEF_SAT { get { return s.DefSat; } }
    void SaveAsDefault()
    {
        s.DefGamma = tg.Value; s.DefGain = tk.Value; s.DefCon = tc.Value; s.DefSat = ts.Value;
        s.Save(); RefreshLabels();
        Log("default preset saved gamma=" + s.DefGamma + " gain=" + s.DefGain + " contrast=" + s.DefCon + " saturation=" + s.DefSat);
        osd.ShowText(T.L("현재 값을 기본값으로 저장했습니다", "Saved current values as default"));
    }
    void SetAll(int gamma, int gain, int contrast, int sat)
    {
        tg.Value = Clamp(gamma, tg.Minimum, tg.Maximum); tk.Value = Clamp(gain, tk.Minimum, tk.Maximum);
        tc.Value = Clamp(contrast, tc.Minimum, tc.Maximum); ts.Value = Clamp(sat, ts.Minimum, ts.Maximum);
    }
    static int Clamp(int v, int lo, int hi) { return Math.Max(lo, Math.Min(hi, v)); }
    void ShowWindow() { Show(); WindowState = FormWindowState.Normal; Activate(); }

    // ---- global hotkeys: Ctrl+Alt+= / -  gain,  Ctrl+Alt+] / [  gamma,  Ctrl+Alt+0 off,  Ctrl+Alt+9 default ----
    Osd osd = new Osd();
    const int HK_GAIN_UP = 1, HK_GAIN_DN = 2, HK_GAMMA_UP = 3, HK_GAMMA_DN = 4, HK_OFF = 5, HK_DEF = 6, HK_WIN = 7,
              HK_CON_UP = 8, HK_CON_DN = 9, HK_SAT_UP = 10, HK_SAT_DN = 11, HK_LAST = 11;
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        uint m = Native.MOD_CONTROL | Native.MOD_ALT | Native.MOD_NOREPEAT;
        bool ok = Native.RegisterHotKey(Handle, HK_GAIN_UP, m, 0xBB)   // VK_OEM_PLUS  (=)
               & Native.RegisterHotKey(Handle, HK_GAIN_DN, m, 0xBD)   // VK_OEM_MINUS (-)
               & Native.RegisterHotKey(Handle, HK_GAMMA_UP, m, 0xDD)  // VK_OEM_6     (])
               & Native.RegisterHotKey(Handle, HK_GAMMA_DN, m, 0xDB)  // VK_OEM_4     ([)
               & Native.RegisterHotKey(Handle, HK_OFF, m, 0x30)       // 0
               & Native.RegisterHotKey(Handle, HK_DEF, m, 0x39)       // 9
               & Native.RegisterHotKey(Handle, HK_WIN, m, 0x42)       // B : show/hide this window
               & Native.RegisterHotKey(Handle, HK_CON_UP, m, 0xDE)    // VK_OEM_7     (')
               & Native.RegisterHotKey(Handle, HK_CON_DN, m, 0xBA)    // VK_OEM_1     (;)
               & Native.RegisterHotKey(Handle, HK_SAT_UP, m, 0xBE)    // VK_OEM_PERIOD (.)
               & Native.RegisterHotKey(Handle, HK_SAT_DN, m, 0xBC);   // VK_OEM_COMMA  (,)
        Log("hotkeys registered=" + ok);
    }
    protected override void WndProc(ref Message msg)
    {
        if (msg.Msg == Native.WM_HOTKEY)
        {
            if ((int)msg.WParam == HK_WIN) { if (Visible) Hide(); else ShowWindow(); return; }
            switch ((int)msg.WParam)
            {
                case HK_GAIN_UP: tk.Value = Clamp(tk.Value + 5, tk.Minimum, tk.Maximum); break;
                case HK_GAIN_DN: tk.Value = Clamp(tk.Value - 5, tk.Minimum, tk.Maximum); break;
                case HK_GAMMA_UP: tg.Value = Clamp(tg.Value + 5, tg.Minimum, tg.Maximum); break;
                case HK_GAMMA_DN: tg.Value = Clamp(tg.Value - 5, tg.Minimum, tg.Maximum); break;
                case HK_CON_UP: tc.Value = Clamp(tc.Value + 5, tc.Minimum, tc.Maximum); break;
                case HK_CON_DN: tc.Value = Clamp(tc.Value - 5, tc.Minimum, tc.Maximum); break;
                case HK_SAT_UP: if (Nv.Available) ts.Value = Clamp(ts.Value + 5, ts.Minimum, ts.Maximum); break;
                case HK_SAT_DN: if (Nv.Available) ts.Value = Clamp(ts.Value - 5, ts.Minimum, ts.Maximum); break;
                case HK_OFF: SetAll(100, 100, 100, Nv.Default); break;
                case HK_DEF: SetAll(DEF_GAMMA, DEF_GAIN, DEF_CON, DEF_SAT); break;
            }
            int dev = Ramp.MaxDeviation(tg.Value / 100.0, tk.Value / 100.0, tc.Value / 100.0);
            osd.ShowText(string.Format(T.L("감마 {0:0.00}  밝기 {1:0.00}  대비 {2:0.00}  채도 {3}{4}", "Gamma {0:0.00}  Gain {1:0.00}  Contrast {2:0.00}  Sat {3}{4}"),
                tg.Value / 100.0, tk.Value / 100.0, tc.Value / 100.0, Nv.Available ? ts.Value.ToString() : "-",
                dev > 128 ? T.L("  (한계 초과)", "  (over limit)") : (Ramp.Is3DOn() ? "" : T.L("  (3D 꺼짐)", "  (3D off)"))));
            return;
        }
        base.WndProc(ref msg);
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!reallyExit && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); Native.Balloon(tray, bigIcon, T.L("Owl3D 3D 밝기", "Owl3D 3D Brightness"), T.L("트레이에서 계속 동작합니다. 종료는 트레이 아이콘 우클릭.", "Still running in the tray. Right-click the tray icon to exit.")); return; }
        poll.Stop(); if (applied) ResetNow(); tray.Visible = false;
        for (int id = 1; id <= HK_LAST; id++) Native.UnregisterHotKey(Handle, id);
        base.OnFormClosing(e);
    }

    void RefreshLabels()
    {
        double g = tg.Value / 100.0, k = tk.Value / 100.0;
        lg.Text = string.Format(T.L("감마: {0:0.00}   [기본 {1:0.00}]   1.00 = 없음, 높을수록 밝게", "Gamma: {0:0.00}   [default {1:0.00}]   1.00 = none, higher = brighter"), g, DEF_GAMMA / 100.0);
        lk.Text = string.Format(T.L("밝기 배율: {0:0.00}   [기본 {1:0.00}]   1.00 = 없음, 높으면 밝은 곳이 흰색으로", "Gain: {0:0.00}   [default {1:0.00}]   1.00 = none, higher clips to white"), k, DEF_GAIN / 100.0);
        double c = tc.Value / 100.0;
        lc.Text = string.Format(T.L("대비: {0:0.00}   [기본 {1:0.00}]   1.00 = 없음, 높을수록 명암 차이가 커짐", "Contrast: {0:0.00}   [default {1:0.00}]   1.00 = none, higher = more contrast"), c, DEF_CON / 100.0);
        ls.Text = Nv.Available
            ? string.Format(T.L("색 진하기(채도): {0}   [기본 {1}]   {2} = 없음, 높을수록 진하게", "Saturation: {0}   [default {1}]   {2} = none, higher = more vivid"), ts.Value, DEF_SAT, Nv.Default)
            : T.L("색 진하기(채도): NVIDIA 그래픽 출력에서만 쓸 수 있습니다", "Saturation: only available on NVIDIA outputs");
        int dev = Ramp.MaxDeviation(g, k, c); double mid = Ramp.Build(g, k, c)[128] / 655.35;
        // Windows refuses ramps further than 128 levels from the original; say so in plain words only when it happens
        if (dev > 128) { st.ForeColor = Color.Firebrick; st.Text = T.L("보정이 너무 강해서 Windows가 받아주지 않습니다. 감마·밝기·대비 중 하나를 1.00 쪽으로 조금 되돌리세요.", "Too strong for Windows to accept. Move gamma, gain or contrast a little back towards 1.00."); }
        else { st.ForeColor = Color.Black; st.Text = Ramp.Is3DOn() ? T.L("Live 3D 켜짐 — 지금 화면에 적용 중입니다.", "Live 3D is on — applied to the screen now.") : T.L("Live 3D 꺼짐 — 3D를 켜면 이 값이 적용됩니다.", "Live 3D is off — these values apply when 3D starts."); }
    }

    void ApplyNow()
    {
        var r = Ramp.Build(s.Gamma, s.Gain, s.Contrast);
        bool ok = Ramp.Apply(r); Thread.Sleep(150); int v = Ramp.Read128();
        if (ok && Math.Abs(v - r[128]) <= 2) { target128 = v; applied = true; Log("applied gamma=" + s.Gamma + " gain=" + s.Gain + " contrast=" + s.Contrast + " ramp128=" + v); }
        else Log("apply failed ok=" + ok + " ramp128=" + v + " expected=" + r[128]);
        ApplySat();
    }
    void ApplySat()
    {
        if (!Nv.Available) return;
        int cur = Nv.Get(); if (cur < 0 || cur == s.Sat) return;
        if (s.SatRestore < 0) { s.SatRestore = cur; s.Save(); }   // remember what to put back, even across a crash
        Log("saturation " + cur + " -> " + s.Sat + " ok=" + Nv.Set(s.Sat));
    }
    void RestoreSat()
    {
        if (!Nv.Available || s.SatRestore < 0) return;
        Log("saturation restore -> " + s.SatRestore + " ok=" + Nv.Set(s.SatRestore));
        s.SatRestore = -1; s.Save();
    }
    void ResetNow()
    {
        RestoreSat();
        for (int t = 0; t < 3; t++)
        {
            Ramp.Apply(Ramp.Build(1.0, 0)); Thread.Sleep(300);
            if (Ramp.Read128() == Ramp.Identity128) { Log("reset ok"); applied = false; target128 = Ramp.Identity128; return; }
            Thread.Sleep(700);
        }
        Log("reset NOT effective ramp128=" + Ramp.Read128());
    }
    void Poll()
    {
        bool on = Ramp.Is3DOn();
        if (on && !applied)
        {
            if (onSince == DateTime.MinValue) onSince = DateTime.Now;
            if ((DateTime.Now - onSince).TotalSeconds >= 4) ApplyNow();   // let display-mirror finish its fullscreen init
        }
        else if (on && applied)
        {
            if (Ramp.Read128() != target128) { Log("ramp changed while 3D on -> re-applying"); ApplyNow(); }
            else if (Nv.Available && Nv.Get() != s.Sat) ApplySat();
        }
        else if (!on) { onSince = DateTime.MinValue; if (applied || s.SatRestore >= 0 || Ramp.Read128() != Ramp.Identity128) ResetNow(); }
        if (Visible) RefreshLabels();
    }

    static bool IsAutoStart() { using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) { return k != null && k.GetValue(RunName) != null; } }
    static void SetAutoStart(bool on)
    {
        using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
        { if (on) k.SetValue(RunName, "\"" + Application.ExecutablePath + "\""); else k.DeleteValue(RunName, false); }
    }
    static void Log(string m) { try { Directory.CreateDirectory(Settings.Dir); File.AppendAllText(Settings.LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + m + "\r\n"); } catch { } }

    [STAThread]
    static void Main(string[] args)
    {
        int li = Array.IndexOf(args, "--lang");
        if (li >= 0 && li + 1 < args.Length) T.Ko = args[li + 1].ToLowerInvariant() == "ko";
        bool created; var mutex = new Mutex(true, "Global\\Owl3D3DBrightness", out created);
        if (!created) { MessageBox.Show(T.L("이미 실행 중입니다. 트레이 아이콘을 더블클릭하세요.", "Already running. Double-click the tray icon."), T.L("Owl3D 3D 밝기", "Owl3D 3D Brightness")); return; }
        string aumid = Aumid.Setup();   // must run before any window / tray icon is created
        Application.EnableVisualStyles();
        var f = new MainForm();
        Log("aumid: " + aumid);
        bool startHidden = Array.IndexOf(args, "--hidden") >= 0;
        if (startHidden) { f.Load += (o, e) => f.Hide(); f.Opacity = 0; f.Shown += (o, e) => { f.Hide(); f.Opacity = 1; }; }
        Application.Run(f);
        GC.KeepAlive(mutex);
    }
}
