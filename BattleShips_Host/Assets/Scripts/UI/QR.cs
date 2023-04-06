using QRCoder;
using QRCoder.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class QR : MonoBehaviour
	{
		private RawImage _rawImage;

		private void Awake()
		{
			_rawImage = GetComponent<RawImage>();
		}

		private void OnEnable()
		{
			QRCodeGenerator qrGenerator = new();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(WebsocketServer.GetLink(), QRCodeGenerator.ECCLevel.Q);

			UnityQRCode qrCode = new(qrCodeData);
			Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(20);
			_rawImage.texture = qrCodeAsTexture2D;
		}
	}
}
