using System;

namespace Shared.Scripts
{
	public static class MessageFactory
	{
		// Updates take in a parameter, while Signals do not. And thus they also don't need to be decoded,
		// as they are by themselves already the whole content of the message.

		public enum MessageType : byte
		{
			BoatDirectionUpdate,
			StartGameSignal,
			GoBackToLobbySignal,
			BlowingUpdate,
			DockingAvailableUpdate,
		}

		public static MessageType CheckMessageType(byte[] message)
		{
			return (MessageType) message[0];
		}

		public static byte[] CreateSignal(MessageType messageType)
		{
			byte[] message = new byte[1];
			message[0] = (byte) messageType;

			return message;
		}

		//TODO: Use Span<byte> with stackalloc instead of byte[] wherever possible in this class

		//TODO: Use a generic method for all the Create/Decode methods

#region BoatDirectionUpdate
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

			byte[] directionBytes = new byte[sizeof(float)];
			Array.Copy(message, 1, directionBytes, 0, sizeof(float));
			if (!BitConverter.IsLittleEndian) Array.Reverse(directionBytes);

			return BitConverter.ToSingle(directionBytes, 0);
		}
#endregion

#region BlowingUpdate
		public static byte[] CreateBlowingUpdate(bool blowing)
		{
			byte[] message = new byte[1 + sizeof(bool)];
			message[0] = (byte) MessageType.BlowingUpdate;

			byte[] blowingBytes = BitConverter.GetBytes(blowing);
			if (!BitConverter.IsLittleEndian) Array.Reverse(blowingBytes);
			Array.Copy(blowingBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeBlowingUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.BlowingUpdate)
				throw new ArgumentException($"Message is not a {MessageType.BlowingUpdate} message");

			byte[] blowingBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, blowingBytes, 0, sizeof(bool));
			if (!BitConverter.IsLittleEndian) Array.Reverse(blowingBytes);

			return BitConverter.ToBoolean(blowingBytes, 0);
		}
#endregion

#region DockingAvailableUpdate
		public static byte[] CreateDockingAvailableUpdate(bool dockingAvailable)
		{
			byte[] message = new byte[1 + sizeof(bool)];
			message[0] = (byte) MessageType.DockingAvailableUpdate;

			byte[] dockingAvailableBytes = BitConverter.GetBytes(dockingAvailable);
			if (!BitConverter.IsLittleEndian) Array.Reverse(dockingAvailableBytes);
			Array.Copy(dockingAvailableBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeDockingAvailableUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.DockingAvailableUpdate)
				throw new ArgumentException($"Message is not a {MessageType.DockingAvailableUpdate} message");

			byte[] dockingAvailableBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, dockingAvailableBytes, 0, sizeof(bool));
			if (!BitConverter.IsLittleEndian) Array.Reverse(dockingAvailableBytes);

			return BitConverter.ToBoolean(dockingAvailableBytes, 0);
		}
#endregion
	}
}
