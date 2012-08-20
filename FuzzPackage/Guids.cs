// Guids.cs
// MUST match guids.h
using System;

namespace JaredParsons.FuzzPackage
{
    static class GuidList
    {
        public const string guidFuzzPackagePkgString = "9d1b2f4e-0b3e-4d6d-9e80-1664203637cb";
        public const string guidFuzzPackageCmdSetString = "36efd369-b640-4927-957d-d16abacf5679";

        public static readonly Guid guidFuzzPackageCmdSet = new Guid(guidFuzzPackageCmdSetString);
    };
}