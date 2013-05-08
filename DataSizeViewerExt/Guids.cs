// Guids.cs
// MUST match guids.h
using System;

namespace FourWalledCubicle.DataSizeViewerExt
{
    static class GuidList
    {
        public const string guidDataSizeViewerPkgString = "3e30bc56-2280-4652-ace4-c22cf8830515";
        public const string guidDataSizeViewerCmdSetString = "7c23d55e-1034-499b-8461-194fdc8c197a";
        public const string guidToolWindowPersistanceString = "8e77ed0e-c7f3-4464-98b8-b37b2654bd33";

        public static readonly Guid guidDataSizeViewerCmdSet = new Guid(guidDataSizeViewerCmdSetString);
    };
}