// Adapted from: https://github.com/nickdu088/Unity-QR-Scanner

using System;
using System.Collections;
using Shared.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

namespace UI
{
	public class QRScanner : MonoBehaviour
	{
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private View gotoView;

		private View _parentView;

		private WebCamTexture _webcamTexture;
		private string _qrCode;
		private RawImage _image;

		private void Awake()
		{
			if (gotoView == null)
				Debug.LogError("Goto View is null");
			_parentView = transform.parent.GetComponentInParent<View>();

			_webcamTexture = new WebCamTexture(Screen.width, Screen.width);
			_image = GetComponent<RawImage>();
			_image.texture = _webcamTexture;
			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(Screen.width, Screen.width);
		}

		private void SwitchPanel()
		{
			_parentView.Hide();
			gotoView.Show();
		}

		private void OnEnable()
		{
			_qrCode = string.Empty;
			StartCoroutine(GetQRCode());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			_webcamTexture.Stop();
		}

		private IEnumerator GetQRCode()
		{
			IBarcodeReader barCodeReader = new BarcodeReader();
			_webcamTexture.Play();
			Texture2D snap = new(_webcamTexture.width, _webcamTexture.height, TextureFormat.ARGB32, false);
			while (true)
			{
				try
				{
					snap.SetPixels32(_webcamTexture.GetPixels32());
					Result result = barCodeReader.Decode(snap.GetRawTextureData(), _webcamTexture.width, _webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);
					if (result != null)
					{
						if (result.Text.Trim().StartsWith("ws"))
						{
							_qrCode = result.Text;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.Message);
				}

				yield return null;
			}

			_webcamTexture.Stop();

			inputField.text = _qrCode;
			SwitchPanel();
		}
	}
}
