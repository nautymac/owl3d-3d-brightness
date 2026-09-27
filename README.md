# Owl3D 3D Brightness

Owl3D Shift에서 **Live 3D를 켰을 때 화면이 어두워지는 문제**를 보정하는 작은 트레이 프로그램입니다.
Live 3D가 켜져 있는 동안에만 밝기·감마·채도·대비를 올리고, 3D를 끄면 원래대로 되돌립니다.

A small tray tool for the Owl3D Shift display. While Owl3D Live 3D is running it raises brightness,
gamma, saturation and contrast, and restores the original picture when 3D stops.
The window is Korean on a Korean Windows and English otherwise.

Owl3D 프로그램 자체는 건드리지 않습니다. Owl3D와 관련 없는 개인 도구이며 Owl3D 회사가 만들거나 지원하는 것이 아닙니다.

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

결과물은 `dist\Owl3DBrightness.exe`입니다. 아이콘 파일은 저장소에 없고, 빌드할 때 이 PC에 설치된 Owl3D에서 가져옵니다.
Owl3D가 설치돼 있지 않으면 Windows 기본 아이콘으로 빌드됩니다.
