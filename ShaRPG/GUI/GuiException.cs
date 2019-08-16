using System;

namespace ShaRPG.GUI
{
    public class GuiException : Exception {
        public GuiException(string message) : base(message) { }
    }
}
