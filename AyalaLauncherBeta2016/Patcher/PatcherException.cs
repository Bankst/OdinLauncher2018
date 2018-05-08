using System;

namespace AyalaLauncherBeta2016.Patcher
{
    public class PatcherException : Exception
    {

        public PatcherException()
        {
        }

        public PatcherException(string message)
            : base(message)
        {
        }
    }
}
