using System;
using System.IO;
using System.Runtime.InteropServices;

// In the win32 DLLs longs are only 32 bit.
using Win32Long = System.Int32;
using Win32ULong = System.UInt32;

namespace JEFTDotNet
{
    internal static class FreeType
    {
        private const string Library = @"win-x64\freetype";

        public const int FT_LOAD_RENDER = (1 << 2);
        public const uint FT_OPEN_STREAM = 0x2;

        [DllImport(Library)]
        public static extern int FT_Init_FreeType(out IntPtr alibrary);

        [DllImport(Library)]
        public static extern int FT_Done_FreeType(IntPtr library);

        [DllImport(Library, EntryPoint = "FT_Open_Face")]
        private static extern int _FT_Open_Face(IntPtr library, ref FT_Open_Args args, long face_index, out IntPtr aface);

        [DllImport(Library, EntryPoint = "FT_Open_Face")]
        private static extern int _FT_Open_Face_Win32(IntPtr library, ref FT_Open_Args.Win32 args, int face_index, out IntPtr aface);

        public static int FT_Open_Face(IntPtr library, FreeTypeStreamWrapper streamWrapper, long face_index, out FT_Face aface)
        {
            var args = streamWrapper.FT_Open_ArgsWin32;
            var result = _FT_Open_Face_Win32(library, ref args, (Win32Long)face_index, out var afacePointer);
            aface = new FT_Face(afacePointer);
            return result;
        }

        [DllImport(Library, EntryPoint = "FT_Set_Pixel_Sizes")]
        private static extern int _FT_Set_Pixel_Sizes(IntPtr face, uint pixel_width, uint pixel_height);

        public static int FT_Set_Pixel_Sizes(FT_Face face, uint pixel_width, uint pixel_height)
        {
            return _FT_Set_Pixel_Sizes(face._pointer, pixel_width, pixel_height);
        }

        [DllImport(Library, EntryPoint = "FT_Load_Char")]
        private static extern int _FT_Load_Char(IntPtr face, ulong char_code, int load_flags);

        [DllImport(Library, EntryPoint = "FT_Load_Char")]
        private static extern int _FT_Load_Char_Win32(IntPtr face, Win32ULong char_code, int load_flags);

        public static int FT_Load_Char(FT_Face face, ulong char_code, int load_flags)
        {
            return _FT_Load_Char_Win32(face._pointer, (Win32ULong)char_code, load_flags);
        }

        public delegate ulong FT_Stream_IoFunc(IntPtr stream, ulong offset, IntPtr buffer, ulong count);
        public delegate Win32ULong FT_Stream_IoFunc_Win32(IntPtr stream, Win32ULong offset, IntPtr buffer, Win32ULong count);

        public delegate void FT_Stream_CloseFunc(IntPtr stream);

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Stream
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32
            {
                public IntPtr @base;
                public Win32ULong size;
                public Win32ULong pos;

                public IntPtr descriptor;
                public IntPtr pathname;
                public IntPtr read;
                public IntPtr close;

                public IntPtr memory;
                public IntPtr cursor;
                public IntPtr limit;
            }

            public IntPtr @base;
            public ulong size;
            public ulong pos;

            public IntPtr descriptor;
            public IntPtr pathname;
            public IntPtr read;
            public IntPtr close;

            public IntPtr memory;
            public IntPtr cursor;
            public IntPtr limit;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Open_Args
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32
            {
                public uint flags;
                public IntPtr memory_base;
                public Win32Long memory_size;
                public string pathname;
                public IntPtr stream;
                public IntPtr driver;
                public int num_params;
                IntPtr @params;
            }

            public uint flags;
            public IntPtr memory_base;
            public long memory_size;
            public string pathname;
            public IntPtr stream;
            public IntPtr driver;
            public int num_params;
            IntPtr @params;
        }

        // This class implements the logic for FreeType reading from .NET streams.
        public class FreeTypeStreamWrapper : IDisposable
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

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Generic
        {
            public IntPtr data;
            public IntPtr finalizier;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_BBox
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32
            {
                public Win32Long xMin;
                public Win32Long yMin;
                public Win32Long xMax;
                public Win32Long yMax;
            }

            public long xMin;
            public long yMin;
            public long xMax;
            public long yMax;
        }

        public class FT_Face
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct Rec
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct Win32
                {
                    public Win32Long num_faces;
                    public Win32Long face_index;

                    public Win32Long face_flags;
                    public Win32Long style_flags;

                    public Win32Long num_glyphs;

                    public string family_name;
                    public string style_name;

                    public int num_fixed_sizes;
                    public IntPtr available_sizes;

                    public int num_charmaps;
                    public IntPtr charmaps;

                    public FT_Generic generic;

                    public FT_BBox.Win32 bbox;

                    public ushort units_per_EM;
                    public short ascender;
                    public short descender;
                    public short height;

                    public short max_advance_width;
                    public short max_advance_height;

                    public short underline_position;
                    public short underline_thickness;

                    public IntPtr glyph;
                    public IntPtr size;
                    public IntPtr charmap;
                }
            }

            internal readonly IntPtr _pointer;

            internal FT_Face(IntPtr pointer)
            {
                _pointer = pointer;
            }

            public FT_Glyph Glyph()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return new FT_Glyph(rec.glyph);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Glyph_Metrics
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32
            {
                public Win32Long width;
                public Win32Long height;
                public Win32Long horiBearingX;
                public Win32Long horiBearingY;
                public Win32Long horiAdvance;
                public Win32Long vertBearingX;
                public Win32Long vertBearingY;
                public Win32Long vertAdvance;
            }

            public long width;
            public long height;
            public long horiBearingX;
            public long horiBearingY;
            public long horiAdvance;
            public long vertBearingX;
            public long vertBearingY;
            public long vertAdvance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Vector
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32
            {
                public Win32Long x;
                public Win32Long y;
            }

            public long x;
            public long y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_Bitmap
        {
            public uint rows;
            public uint width;
            public int pitch;
            public IntPtr buffer;
            public ushort num_grays;
            public byte pixel_mode;
            public byte palette_mode;
            public IntPtr palette;
        }

        public class FT_Glyph
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct Rec
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct Win32
                {
                    public IntPtr library;
                    public IntPtr face;
                    public IntPtr next;
                    public uint glyph_index;
                    public FT_Generic generic;

                    public FT_Glyph_Metrics.Win32 metrics;

                    public int linearHoriAdvance;
                    public int linearVertAdvance;
                    public FT_Vector.Win32 advance;

                    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                    public byte[] format;

                    public FT_Bitmap bitmap;
                    public int bitmap_left;
                    public int bitmap_top;
                }
            }

            internal readonly IntPtr _pointer;

            internal FT_Glyph(IntPtr pointer)
            {
                _pointer = pointer;
                //var format = BitConverter.GetBytes(((RecWin32)_struct).format).Select(x => (char)x).Reverse().ToArray();
            }

            public FT_Glyph_Metrics Metrics()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return new FT_Glyph_Metrics()
                {
                    width = rec.metrics.width,
                    height = rec.metrics.height,
                    horiBearingX = rec.metrics.horiBearingX,
                    horiBearingY = rec.metrics.horiBearingY,
                    horiAdvance = rec.metrics.horiAdvance,
                    vertBearingX = rec.metrics.vertBearingX,
                    vertBearingY = rec.metrics.vertBearingY,
                    vertAdvance = rec.metrics.vertAdvance,
                };
            }

            public FT_Vector Advance()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return new FT_Vector()
                {
                    x = rec.advance.x,
                    y = rec.advance.y,
                };
            }

            public FT_Bitmap Bitmap()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return rec.bitmap;
            }

            public int BitmapLeft()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return rec.bitmap_left;
            }

            public int BitmapTop()
            {
                var rec = Marshal.PtrToStructure<Rec.Win32>(_pointer);
                return rec.bitmap_left;
            }
        }
    }
}
