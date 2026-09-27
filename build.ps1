# Builds dist\Owl3DBrightness.exe with the C# compiler that ships with Windows (.NET Framework 4).
# The application icon is NOT stored in this repository: it is taken from the Owl3D installed on this PC.
# If Owl3D is not installed the exe is built with the generic Windows icon.
#
#   powershell -ExecutionPolicy Bypass -File build.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src  = Join-Path $root 'src\Owl3DBrightness.cs'
$dist = Join-Path $root 'dist'
$ico  = Join-Path $dist 'app.ico'
$out  = Join-Path $dist 'Owl3DBrightness.exe'
$csc  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "csc.exe not found: $csc" }
New-Item -ItemType Directory -Force $dist | Out-Null

# --- icon: copy every size of the first icon group out of Owl3D.exe into an .ico file ---
$code = @'
using System; using System.IO; using System.Runtime.InteropServices; using System.Collections.Generic;
public static class IconExtract {
  [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern IntPtr LoadLibraryEx(string f, IntPtr h, uint fl);
  [DllImport("kernel32.dll")] static extern bool FreeLibrary(IntPtr h);
  delegate bool EnumResNameProc(IntPtr h, IntPtr type, IntPtr name, IntPtr l);
  [DllImport("kernel32.dll", CharSet=CharSet.Unicode)] static extern bool EnumResourceNames(IntPtr h, IntPtr type, EnumResNameProc cb, IntPtr l);
  [DllImport("kernel32.dll")] static extern IntPtr FindResource(IntPtr h, IntPtr name, IntPtr type);
  [DllImport("kernel32.dll")] static extern IntPtr LoadResource(IntPtr h, IntPtr res);
  [DllImport("kernel32.dll")] static extern IntPtr LockResource(IntPtr res);
  [DllImport("kernel32.dll")] static extern uint SizeofResource(IntPtr h, IntPtr res);
  static byte[] Get(IntPtr h, IntPtr name, int type) {
    IntPtr r = FindResource(h, name, (IntPtr)type); if (r == IntPtr.Zero) return null;
    int n = (int)SizeofResource(h, r); byte[] b = new byte[n]; Marshal.Copy(LockResource(LoadResource(h, r)), b, 0, n); return b; }
  public static bool Extract(string exe, string ico) {
    IntPtr h = LoadLibraryEx(exe, IntPtr.Zero, 2); if (h == IntPtr.Zero) return false;   // LOAD_LIBRARY_AS_DATAFILE
    try {
      IntPtr first = IntPtr.Zero; bool got = false;
      EnumResourceNames(h, (IntPtr)14, (hh, t, name, l) => { first = name; got = true; return false; }, IntPtr.Zero);   // RT_GROUP_ICON
      if (!got) return false;
      byte[] grp = Get(h, first, 14); int count = BitConverter.ToUInt16(grp, 4);
      var imgs = new List<byte[]>();
      using (var ms = new MemoryStream()) using (var w = new BinaryWriter(ms)) {
        w.Write((ushort)0); w.Write((ushort)1); w.Write((ushort)count);
        int offset = 6 + 16 * count;
        for (int i = 0; i < count; i++) {
          int p = 6 + 14 * i; ushort id = BitConverter.ToUInt16(grp, p + 12); byte[] img = Get(h, (IntPtr)id, 3);   // RT_ICON
          imgs.Add(img); w.Write(grp, p, 8); w.Write((uint)img.Length); w.Write((uint)offset); offset += img.Length; }
        foreach (var img in imgs) w.Write(img);
        File.WriteAllBytes(ico, ms.ToArray());
      }
      return true;
    } finally { FreeLibrary(h); }
  }
}
'@
Add-Type -TypeDefinition $code

$owl = Get-ChildItem (Join-Path $env:LOCALAPPDATA 'owl3d-desktop-app') -Directory -Filter 'app-*' -ErrorAction SilentlyContinue |
    Sort-Object { [version]($_.Name -replace '^app-', '') } -Descending | Select-Object -First 1
$iconArg = @()
if ($owl -and (Test-Path (Join-Path $owl.FullName 'Owl3D.exe')) -and [IconExtract]::Extract((Join-Path $owl.FullName 'Owl3D.exe'), $ico)) {
    Write-Host "icon: taken from $($owl.FullName)\Owl3D.exe"
    $iconArg = @("/win32icon:$ico")
} else {
    Write-Host 'icon: Owl3D not found on this PC, building with the generic icon'
}

& $csc /nologo /nowarn:0219 /target:winexe /codepage:65001 /optimize+ @iconArg "/out:$out" $src
if ($LASTEXITCODE -ne 0) { throw "build failed ($LASTEXITCODE)" }
Write-Host "built: $out ($((Get-Item $out).Length) bytes)"
