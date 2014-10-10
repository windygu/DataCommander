namespace DataCommander.Foundation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <exclude/>
    /// <summary>
    /// Wrapper class for some NativeMethods. API functions (KERNEL32.DLL, USER32.DLL)
    /// and constants declared in WinUser.h.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum MoveFileExFlags
        {
            /// <summary>
            /// 
            /// </summary>
            ReplaceExisiting = 0x00000001,

            /// <summary>
            /// 
            /// </summary>
            CopyAllowed = 0x00000002,

            /// <summary>
            /// 
            /// </summary>
            DelayUntilReboot = 0x00000004,

            /// <summary>
            /// 
            /// </summary>
            WriteThrough = 0x00000008,

            /// <summary>
            /// 
            /// </summary>
            CreateHardLink = 0x00000010,

            /// <summary>
            /// 
            /// </summary>
            FailIfNotTrackable = 0x00000020
        }

        [DllImport("kernel32")]
        public static extern Int16 FreeLibrary(Int32 hModule);

        [DllImport( "kernel32" )]
        public static extern UInt32 GetCurrentThreadId();

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern UInt32 GetShortPathName(
            String lpszLongPath,
            StringBuilder lpszShortPath,
            UInt32 cchBuffer);

        [DllImport("kernel32")]
        public static extern Int32 LoadLibrary(String fileName);

        [DllImport("user32")]
        public static extern Int32 LoadString(Int32 hInstance, Int32 id, byte[] buf, Int32 buflen);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpExistingFileName"></param>
        /// <param name="lpNewFileName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean MoveFileEx(
            String lpExistingFileName,
            String lpNewFileName,
            MoveFileExFlags flags);

        /// <summary>Opens a compound document file and represents it as an IStorage object</summary>
        /// <remarks>Use on Windows 2000 and earlier</remarks>
        /// <param name="pwcsName">The path and filename of the file to open</param>
        /// <param name="pstgPriority">Generally passed as null</param>
        /// <param name="grfMode">Access Method</param>
        /// <param name="snbExclude">Must be NULL</param>
        /// <param name="reserved">Pass as zero (0)</param>
        /// <param name="ppstgOpen">out IStorage</param>
        /// <returns>int HRESULT</returns>
        /// Needs more help form MS
        [DllImport("ole32")]
        public static extern Int32 StgOpenStorage(
            [MarshalAs(UnmanagedType.LPWStr)] String pwcsName,
            IStorage pstgPriority,
            STGM grfMode,
            IntPtr snbExclude,
            uint reserved,
            out IStorage ppstgOpen);   // Returned Storage
    }
}