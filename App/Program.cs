// ReSharper disable AccessToDisposedClosure : Analyser cannot prove _window.Run() blocks, but it does. When disposing in
//                                             "correct context" SIGABRT happens on other resources resulting in dirty exit.

using System.Drawing;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace App;

internal class Program
{
    private static IWindow         _window = null!;
    private static GL              _gl     = null!;
    private static IInputContext   _input  = null!;
    private static ImGuiController _imGui  = null!;
    
    private const  int BaseHeight = 160;                // Base Height of GB(C) screen in PX
    private const  int BaseWidth  = 144;                // Base Width  of GB(C) screen in PX
    
    private static int  _scale = 3;                     // The current emulator scale
    private static int? _pendingScale;                  // New scale to be applied on change
    private const  int  MenuBarReservePx = 18;          // Number of pixels to preserve for menu bar
    
    private static void Main()
    {
        // Create a Silk.NET window
        var windowOptions = WindowOptions.Default;
        
        windowOptions.WindowBorder = WindowBorder.Fixed;
        windowOptions.Size = new Vector2D<int>(
            BaseWidth  * _scale,
            BaseHeight * _scale + MenuBarReservePx);
        
        _window = Window.Create(windowOptions);
        
        // Load : Set up when window is loaded
        _window.Load += () =>
        {
            _gl    = GL.GetApi(_window);
            _input = _window.CreateInput();
            _imGui = new ImGuiController(_gl, _window, _input);
            
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            
            // Ensure viewport matches actual framebuffer size at start
            var fb = _window.FramebufferSize;
            _gl.Viewport(0, 0, (uint)fb.X, (uint)fb.Y);
            
            ApplyScale();
        };
        
        // Update : Changes to be made when window itself is modified
        _window.Update += delta =>
        {
            // Defer window resizes to avoid native re-entrancy / segfaults
            if (_pendingScale.HasValue && _pendingScale != _scale)
            {
                _scale = _pendingScale.Value;
                _pendingScale = null;
                
                ApplyScale();
            }
        }; 

        // Render : What to do when a frame is rendered
        _window.Render += delta =>
        {
            // Make sure ImGui is up-to-date
            _imGui.Update((float) delta);

            // --- Window Clear ---
            _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
            _gl.Clear((uint) ClearBufferMask.ColorBufferBit);
            
            //ImGui.ShowDemoWindow(); // Built-in demo window
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit")) _window.Close();
                   
                    ImGui.EndMenu(/*File*/);
                }
                
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.BeginMenu("Scale"))
                    {
                        ScaleItem(2);
                        ScaleItem(3);
                        ScaleItem(4);
                        ScaleItem(5);
                        
                        ImGui.EndMenu(/*Scale*/);
                    }
                    
                    ImGui.EndMenu(/*View*/);
                }
                
                ImGui.EndMainMenuBar();
            }

            _imGui.Render();
        };
        
        // Resize : Handle what happens when window size changes
        _window.FramebufferResize += size => _gl.Viewport(0, 0, (uint) size.X, (uint) size.Y);

        // Closing : Clean up on exit
        _window.Closing += () => _imGui.Dispose();

        _window.Run();          // Run runs the render loop and blocks until return
        _window.Dispose();      // At which point we can dispose the window.
    }
    
    private static void ScaleItem(int scale)
    {
        bool selected = _scale == scale;                // Set selected scale in menu
        if (ImGui.MenuItem($"{scale}x", "", selected))  // Apply the scaling
            _pendingScale = scale;
    }
    
    private static void ApplyScale()
    {
        // Resize the window to the new scale
        _window.Size = new Vector2D<int>(
            (BaseWidth  * _scale),
            (BaseHeight * _scale) + MenuBarReservePx);
        
        // Recreate ImGui controller so it picks up new framebuffer size + rebuilds device objects
        _imGui.Dispose();
        _imGui = new ImGuiController(_gl, _window, _input);
        
        // Reapply IO flag (This is a safety tweak)
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
    }
}