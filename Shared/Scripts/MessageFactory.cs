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
			RequestDockingStatusUpdate,
			IsDockedUpdate,
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
			Array.Copy(blowingBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeBlowingUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.BlowingUpdate)
				throw new ArgumentException($"Message is not a {MessageType.BlowingUpdate} message");

			byte[] blowingBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, blowingBytes, 0, sizeof(bool));

			return BitConverter.ToBoolean(blowingBytes, 0);
		}
#endregion

#region DockingAvailableUpdate
		public static byte[] CreateDockingAvailableUpdate(bool dockingAvailable)
		{
			byte[] message = new byte[1 + sizeof(bool)];
			message[0] = (byte) MessageType.DockingAvailableUpdate;

			byte[] dockingAvailableBytes = BitConverter.GetBytes(dockingAvailable);
			Array.Copy(dockingAvailableBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeDockingAvailableUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.DockingAvailableUpdate)
				throw new ArgumentException($"Message is not a {MessageType.DockingAvailableUpdate} message");

			byte[] dockingAvailableBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, dockingAvailableBytes, 0, sizeof(bool));

			return BitConverter.ToBoolean(dockingAvailableBytes, 0);
		}
#endregion

#region RequestDockingUpdate
		/// <param name="requestDocking"><b>True</b> for Docking, <b>False</b> for Undocking</param>
		public static byte[] CreateDockingStatusUpdate(bool requestDocking)
		{
			byte[] message = new byte[1 + sizeof(bool)];
			message[0] = (byte) MessageType.RequestDockingStatusUpdate;

			byte[] dockingStatusBytes = BitConverter.GetBytes(requestDocking);
			Array.Copy(dockingStatusBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeDockingStatusUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.RequestDockingStatusUpdate)
				throw new ArgumentException($"Message is not a {MessageType.RequestDockingStatusUpdate} message");

			byte[] dockingStatusBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, dockingStatusBytes, 0, sizeof(bool));

			return BitConverter.ToBoolean(dockingStatusBytes, 0);
		}
#endregion

#region IsDockedUpdate
		public static byte[] CreateIsDockedUpdate(bool isDocked)
		{
			byte[] message = new byte[1 + sizeof(bool)];
			message[0] = (byte) MessageType.IsDockedUpdate;

			byte[] isDockedBytes = BitConverter.GetBytes(isDocked);
			Array.Copy(isDockedBytes, 0, message, 1, sizeof(bool));

			return message;
		}

		public static bool DecodeIsDockedUpdate(byte[] message)
		{
			if (CheckMessageType(message) != MessageType.IsDockedUpdate)
				throw new ArgumentException($"Message is not a {MessageType.IsDockedUpdate} message");

			byte[] isDockedBytes = new byte[sizeof(bool)];
			Array.Copy(message, 1, isDockedBytes, 0, sizeof(bool));

			return BitConverter.ToBoolean(isDockedBytes, 0);
		}
#endregion
	}
}
