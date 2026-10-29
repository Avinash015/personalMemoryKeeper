# personalMemoryKeeper

Android AR app (Unity 2022.3.43f1, AR Foundation 5.1 + ARCore) for pinning personal "memories"
(photos and videos) to real-world locations. A location is identified by scanning a QR code; memories
are uploaded to and fetched from a small HTTP backend and placed in AR on detected planes.

## Scene flow (Build Settings order)

| # | Scene | Purpose |
| --- | --- | --- |
| 0 | `Assets/Scenes/LoginScene.unity` | email/password login and sign-up against the backend (`LoginAndSignUp.cs`) |
| 1 | `Assets/Scenes/QRScannerScene.unity` | continuous QR scan with the AR camera to pick the location (`QRScanner.cs`, ZXing) |
| 2 | `Assets/Scenes/uploadRetrieve.unity` | pick an image/video from the gallery (NativeGallery), upload it, list saved memories (`UploadMemory.cs`, `UploadVideo.cs`, `ViewMemories.cs`) |
| 3 | `Assets/Scenes/startanimation.unity` | intro animation |
| 4 | `Assets/Scenes/getstarted.unity` | onboarding screen |
| 5 | `Assets/Scenes/demo.unity` | AR placement demo: tap a plane to place, drag, resize, save position (`ARObjectPlacer.cs`, `Draggable.cs`, `ResizableBox.cs`, `SavePositionManager.cs`) |

## Backend

The app expects a REST backend on the local network, port 8000. The base URL is hardcoded in several
scripts and must be changed to your server before building:

- `Assets/Scripts/LoginAndSignUp.cs` (`baseURL`, `/api/users/`)
- `Assets/Scripts/UploadMemory.cs`, `UploadVideo.cs`, `SavePositionManager.cs`, `ViewMemories.cs` (`/api/memories/...`, `/api/videos/upload`)
- `Assets/demoscripts/ImagePlacerdemo.cs`

The backend itself is not part of this repository.

## How to run

1. Install Unity Hub and Unity **2022.3.43f1** with the *Android Build Support* module (OpenJDK, SDK and NDK).
2. Clone this repository and open the folder in Unity Hub (Add > Add project from disk). First open takes
   several minutes while the `Library` folder is regenerated.
3. Update the backend URLs listed above.
4. File > Build Settings > Android > Switch Platform, then *Build And Run* with an ARCore-capable phone
   connected over USB (USB debugging enabled). AR scenes do not work in the editor Play mode.

## Packages

AR Foundation 5.1.6, ARCore XR Plugin 5.1.6, TextMeshPro 3.0.6, ZXing (bundled), NativeGallery (bundled).
Everything is restored automatically from `Packages/manifest.json` on first open.
