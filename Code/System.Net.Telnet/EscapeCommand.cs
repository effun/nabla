namespace System.Net.Telnet
{
    // http://www.termsys.demon.co.uk/vtansi.htm

    enum EscapeCommand
    {
        Unknown,
        CursorHome,
        CursorUp,
        CursorDown,
        CursorForward,
        CursorBackward,
        CursorForce,
        CursorSave,
        CursorUnsave,
        CursorSaveAttrs,
        CursorRestoreAttrs,
        ScrollScreen,
        ScrollDown,
        ScrollUp,
        TabSet,
        TabUnset,
        TabClear,
        ClearLineFromCursor,
        ClearLineToCursor,
        ClearEntireLine,
        ClearScreenFromCursor,
        ClearScreenToCursor,
        ClearScreen
    }

}
