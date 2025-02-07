using DPUruNet;
using System.Text.RegularExpressions;

namespace FingerPrintApplication
{
    public record FingerDto
    {
        public string SavedFinger { get; init; }
        public string UserFinger { get; init; }
    }
}
