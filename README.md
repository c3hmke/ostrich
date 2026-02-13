# Ostrich emulator

A simple Game Boy emulator written in C# using SDL2 and ImGUI.

### Display

The display uses only integer scaling in relation to the emulators display buffer. This is so that the rendered image
better reflects the hardware being emulated.

Window Pipeline:
```
    Window
    ├─ Padding
    │   └─ Content Area
    │       └─ Integer-scaled emulator image
    └─ ImGui overlay
```

Padding has been added around the content area. This was present on original hardware and games would draw to the edge
of the screen with the knowledge that the padding was there.

### Project Structure

App (FrontEnd), it holds knowledge of Silk.NET, OpenGL, ImGui and is generally responsible for HCI tasks.

Emulation (Contracts), is an interface layer defining what should be exposed.

GameBoy (Core), implements the contracts and holds knowledge of the emulated hardware.

#### Reference Graph

Ostrich.App       ──▶  Ostrich.Emulation
Ostrich.GameBoy   ──▶  Ostrich.Emulation