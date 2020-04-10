using System;
using System.IO;
using System.Runtime.InteropServices;
using static JEFTDotNet.FreeType;

// In the win32 DLLs longs are only 32 bit.
using Win32ULong = System.UInt32;

namespace JEFTDotNet
{
    // This class implements the logic for FreeType reading from .NET streams.
    internal class FreeTypeStreamWrapper : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _startOffset;

        private readonly object _ftStream;
        private readonly GCHandle _ftStreamHandle;

        public FT_Open_Args.Win32 FT_Open_ArgsWin32 { get; }

        // The properties ensure the delegates don't disappear as long as the StreamWrapper exists.
        private FT_Stream_IoFunc IoFunc => StreamRead;
        private FT_Stream_IoFunc_Win32 IoFuncWin32 => StreamReadWin32;
        private FT_Stream_CloseFunc CloseFunc => StreamClose;

        public FreeTypeStreamWrapper(Stream stream)
        {
            _stream = stream;
            _startOffset = stream.Position;

            var ftStream = new FT_Stream.Win32()
            {
                @base = IntPtr.Zero,
                pos = 0,
                size = 0x7FFFFFFF, // Tells FreeType the size of the stream is unknown
                read = Marshal.GetFunctionPointerForDelegate(IoFuncWin32),
                close = Marshal.GetFunctionPointerForDelegate(CloseFunc)
            };

            _ftStream = ftStream;
            _ftStreamHandle = GCHandle.Alloc(_ftStream, GCHandleType.Pinned);

            var ftOpenArgs = new FT_Open_Args.Win32()
            {
                flags = FT_OPEN_STREAM,
                stream = _ftStreamHandle.AddrOfPinnedObject()
            };

            FT_Open_ArgsWin32 = ftOpenArgs;
        }

        public void Dispose()
        {
            _stream.Seek(_startOffset, SeekOrigin.Begin);
            _ftStreamHandle.Free();
        }

        private ulong StreamRead(IntPtr _, ulong offset, IntPtr buffer, ulong count)
        {
            _stream.Seek(_startOffset + (long)offset, SeekOrigin.Begin);

            if (count == 0)
                return 0;

            var temp = new byte[count];
            var read = _stream.Read(temp, 0, (int)count);
            Marshal.Copy(temp, 0, buffer, read);

            return (ulong)read;
        }

        private Win32ULong StreamReadWin32(IntPtr _, Win32ULong offset, IntPtr buffer, Win32ULong count)
        {
            return (Win32ULong)StreamRead(_, offset, buffer, count);
        }

        private void StreamClose(IntPtr _)
        {
            // We can just ignore this one.
        }
    }
}
