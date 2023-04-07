using System;

namespace Shared.Scripts
{
	public static class MessageFactory
	{
		public enum MessageType : byte
		{
			BoatDirectionUpdate = 0,
		}

		public static MessageType CheckMessageType(byte[] message)
		{
			return (MessageType) message[0];
		}

		public static byte[] CreateBoatDirectionUpdate(float direction)
		{
			byte[] message = new byte[1 + sizeof(float)];
			message[0] = (byte) MessageType.BoatDirectionUpdate;

			byte[] directionBytes = BitConverter.GetBytes(direction);
			if (!BitConverter.IsLittleEndian) Array.Reverse(directionBytes);
			Array.Copy(directionBytes, 0, message, 1, sizeof(float));

			return message;
		}

		public static float DecodeBoatDirectionUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.BoatDirectionUpdate)
				throw new ArgumentException($"Message is not a {MessageType.BoatDirectionUpdate} message");

			byte[] directionBytes = new byte[sizeof(float)]; //TODO: Use Span<byte> with stackalloc instead
			Array.Copy(message, 1, directionBytes, 0, sizeof(float));
			if (!BitConverter.IsLittleEndian) Array.Reverse(directionBytes);

			return BitConverter.ToSingle(directionBytes, 0);
		}
	}
}
