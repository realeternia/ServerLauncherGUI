using System;

namespace DGServerControllerGUI
{
    [Flags]
    public enum LogShowMode
    {
        DEBUG = 1,
        NOTIC = 2,
        WARN = 4,
        ERROR = 8,
        FATAL = 16,
        All = DEBUG | NOTIC | WARN | ERROR | FATAL
    }

    [Flags]
    public enum LogShowDevice
    {
        LGI = 1,
        ROL = 2,
        MAY = 4,
        DBS = 8,
        GAT = 16,
        CET = 32,
        All = LGI | ROL | MAY | DBS | GAT | CET
    }

    public enum ServerRunState
    {
        Start, Running, Shutdown
    }
}
