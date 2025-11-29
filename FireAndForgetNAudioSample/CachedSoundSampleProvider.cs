using System;
using NAudio.Wave;

namespace FireAndForgetAudioSample
{
	public class CachedSoundSampleProvider : ISampleProvider
	{
		private readonly CachedSound cachedSound;
		private long position;
		public float 
			LeftVolume, 
			RightVolume;
		public static float[] Volume;
		public static int Index;

		public static void Initialize(int length)
		{
			Volume = new float[length];
			for (int i = 0; i < length; i++)
			{
				Volume[i] = 1f;
			}
		}

		public CachedSoundSampleProvider(CachedSound cachedSound, float volume, float pan)
		{
			this.cachedSound = cachedSound;
			LeftVolume = volume * (0.5f - pan / 2);
			RightVolume = volume * (0.5f + pan / 2);
		}

		public CachedSoundSampleProvider(CachedSound cachedSound, float volume = 1)
		{
			this.cachedSound = cachedSound;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			long availableSamples = cachedSound.AudioData.Length - position;
			long samplesToCopy = Math.Min(availableSamples, count);

			int destOffset = offset;
			for (int sourceSample = 0; sourceSample < samplesToCopy; sourceSample += 2)
			{
				float outL = cachedSound.AudioData[position + sourceSample + 0];
				float outR = cachedSound.AudioData[position + sourceSample + 1];

				buffer[destOffset + 0] = outL * Volume[Index];//LeftVolume;
				buffer[destOffset + 1] = outR * Volume[Index];//RightVolume;
				destOffset += 2;
			}

			position += samplesToCopy;
			return (int)samplesToCopy;
		}

		public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
	}
}