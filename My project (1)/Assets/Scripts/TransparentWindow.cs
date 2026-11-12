using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_EX_TOPMOST = 0x00000008;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    void Start()
    {
        // This only works in a built Windows executable, not the Unity Editor
        #if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();

        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // Set window to be borderless, layered (transparent), and always on top
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);

        // Force window to update position to apply Topmost
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0x0001 | 0x0002); // SWP_NOSIZE | SWP_NOMOVE
        #endif
        
        // Ensure Unity's background is transparent
        Application.runInBackground = true;
    }
}
