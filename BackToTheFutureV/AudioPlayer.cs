using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BackToTheFutureV
{
    public delegate void OnPlaybackStopped();

    public class AudioPlayer : Disposable
    {
        private static Dictionary<string, AudioPlayer> cache = new Dictionary<string, AudioPlayer>();

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private Entity currentEntity;
        private PanningSampleProvider panner;

        public bool IsLooping { get; private set; }

        public bool IsPlaying { get; private set; }

        public OnPlaybackStopped OnPlaybackStopped { get; set; }

        public AudioPlayer(string name, bool loop, float volume = 1) : base()
        {
            audioFile = new AudioFileReader(Path.Combine("./scripts/BackToTheFutureV/sounds/", name));
            audioFile.Volume = volume;
            outputDevice = new WaveOutEvent();

            IsLooping = loop;

            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
        }

        public override void Dispose()
        {
            outputDevice?.Dispose();
            audioFile?.Dispose();
        }

        public void Play()
        {
            Stop();

            IsPlaying = true;

            audioFile.Position = 0;
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }
        
        public void Play(Entity entity)
        {
            Stop();

            IsPlaying = true;

            currentEntity = entity;
            audioFile.Position = 0;
            audioFile.Volume = Utils.CalculateVolume(entity);
            panner = new PanningSampleProvider(audioFile);
            panner.Pan = Utils.CalculateStereo(entity);
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
            outputDevice.Init(panner);
            outputDevice.Play();
        }

        public void Process()
        {
            if(currentEntity != null && panner != null)
            {
                if (currentEntity is Vehicle vehicle && vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.Player.Character) return;

                float volume = Utils.CalculateVolume(currentEntity);
                float stereo = Utils.CalculateStereo(currentEntity);

                panner.Pan = stereo;
                audioFile.Volume = volume;
            }
        }

        public void Stop()
        {
            IsPlaying = false;

            audioFile.Position = 0;
            outputDevice?.Stop();

            currentEntity = null;
            panner = null;
        }

        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (IsLooping && IsPlaying)
            {
                Play();
                return;
            }

            IsPlaying = false;
            OnPlaybackStopped?.Invoke();
            outputDevice.Dispose();
        }

        public static void PlaySoundFromName(string name, out AudioPlayer player, bool loop = false, int volume = 1)
        {
            if (cache.TryGetValue(name, out player))
            {
                player.Play();
                return;
            }

            var path = "./scripts/BackToTheFutureV/sounds/";
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (extension == ".wav" && fileName == name)
                {
                    var audioPlayer = new AudioPlayer(Path.GetFileName(file), loop, volume);
                    audioPlayer.Play();

                    cache.Add(name, audioPlayer);
                }
            }
        }
    }
}
