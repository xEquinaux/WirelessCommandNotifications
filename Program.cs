using InputLib;
using Microsoft.VisualBasic.Devices;
using NAudio.CoreAudioApi;
using NAudio.Mixer;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using REWD;
using REWD.D2D;
using REWD.FoundationR;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Numerics;
using tUserInterface.ModUI;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using FireAndForgetAudioSample;

namespace CommandNotifications
{
	internal class Program
	{
		static void Main(string[] args)
		{
			new Game(800, 400);
		}
	}

	public class Game(int width, int height) : Direct2D(width, height)
	{
		nint WindowHandle;

		System.Drawing.Bitmap? surface;
		Graphics? graphics;
		BufferedGraphics? buffer;
		BufferedGraphicsContext context = BufferedGraphicsManager.Current;
		Rectangle rect => new Rectangle(0, 0, Width, Height);
		System.Drawing.Color clearColor = Color.CornflowerBlue;

		int Width = 800, Height = 400;
		int Length => _sound.Count;
		bool clicked2 = false;
		bool init = false;
		bool[] clicked = new bool[] { };
		internal static Point Mouse;
		HorizontalScroll? scroll;
		IList<string> _sound = new List<string>();
		IList<Meter> Meters = new List<Meter>();
		IList<Button> Buttons = new List<Button>();

		IList<CachedSound> Sound = new List<CachedSound>();
		WaveOut? play;
		//MixingWaveProvider32 _mixer = new MixingWaveProvider32();
		MixingSampleProvider? mixer;
		MMDevice? device;
		WasapiOut? output;
		AudioPlaybackEngine? Instance;

		public override void Initialize()
		{
			var wf = (device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)).AudioClient.MixFormat;

			Mp3FileReader mp3 = null;
			WaveFileReader wfr = null;
			Wave16ToFloatProvider wave = null;
			MultiplexingWaveProvider msw = null;
			WaveFormatConversionProvider wfc = null;
			ConcatenatingSampleProvider csp = null;
			Pcm8BitToSampleProvider pcm8 = null;
			Pcm16BitToSampleProvider pcm16 = null;
			Pcm24BitToSampleProvider pcm24 = null;
			Pcm32BitToSampleProvider pcm32 = null;
			VolumeWaveProvider16 vwp = null;

			ISampleProvider audio = null;

			Volume += AudioVolume;
			scroll = new HorizontalScroll(new Rectangle(0, 0, Width, Height));
			foreach (string file in Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "sounds")).Where(t => Path.HasExtension(".wav") || Path.HasExtension(".mp3")))
			{
				int length = 0;
				if (file.EndsWith(".mp3"))
				{
					using (mp3 = new Mp3FileReader(file))
					{
						length = (int)mp3.Length;
						IWaveProvider iwp = null;
						try
						{
							iwp = new MultiplexingWaveProvider(new[] { mp3 }, 2);
						}
						catch
						{
							iwp = wfr;
						}
						switch (mp3.WaveFormat.BitsPerSample)
						{
							case 8:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm8 = new Pcm8BitToSampleProvider(wfc);
								audio = pcm8;
								goto default;
							case 16:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm16 = new Pcm16BitToSampleProvider(wfc);
								audio = pcm16;
								goto default;
							case 24:
								pcm24 = new Pcm24BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm24.ToWaveProvider())).Reposition();
								audio = pcm24;
								goto default;
							case 32:
								var w = new WaveFloatTo16Provider(iwp);
								//pcm32 = new Pcm32BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm32.ToWaveProvider())).Reposition();
								audio = w.ToSampleProvider();
								goto default;
							default:
								break;
						}
					}
				}
				else if (file.EndsWith(".wav"))
				{
					using (wfr = new WaveFileReader(file))
					{
						length = (int)wfr.Length;
						IWaveProvider iwp = null;
						try
						{
							iwp = new MultiplexingWaveProvider(new[] { wfr }, 2);
						}
						catch
						{
							iwp = wfr;
						}
						switch (wfr.WaveFormat.BitsPerSample)
						{
							case 8:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm8 = new Pcm8BitToSampleProvider(wfc);
								audio = pcm8;
								goto default;
							case 16:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm16 = new Pcm16BitToSampleProvider(wfc);
								audio = pcm16;
								goto default;
							case 24:
								pcm24 = new Pcm24BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm24.ToWaveProvider())).Reposition();
								audio = pcm24;
								goto default;
							case 32:
								var w = new WaveFloatTo16Provider(iwp);
								//pcm32 = new Pcm32BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm32.ToWaveProvider())).Reposition();
								audio = w.ToSampleProvider();
								goto default;
							default:
								break;
						}
					}
				}
				Sound.Add(
					new CachedSound(file)
				);
				Meters.Add(new Meter());
				_sound.Add(file);
				Buttons.Add(new Button());
			}
			CachedSoundSampleProvider.Initialize(Length);
			clicked = new bool[Length];
			play = new WaveOut();
		}
		public SampleChannel GetSoundObject(string file)
		{
			var wf = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).AudioClient.MixFormat;

			Mp3FileReader mp3 = null;
			WaveFileReader wfr = null;
			Wave16ToFloatProvider wave = null;
			MultiplexingWaveProvider msw = null;
			WaveFormatConversionProvider wfc = null;
			ConcatenatingSampleProvider csp = null;
			Pcm8BitToSampleProvider pcm8 = null;
			Pcm16BitToSampleProvider pcm16 = null;
			Pcm24BitToSampleProvider pcm24 = null;
			Pcm32BitToSampleProvider pcm32 = null;
			VolumeWaveProvider16 vwp = null;

			ISampleProvider audio = null;

			try
			{
				if (file.EndsWith(".mp3"))
				{
					using (mp3 = new Mp3FileReader(file))
					{
						mp3.Seek(0, SeekOrigin.Begin);
						IWaveProvider iwp = null;
						try
						{
							iwp = new MultiplexingWaveProvider(new[] { mp3 }, 2);
						}
						catch
						{
							iwp = wfr;
						}
						switch (mp3.WaveFormat.BitsPerSample)
						{
							case 8:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm8 = new Pcm8BitToSampleProvider(wfc);
								audio = pcm8;
								goto default;
							case 16:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm16 = new Pcm16BitToSampleProvider(wfc);
								audio = pcm16;
								goto default;
							case 24:
								pcm24 = new Pcm24BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm24.ToWaveProvider())).Reposition();
								audio = pcm24;
								goto default;
							case 32:
								var w = new WaveFloatTo16Provider(iwp);
								//pcm32 = new Pcm32BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm32.ToWaveProvider())).Reposition();
								return new SampleChannel(w);
							default:
								break;
						}
					}
				}
				else if (file.EndsWith(".wav"))
				{
					using (wfr = new WaveFileReader(file))
					{
						wfr.Seek(0, SeekOrigin.Begin);
						IWaveProvider iwp = null;
						try
						{
							iwp = new MultiplexingWaveProvider(new[] { wfr }, 2);
						}
						catch
						{
							iwp = wfr;
						}
						switch (wfr.WaveFormat.BitsPerSample)
						{
							case 8:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm8 = new Pcm8BitToSampleProvider(wfc);
								audio = pcm8;
								goto default;
							case 16:
								(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), iwp)).Reposition();
								pcm16 = new Pcm16BitToSampleProvider(wfc);
								audio = pcm16;
								goto default;
							case 24:
								pcm24 = new Pcm24BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm24.ToWaveProvider())).Reposition();
								audio = pcm24;
								goto default;
							case 32:
								var w = new WaveFloatTo16Provider(iwp);
								//pcm32 = new Pcm32BitToSampleProvider(iwp);
								//(wfc = new WaveFormatConversionProvider(new WaveFormat(wf.SampleRate, 16, 2), pcm32.ToWaveProvider())).Reposition();
								return new SampleChannel(w);
							default:
								break;
						}
					}
				}
				return
					new SampleChannel(audio.ToWaveProvider());
			}
			catch { return null; }
		}
		public override void LoadResources()
		{
			WindowHandle = Utility.FindWindowByCaption(IntPtr.Zero, "Main Window");
		}
		public override void Update()
		{
			if (!init)
			{
				init = true;
				var wf = (device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)).AudioClient.MixFormat;
				Instance = new AudioPlaybackEngine(wf.SampleRate, wf.Channels);
				var ieee = WaveFormat.CreateIeeeFloatWaveFormat(wf.SampleRate, 2);
				mixer = new MixingSampleProvider(ieee);
				mixer.ReadFully = true;
			}
			Mouse = Input.MouseScreen(WindowHandle);
			int _yCoord = 10;
			if (scroll != null && clicked.All(t => t == false))
			{
				HorizontalScroll.DirectMouseInteract(scroll, Mouse, Input.IsLeftPressed());
			}
			for (int i = 0; i < Length; i++)
			{
				if (!scroll.clicked && clicked.All(t => t == false) && Meters[i].Hitbox.Contains(Mouse) && Input.IsLeftPressed())
					clicked[i] = true;
				if (!Input.IsLeftPressed())
					clicked[i] = false;
				if (clicked[i])
				{
					if (Mouse.Y >= 10 && Mouse.Y <= 360)
					{
						Meters[i].Weight = Math.Abs((Mouse.Y - _yCoord) / (float)(Height - _yCoord * 2) - 1f);
						Volume?.Invoke(this, new VolumeEventArgs()
						{
							Audio = Sound[i],
							Index = i,
							Volume = Math.Min(1f, Math.Max(0f, Meters[i].Weight))
						});
					}
				}
				if (!clicked2 && !scroll.clicked && clicked.All(t => t == false) && Buttons[i].Clicked())
				{
					clicked2 = true;

					/* 
					 * Duplicating the selecting audio file is not happening. I'll look into cached sounds I guess.
					 */

					//var sound = GetSoundObject(_sound[i]);
					//if (sound == null) break;
					/*
					byte[] buffer = new byte[8192];
					BufferedWaveProvider bwp = new BufferedWaveProvider(sound.WaveFormat);
					int read = 0;
					while ((read = sound.Read(buffer, 0, buffer.Length)) > 0)
					{
						bwp.AddSamples(buffer, 0, read);
					}
					RawSourceWaveStream raw = new RawSourceWaveStream(buffer, 0, buffer.Length, new WaveFormat());
					 */

					//using (WaveOutEvent play = new WaveOutEvent())
					//{
					//	play.Init(Sound[i]);
					//	play.Play();
					//	while (play.PlaybackState == PlaybackState.Playing)
					//	{
					//		Task.WaitAll(Task.Delay(200));
					//	}
					//}

					CachedSoundSampleProvider.Index = i;
					Instance?.PlaySound(Sound[i]);
					break;

					/* 
					 * Using a mixer causes dilemmas
					 */

					//NAudio.MmException: 'AcmNotPossible calling acmStreamOpen'
					//var a = new WaveFormatConversionProvider(new WaveFormat(), bwp);
					//mixer.AddInputStream(a); //System.ArgumentException: 'Must be IEEE floating point (Parameter 'waveProvider.WaveFormat')'
				}
			}
			if (!Input.IsLeftPressed())
			{
				clicked2 = false;
			}
		}
		public override void Draw(DeviceContext rt)
		{
			BeginDraw();
			Graphics g = buffer.Graphics;
			buffer?.Graphics.Clear(Color.Black);
			int _width = 180;
			int _width2 = _width / 6;
			int _xCoord = 0;
			int _yCoord = 20;
			for (int i = 0; i < Length; i++)
			{
				int xCoord = i * _xCoord;
				int _height = Height - _yCoord * 2;
				int x = (int)(xCoord + i * _width - Length * _width * scroll.value);
				int y = _yCoord + (int)(Math.Abs(Meters[i].Weight - 1f) * _height);
				Meters[i].Hitbox = new Rectangle(x + _width / 3, y, _width2, Height - _yCoord * 2 - y);
				g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), new Rectangle(x, 0, _width - _width2, Height));
				g.FillRectangle(Brushes.Green, Meters[i].Hitbox);
				g.DrawString(Math.Round(Meters[i].Weight, 2).ToString(), new Font("Helvetica", 10f), Brushes.White, new Point(Meters[i].Hitbox.Left, Meters[i].Hitbox.Top - 10));
				g.DrawString(Path.GetFileNameWithoutExtension(_sound[i]), new Font("Helvetica", 11f), Brushes.White, new Point(x, Height - _yCoord * 2));
				Buttons[i].Initialize(x + _yCoord, _yCoord);
				Buttons[i].Update(x + _yCoord / 2, _yCoord / 2);
				Buttons[i].Draw(g);
			}
			g.FillRectangle(Brushes.Gray, new Rectangle(0, Height - _yCoord / 2, Width, _yCoord / 2));
			scroll?.Draw(g, Brushes.White);
			Render(rt);
			EndDraw();
		}
		public void BeginDraw()
		{
			surface = new System.Drawing.Bitmap(Width, Height);
			graphics = Graphics.FromImage(surface);
			buffer = context.Allocate(graphics, rect);
		}
		public void Render(DeviceContext rt)
		{
			buffer?.Render();
			var bmp = ConvertBitmap(surface, rt);
			rt.DrawBitmap(bmp, 1f, BitmapInterpolationMode.NearestNeighbor);
			surface.Dispose();
			bmp.Dispose();
		}
		public void EndDraw()
		{
			buffer?.Dispose();
			graphics?.Dispose();
			surface?.Dispose();
		}
		public SharpDX.Direct2D1.Bitmap ConvertBitmap(System.Drawing.Bitmap bitmap, SharpDX.Direct2D1.DeviceContext deviceContext)
		{
			var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties(
				new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));

			var bitmapData = bitmap.LockBits(
				new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

			using (var dataStream = new SharpDX.DataStream(bitmapData.Scan0, bitmapData.Stride * bitmap.Height, true, false))
			{
				var sharpDxBitmap = new SharpDX.Direct2D1.Bitmap(deviceContext, new Size2(bitmap.Width, bitmap.Height), dataStream, bitmapData.Stride, bitmapProperties);
				bitmap.UnlockBits(bitmapData);
				return sharpDxBitmap;
			}
		}

		private void AudioVolume(object? sender, VolumeEventArgs e)
		{
			if (e.Audio != null)
			{
				CachedSoundSampleProvider.Volume[e.Index] = e.Volume;
			}
		}

		public event EventHandler<VolumeEventArgs>? Volume;
		public class VolumeEventArgs : EventArgs
		{
			public float Volume;
			public int Index;
			public CachedSound? Audio;
		}
	}
	class Meter
	{
		public float Weight = 1f;
		public Rectangle Hitbox;
	}
	class Button
	{
		public bool active = true;
		public bool init = false;
		public readonly string text = "Play";
		public int
			width = 40,
			height = 24,
			margin = 2;
		public System.Drawing.Brush
			one = Brushes.Gray,
			two = Brushes.LightBlue,
			three = Brushes.White;
		private Font font = new Font("Helvetica", 10f);
		public Point
			position,
			drawText;
		public Rectangle Hitbox => new Rectangle(position.X, position.Y, width, height);
		public Rectangle Margin => new Rectangle(position.X - margin, position.Y - margin, width + margin * 2, height + margin * 2);
		public bool Clicked() => Hover() && Input.IsLeftPressed();
		public bool Hover() => Hitbox.Contains(Game.Mouse);
		public void Initialize(int x, int y, int margin = 3)
		{
			if (!init)
			{
				init = true;
				position = new Point(x, y);
				drawText = new Point(x + margin, y + margin);
			}
		}
		public void Update(int x, int y)
		{
			position = new Point(x, y);
			drawText = new Point(x + margin, y + margin);
		}
		public void Draw(Graphics g)
		{
			g.FillRectangle(three, Margin);
			g.FillRectangle(Hover() ? two : one, Hitbox);
			g.DrawString(text, font, Brushes.Black, drawText);
		}
	}
}