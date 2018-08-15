using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace BackToTheFutureV
{
    public class AudioPlayer
    {
        private static Dictionary<string, AudioPlayer> cache = new Dictionary<string, AudioPlayer>();

        private WaveOutEvent outputDevice;
        private AudioFileReader reader;

        public bool IsLooping { get; private set; }

        public bool IsPlaying { get; private set; }

        public AudioPlayer(string path, bool loop, int volume = 1)
        {
            outputDevice = new WaveOutEvent();
            reader = new AudioFileReader(path);
            reader.Volume = volume;

            outputDevice.Init(reader);

            IsLooping = loop;

            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
        }

        public void Play()
        {
            IsPlaying = true;

            reader.Position = 0;
            outputDevice.Play();
        }

        public void Stop()
        {
            IsPlaying = false;

            reader.Position = 0;
            outputDevice.Stop();
        }

        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if(IsLooping && IsPlaying)
            {
                reader.Position = 0;
                outputDevice.Play();
            }
        }

        public static void PlaySoundFromName(string name, out AudioPlayer player, bool loop = false, int volume = 1)
        {
            if(cache.TryGetValue(name, out player))
            {
                player.Play();
                return;
            }

            var path = "./scripts/BackToTheFutureV/sounds/";
            var files = Directory.GetFiles(path);

            foreach(var file in files)
            {
                var extension = Path.GetExtension(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                if(extension == ".wav" && fileName == name)
                {
                    var audioPlayer = new AudioPlayer(file, loop, volume);
                    audioPlayer.Play();

                    cache.Add(name, audioPlayer);
                }
            }
        }
    }
}
