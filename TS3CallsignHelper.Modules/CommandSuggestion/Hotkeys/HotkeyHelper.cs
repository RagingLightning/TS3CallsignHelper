﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace TS3_Callsign_Helper.Lib {
  /// <summary>
  /// Simpler way to expose key modifiers
  /// </summary>
  [Flags]
  public enum HotKeyModifiers {
    Alt = 1,        // MOD_ALT
    Control = 2,    // MOD_CONTROL
    Shift = 4,      // MOD_SHIFT
    WindowsKey = 8,     // MOD_WIN
  }

  /// <summary>
  /// Keys Enum from System.Windows.Forms for use with WPF
  /// </summary>
  public enum Hotkeys {
    A = 65,
    Add = 107,
    Alt = 262144,
    Apps = 93,
    Attn = 246,
    B = 66,
    Back = 8,
    BrowserBack = 166,
    BrowserFavorites = 171,
    BrowserForward = 167,
    BrowserHome = 172,
    BrowserRefresh = 168,
    BrowserSearch = 170,
    BrowserStop = 169,
    C = 67,
    Cancel = 3,
    Capital = 20,
    CapsLock = 20,
    Clear = 12,
    Control = 131072,
    ControlKey = 17,
    Crsel = 247,
    D = 68,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    Decimal = 110,
    Delete = 46,
    Divide = 111,
    Down = 40,
    E = 69,
    End = 35,
    Enter = 13,
    EraseEof = 249,
    Escape = 27,
    Execute = 43,
    Exsel = 248,
    F = 70,
    F1 = 112,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    F13 = 124,
    F14 = 125,
    F15 = 126,
    F16 = 127,
    F17 = 128,
    F18 = 129,
    F19 = 130,
    F2 = 113,
    F20 = 131,
    F21 = 132,
    F22 = 133,
    F23 = 134,
    F24 = 135,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    FinalMode = 24,
    G = 71,
    H = 72,
    HanguelMode = 21,
    HangulMode = 21,
    HanjaMode = 25,
    Help = 47,
    Home = 36,
    I = 73,
    IMEAccept = 30,
    IMEAceept = 30,
    IMEConvert = 28,
    IMEModeChange = 31,
    IMENonconvert = 29,
    Insert = 45,
    J = 74,
    JunjaMode = 23,
    K = 75,
    KanaMode = 21,
    KanjiMode = 25,
    KeyCode = 65535,
    L = 76,
    LaunchApplication1 = 182,
    LaunchApplication2 = 183,
    LaunchMail = 180,
    LButton = 1,
    LControlKey = 162,
    Left = 37,
    LineFeed = 10,
    LMenu = 164,
    LShiftKey = 160,
    LWin = 91,
    M = 77,
    MButton = 4,
    MediaNextTrack = 176,
    MediaPlayPause = 179,
    MediaPreviousTrack = 177,
    MediaStop = 178,
    Menu = 18,
    Modifiers = -65536,
    Multiply = 106,
    N = 78,
    Next = 34,
    NoName = 252,
    None = 0,
    NumLock = 144,
    NumPad0 = 96,
    NumPad1 = 97,
    NumPad2 = 98,
    NumPad3 = 99,
    NumPad4 = 100,
    NumPad5 = 101,
    NumPad6 = 102,
    NumPad7 = 103,
    NumPad8 = 104,
    NumPad9 = 105,
    O = 79,
    Oem1 = 186,
    Oem102 = 226,
    Oem2 = 191,
    Oem3 = 192,
    Oem4 = 219,
    Oem5 = 220,
    Oem6 = 221,
    Oem7 = 222,
    Oem8 = 223,
    OemBackslash = 226,
    OemClear = 254,
    OemCloseBrackets = 221,
    Oemcomma = 188,
    OemMinus = 189,
    OemOpenBrackets = 219,
    OemPeriod = 190,
    OemPipe = 220,
    Oemplus = 187,
    OemQuestion = 191,
    OemQuotes = 222,
    OemSemicolon = 186,
    Oemtilde = 192,
    P = 80,
    Pa1 = 253,
    Packet = 231,
    PageDown = 34,
    PageUp = 33,
    Pause = 19,
    Play = 250,
    Print = 42,
    PrintScreen = 44,
    Prior = 33,
    ProcessKey = 229,
    Q = 81,
    R = 82,
    RButton = 2,
    RControlKey = 163,
    Return = 13,
    Right = 39,
    RMenu = 165,
    RShiftKey = 161,
    RWin = 92,
    S = 83,
    Scroll = 145,
    Select = 41,
    SelectMedia = 181,
    Separator = 108,
    Shift = 65536,
    ShiftKey = 16,
    Sleep = 95,
    Snapshot = 44,
    Space = 32,
    Subtract = 109,
    T = 84,
    Tab = 9,
    U = 85,
    Up = 38,
    V = 86,
    VolumeDown = 174,
    VolumeMute = 173,
    VolumeUp = 175,
    W = 87,
    X = 88,
    XButton1 = 5,
    XButton2 = 6,
    Y = 89,
    Z = 90,
    Zoom = 251
  }

  // --------------------------------------------------------------------------
  /// <summary>
  /// A nice generic class to register multiple hotkeys for your app
  /// </summary>
  // --------------------------------------------------------------------------
  public class HotKeyHelper : IDisposable {
    // Required interop declarations for working with hotkeys
    [DllImport("user32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32", SetLastError = true)]
    public static extern int UnregisterHotKey(IntPtr hwnd, int id);
    [DllImport("user32", SetLastError = true)]
    public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
    [DllImport("kernel32", SetLastError = true)]
    public static extern short GlobalAddAtom(string lpString);
    [DllImport("kernel32", SetLastError = true)]
    public static extern short GlobalDeleteAtom(short nAtom);

    public const int WM_HOTKEY = 0x312;

    private Hotkeys Key;
    private HotKeyModifiers Mods;

    /// <summary>
    /// The unique ID to receive hotkey messages
    /// </summary>
    public short HotkeyID { get; private set; }

    /// <summary>
    /// Handle to the window listening to hotkeys
    /// </summary>
    private IntPtr _windowHandle;

    /// <summary>
    /// Callback for hot keys
    /// </summary>
    Action<int> _onHotKeyPressed;

    // --------------------------------------------------------------------------
    /// <summary>
    /// ctor
    /// </summary>
    // --------------------------------------------------------------------------

    public HotKeyHelper(Window handlerWindow, Action<int> hotKeyHandler) {
      _onHotKeyPressed = hotKeyHandler;

      // Create a unique Id for this class in this instance
      string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + this.GetType().FullName;
      HotkeyID = GlobalAddAtom(atomName);

      // Set up the hook to listen for hot keys
      _windowHandle = new WindowInteropHelper(handlerWindow).Handle;
      if (_windowHandle == null) {
        throw new ApplicationException("Cannot find window handle.  Try calling this on or after OnSourceInitialized()");
      }
      var source = HwndSource.FromHwnd(_windowHandle);
      source.AddHook(HwndHook);
    }

    // --------------------------------------------------------------------------
    /// <summary>
    /// Intermediate processing of hotkeys
    /// </summary>
    // --------------------------------------------------------------------------
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyID) {
        _onHotKeyPressed?.Invoke(lParam.ToInt32());
        handled = true;
      }
      return IntPtr.Zero;
    }

    // --------------------------------------------------------------------------
    /// <summary>
    /// Tell what key you want to listen for.  Returns an id representing
    /// this particular key combination.  Use this in your handler to
    /// disambiguate what key was pressed.
    /// </summary>
    // --------------------------------------------------------------------------
    public uint ListenForHotKey(Hotkeys key, HotKeyModifiers mods) {
      Key = key;
      Mods = mods;
      RegisterHotKey(_windowHandle, HotkeyID, (uint)mods, (uint)key);
      return (uint)mods | (((uint)key) << 16);
    }

    // --------------------------------------------------------------------------
    /// <summary>
    /// Stop listening for hotkeys
    /// </summary>
    // --------------------------------------------------------------------------
    public void StopListening() {
      if (HotkeyID != 0) {
        UnregisterHotKey(_windowHandle, HotkeyID);
        // clean up the atom list
        GlobalDeleteAtom(HotkeyID);
        HotkeyID = 0;
      }
    }

    // --------------------------------------------------------------------------
    /// <summary>
    /// Dispose
    /// </summary>
    // --------------------------------------------------------------------------
    public void Dispose() {
      StopListening();
    }

    public void SendBlockedKey() {
      UnregisterHotKey(_windowHandle, HotkeyID);
      keybd_event((byte)Hotkeys.ControlKey, 29, 0, 0); //Ctrl down
      keybd_event((byte)Hotkeys.V, 47, 0, 0); //V down
      keybd_event((byte)Hotkeys.V, 47, 2, 0); //V up
      keybd_event((byte)Hotkeys.ControlKey, 29, 2, 0); //Ctrl up
      RegisterHotKey(_windowHandle, HotkeyID, (uint)Mods, (uint)Key);
    }
  }
}
