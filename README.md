# Owl3D 3D Brightness

Owl3D Shift에서 **Live 3D를 켰을 때 화면이 어두워지는 문제**를 보정하는 작은 트레이 프로그램입니다.
Live 3D가 켜져 있는 동안에만 밝기·감마·채도·대비를 올리고, 3D를 끄면 원래대로 되돌립니다.

A small tray tool for the Owl3D Shift display. While Owl3D Live 3D is running it raises brightness,
gamma, saturation and contrast, and restores the original picture when 3D stops.
The window is Korean on a Korean Windows and English otherwise.

Owl3D 프로그램 자체는 건드리지 않습니다. Owl3D와 관련 없는 개인 도구이며 Owl3D 회사가 만들거나 지원하는 것이 아닙니다.

[English](#english)

## 왜 필요한가

Owl3D Shift는 Live 3D를 켜면 흰색은 그대로인데 중간 밝기가 눈에 띄게 어두워집니다. 영상과 사진이 칙칙하게 보이고,
모니터 밝기를 끝까지 올려도 해결되지 않습니다. 그렇다고 Windows나 그래픽 설정에서 밝기를 올려 두면 3D를 끈 평소 화면이
너무 밝고 뿌옇게 됩니다. 이 프로그램은 **3D가 켜진 동안에만** 보정을 걸어서 두 상태를 모두 보기 좋게 유지합니다.

## 기능

| 기능 | 설명 |
|---|---|
| 3D 자동 감지 | Live 3D가 켜지면 보정을 적용하고, 꺼지면 원래 화면으로 되돌립니다. 직접 켜고 끌 필요가 없습니다. |
| 감마 | 중간 밝기를 올리거나 내립니다. 흰색과 검정은 그대로 둡니다. |
| 밝기 배율 | 화면 전체를 배수로 밝게 합니다. 검정은 검정으로 남아 뿌옇게 뜨지 않습니다. |
| 색 진하기(채도) | 색을 진하게 또는 옅게 합니다. NVIDIA 그래픽에서 동작합니다. |
| 대비 | 밝은 곳과 어두운 곳의 차이를 조절합니다. |
| 실시간 조절 | 슬라이더를 움직이면 3D 화면을 보면서 바로 결과를 확인할 수 있습니다. |
| 전역 단축키 | 영상이나 게임이 전체화면이어도 단축키로 조절합니다. 누르면 현재 값이 화면에 잠깐 표시됩니다. |
| 내 기본값 저장 | 마음에 드는 값을 기본값으로 저장해 두고, 버튼이나 단축키 하나로 되돌립니다. |
| 값 유지 | 조절한 값은 저장되어 다음 실행에도 그대로 적용됩니다. |
| 트레이 상주 | 창을 닫아도 트레이에서 계속 동작합니다. 로그인 시 자동 실행을 켤 수 있습니다. |
| 안전한 복구 | 보정이 걸린 채 PC가 꺼지거나 프로그램이 종료돼도 다음 실행 때 원래 화면으로 되돌립니다. |
| 화면 끄기 / 켜기 | Shift에는 전원 버튼이 없습니다. 단축키나 버튼으로 화면을 끄고 다시 켭니다. |
| 한국어 / 영어 | Windows 표시 언어에 맞춰 자동으로 바뀝니다. |
| 설치 불필요 | exe 파일 하나입니다. 아무 폴더에 두고 실행하면 됩니다. |

## 사용법

1. [Releases](../../releases)에서 `Owl3DBrightness.exe`를 받아 아무 폴더에나 둡니다. 설치 과정은 없습니다.
2. 실행하면 조절 창이 뜨고 트레이에 아이콘이 생깁니다. 창을 닫아도 트레이에서 계속 동작합니다.
3. Owl3D에서 Live 3D를 켜면 4~5초 뒤 보정이 적용됩니다. 끄면 2~3초 안에 원래대로 돌아옵니다.

| 항목 | 범위 | 없음 | 처음 기본값 |
|---|---|---|---|
| 감마 | 0.50 ~ 2.50 | 1.00 | 0.70 |
| 밝기 배율 | 1.00 ~ 2.00 | 1.00 | 1.40 |
| 색 진하기(채도) | 0 ~ 100 | 50 | 70 |
| 대비 | 0.50 ~ 1.50 | 1.00 | 1.00 |

- **기본값**: 저장해 둔 기본값으로 되돌립니다.
- **현재 값을 기본값으로 저장**: 지금 슬라이더 값을 기본값으로 저장합니다. 각 항목 옆 `[기본 …]`에 표시됩니다.
- **보정 끄기**: 모든 값을 "없음"으로 둡니다.
- **로그인 시 자동 실행**: exe의 현재 위치를 등록합니다. exe를 옮겼으면 다시 체크하세요.

## 단축키

어느 프로그램이 앞에 있어도 동작하고, 누르면 화면 위쪽에 현재 값이 잠깐 표시됩니다.

| 키 | 동작 |
|---|---|
| `Ctrl`+`Alt`+`B` | 조절 창 열기 / 숨기기 |
| `Ctrl`+`Alt`+`P` | Shift 화면 끄기 / 켜기 |
| `Ctrl`+`Alt`+`]` / `[` | 감마 +0.05 / −0.05 |
| `Ctrl`+`Alt`+`=` / `-` | 밝기 배율 +0.05 / −0.05 |
| `Ctrl`+`Alt`+`.` / `,` | 채도 +5 / −5 |
| `Ctrl`+`Alt`+`'` / `;` | 대비 +0.05 / −0.05 |
| `Ctrl`+`Alt`+`9` | 기본값 |
| `Ctrl`+`Alt`+`0` | 보정 끄기 |

## 알아둘 점

- **Windows 한계**: Windows는 원본에서 너무 많이 벗어난 보정을 받아주지 않습니다. 감마·밝기·대비를 함께 크게 올리면
  창 아래에 빨간 경고가 뜨고 적용되지 않습니다. 하나를 1.00 쪽으로 되돌리면 됩니다.
- **채도는 NVIDIA 그래픽에서만** 됩니다(드라이버의 디지털 바이브런스 사용). 다른 그래픽에서는 슬라이더가 비활성입니다.
- 보정은 화면 전체에 걸립니다. 영상 부분만 밝히지는 못합니다.
- 화면 끄기는 모니터에 전원 명령(DDC/CI)을 보내는 방식입니다. 꺼진 동안에는 창이 보이지 않으니 켤 때는 단축키를 쓰세요. 프로그램이 실행 중이어야 단축키가 동작하고, 안 켜지면 케이블을 뺐다 꽂으면 됩니다.
- 설정: `%LOCALAPPDATA%\Owl3D\owl3d-3d-brightness.ini`, 기록: 같은 폴더의 `owl3d-3d-brightness.log`
- 실행 옵션: `--hidden`(트레이로만 시작), `--lang en` / `--lang ko`(언어 강제)

## 동작 원리

Live 3D가 돌고 있는지는 `display-mirror.exe` 프로세스가 있는지로 판단합니다. 밝기·감마·대비는 그래픽 출력 감마 램프로,
채도는 NVIDIA 드라이버로 적용합니다. Owl3D가 전체화면 출력을 잡을 때 램프를 되돌리기 때문에, 켜져 있는 동안 2초마다
값을 확인해 다시 적용합니다. 채도를 적용한 채로 PC가 꺼져도 다음 실행 때 원래 값으로 되돌립니다.

## 빌드

Windows에 들어 있는 C# 컴파일러만 있으면 됩니다.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

결과물은 `dist\Owl3DBrightness.exe`입니다. 아이콘(`src\app.ico`)은 직접 그린 그림이고, 디자인을 바꾸려면
`tools\IconArt.cs`를 고친 뒤 `tools\make-icon.ps1`을 실행하면 다시 만들어집니다.

---

## English

**Owl3D 3D Brightness** is a small tray tool for the Owl3D Shift display. Not made or supported by Owl3D.

### Why

With Live 3D on, the Shift keeps white as it is but shows midtones noticeably darker, so video and photos look dull even
with the backlight at maximum. Raising brightness in Windows or the graphics driver instead makes the normal 2D desktop too
bright and washed out. This tool applies its correction **only while 3D is running**.

### Features

| Feature | Description |
|---|---|
| Automatic 3D detection | Applies when Live 3D starts and restores the original picture when it stops. |
| Gamma | Raises or lowers midtones; white and black stay where they are. |
| Gain | Multiplies overall brightness; black stays black. |
| Saturation | More vivid or paler colours. Works on NVIDIA graphics. |
| Contrast | Difference between light and dark. |
| Live adjustment | Move a slider and see the result on the 3D screen immediately. |
| Global hotkeys | Work while a video or game is fullscreen; the current values appear briefly on screen. |
| Your own default | Save the current values as the default preset and recall it with one button or hotkey. |
| Remembers values | Settings are kept for the next run. |
| Tray app | Keeps running after the window is closed; optional start at login. |
| Safe restore | If the PC shuts down while the correction is active, the original picture is restored at next start. |
| Screen off / on | The Shift has no power button. A hotkey or button turns its screen off and back on. |
| Korean / English | Follows the Windows display language. |
| No installer | A single exe. |

### Use

Download `Owl3DBrightness.exe` from [Releases](../../releases), put it anywhere and run it. Start Live 3D in Owl3D;
the correction is applied 4–5 seconds later and removed 2–3 seconds after 3D stops.

| Keys | Action |
|---|---|
| `Ctrl`+`Alt`+`B` | Show / hide the window |
| `Ctrl`+`Alt`+`P` | Shift screen off / on |
| `Ctrl`+`Alt`+`]` / `[` | Gamma +0.05 / −0.05 |
| `Ctrl`+`Alt`+`=` / `-` | Gain +0.05 / −0.05 |
| `Ctrl`+`Alt`+`.` / `,` | Saturation +5 / −5 |
| `Ctrl`+`Alt`+`'` / `;` | Contrast +0.05 / −0.05 |
| `Ctrl`+`Alt`+`9` | Default preset |
| `Ctrl`+`Alt`+`0` | Off |

### Notes

- Windows refuses corrections that are too far from the original. If gamma, gain and contrast are all raised a lot, the
  window shows a red warning and nothing is applied; move one of them back towards 1.00.
- Saturation needs an NVIDIA output. On other graphics the slider is disabled.
- The correction covers the whole screen, not only the video area.
- Screen off uses a monitor power command (DDC/CI). While the screen is off, use the hotkey to turn it back on; the tool must be running. If it does not come back, replug the cable.
- Settings: `%LOCALAPPDATA%\Owl3D\owl3d-3d-brightness.ini`. Options: `--hidden`, `--lang en`, `--lang ko`.
- Requires Windows 10/11 (.NET Framework 4, included with Windows).
