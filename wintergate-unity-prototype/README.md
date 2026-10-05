# Wintergate Quest prototype

This first Unity prototype contains the VR calendar surface plus a first playable puzzle:

- 24 doors with complete, available, and locked states
- persistent local progress
- caretaker-code modal
- `YULE-KEY` to unlock the next day
- `RAVEN-24` to unlock all days
- editor mouse input
- basic right-controller ray input on Quest
- **Door 1, "The Raven's Receipt"** — a playable Borrow & Lend puzzle. Four
  objects, one property each, three rules, three-move solution. Click Day 01
  on the calendar to open it. Solving it advances progress the same way a
  cheat code does.

Door 1 is the template for the other 23: one `string[,]` table of objects,
one start-state array, a couple of blocking rules in `PuzzleDrop`, and a win
check. The grid/card rendering, pick/drop input and completion hookup are
shared and don't need to change per door.

## First open

Run `make unity` from the parent `calendar` directory, or add this folder in Unity Hub and open it with Unity 6000.2.5f1.

Unity will resolve the VR packages and automatically create `Assets/Wintergate/WintergateCalendar.unity`. Package resolution can take several minutes on the first open.

## One-time Quest configuration

1. Open **Edit → Project Settings → XR Plug-in Management**.
2. Select the **Android** tab.
3. Enable **OpenXR**.
4. In **OpenXR → Interaction Profiles**, enable **Oculus Touch Controller Profile**.
5. Open **File → Build Profiles**, select Android/Meta Quest, and switch platform.
6. Connect the Quest with developer mode enabled.
7. Select **Build And Run**.

The scene can be tested first in the Unity editor with a mouse. The first headset goal is to see the calendar and select a door using the right controller trigger.
