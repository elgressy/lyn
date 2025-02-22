﻿using System;
using System.Buffers;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Lyn.Types.Bitcoin;

namespace Lyn.Types.Serialization
{
    public static class SequenceReaderExtensions
    {
        private const string NOT_ENOUGH_BYTES_LEFT = "Cannot read data, not enough bytes left.";
        private const string DECODED_BIGSIZE_NOT_CANONICAL = "Decoded bigsize is not canonical.";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(ref this SequenceReader<byte> reader)
        {
            if (!reader.TryRead(out byte value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

            return value > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(ref this SequenceReader<byte> reader)
        {
            if (!reader.TryRead(out byte value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out short value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out short value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out short value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (ushort)value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out short value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (ushort)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out int value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out int value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out int value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (uint)value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out int value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (uint)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out long value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out long value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadULong(ref this SequenceReader<byte> reader, bool isBigEndian = false)
        {
            if (isBigEndian)
            {
                if (!reader.TryReadBigEndian(out long value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (ulong)value;
            }
            else
            {
                if (!reader.TryReadLittleEndian(out long value)) ThrowHelper.ThrowMessageSerializationException(NOT_ENOUGH_BYTES_LEFT);

                return (ulong)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt256 ReadUint256(ref this SequenceReader<byte> reader,bool isBigEndian = false)
        {
            var arr = reader.ReadBytes(32);

            if (isBigEndian)
            {
                arr = arr.ToArray().Reverse().ToArray();
            }
            
            return new UInt256(arr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadVarString(ref this SequenceReader<byte> reader)
        {
            ulong stringLength = ReadVarInt(ref reader);
            ReadOnlySequence<byte> result = reader.Sequence.Slice(reader.Position, (int)stringLength);
            reader.Advance((long)stringLength);

            // in case the string lies in a single span we can save a copy to a byte array
            return Encoding.ASCII.GetString(result.IsSingleSegment ? result.FirstSpan : result.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadBytes(ref this SequenceReader<byte> reader, int length)
        {
            ReadOnlySequence<byte> sequence = reader.Sequence.Slice(reader.Position, length);
            reader.Advance(length);

            if (sequence.IsSingleSegment)
            {
                return sequence.FirstSpan;
            }
            else
            {
                return sequence.ToArray();
            }
        }

        /// <summary>
        /// Reads the byte array, reading first a VarInt of the size of the array, followed by the full array data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[]? ReadByteArray(ref this SequenceReader<byte> reader)
        {
            int arraySize = (int)ReadVarInt(ref reader);

            if (arraySize == 0) return new byte[0];

            ReadOnlySequence<byte> sequence = reader.Sequence.Slice(reader.Position, arraySize);
            reader.Advance(arraySize);

            if (sequence.IsSingleSegment)
            {
                return sequence.FirstSpan.ToArray();
            }
            else
            {
                return sequence.ToArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadVarInt(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out byte firstByte);
            if (firstByte < 0xFD)
            {
                return firstByte;
            }
            else if (firstByte == 0xFD)
            {
                return reader.ReadUShort();
            }
            else if (firstByte == 0xFE)
            {
                return reader.ReadUInt();
            }
            // == 0xFF
            else
            {
                return reader.ReadULong();
            }
        }

        /// <summary>
        /// For details information on BigSize see BOLT 1
        /// https://github.com/lightningnetwork/lightning-rfc/blob/master/01-messaging.md#appendix-a-bigsize-test-vectors
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadBigSize(ref this SequenceReader<byte> reader)
        {
            byte firstByte = reader.ReadByte();

            if (firstByte < 0xFD)
            {
                return firstByte;
            }
            else if (firstByte == 0xFD)
            {
                ushort res = reader.ReadUShort(isBigEndian: true);
                if (res < firstByte) throw new MessageSerializationException(DECODED_BIGSIZE_NOT_CANONICAL);
                return res;
            }
            else if (firstByte == 0xFE)
            {
                uint res = reader.ReadUInt(isBigEndian: true);
                if (res >> 16 == 0) throw new MessageSerializationException(DECODED_BIGSIZE_NOT_CANONICAL);
                return res;
            }
            else // == 0xFF
            {
                ulong res = reader.ReadULong(isBigEndian: true);
                if (res >> 32 == 0) throw new MessageSerializationException(DECODED_BIGSIZE_NOT_CANONICAL);
                return res;
            }
        }

        /// <summary>
        /// Reads an array of <typeparamref name="TSerializableType" /> types.
        /// Internally it expects a VarInt that specifies the length of items to read.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of the serializable type.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSerializableType[] ReadArray<TSerializableType>(this ref SequenceReader<byte> reader, IProtocolTypeSerializer<TSerializableType> serializer, ProtocolTypeSerializerOptions? options = null)
        {
            ulong itemsCount = reader.ReadVarInt();

            var result = new TSerializableType[itemsCount];

            for (ulong i = 0; i < itemsCount; i++)
            {
                result[i] = serializer.Deserialize(ref reader, options);
            }

            return result;
        }

        /// <summary>
        /// Reads an item of <typeparamref name="TSerializableType" /> type using the passed typed serializer.
        /// </summary>
        /// <typeparam name="TSerializableType">The type of the serializable type.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSerializableType ReadWithSerializer<TSerializableType>(this ref SequenceReader<byte> reader, IProtocolTypeSerializer<TSerializableType> serializer, ProtocolTypeSerializerOptions? options = null)
        {
            return serializer.Deserialize(ref reader, options);
        }
    }
}