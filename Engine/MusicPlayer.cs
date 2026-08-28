using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CupheadMusicPlayer.Engine
{
    public class MusicPlayer : IDisposable
    {
        private const int StartDelayMs = 1000;
        private const int FadeMs = 1500;

        private WaveOutEvent output;
        private AudioFileReader audioFile;
        private LoopStream loopStream;
        private float volume = 1.0f;

        private readonly object sync = new object();
        private string desiredFile;
        private int desiredDelayMs = StartDelayMs;
        private int desiredHoldMs;
        private int desiredFadeMs = FadeMs;
        private bool workerBusy;
        private CancellationTokenSource cancel;

        public float Volume
        {
            get => volume;
            set
            {
                volume = Math.Max(0f, Math.Min(1f, value));

                lock (sync)
                {
                    // Don't fight an in-progress fade; the fade reads the target volume.
                    if (!workerBusy && audioFile != null)
                    {
                        try { audioFile.Volume = volume; } catch { }
                    }
                    try { if (output != null) output.Volume = 1f; } catch { }
                }
            }
        }

        public string CurrentFile { get; private set; }

        public bool IsPlaying
            => output != null && output.PlaybackState == PlaybackState.Playing;

        public void PlayLooping(string filePath, int startDelayMs = StartDelayMs, int holdMs = 0, int fadeMs = FadeMs)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            lock (sync)
            {
                // Don't restart the same song if it is already playing.
                if (string.Equals(CurrentFile, filePath, StringComparison.OrdinalIgnoreCase) && IsPlaying)
                    return;

                desiredFile = filePath;
                desiredDelayMs = startDelayMs;
                desiredHoldMs = Math.Max(0, holdMs);
                desiredFadeMs = Math.Max(0, fadeMs);
            }

            Kick();
        }

        public void Stop()
        {
            lock (sync) desiredFile = null;
            Kick();
        }

        private void Kick()
        {
            lock (sync)
            {
                if (workerBusy) return; // running worker will pick up the latest desired file

                workerBusy = true;

                cancel?.Dispose();
                cancel = new CancellationTokenSource();
            }

            _ = ProcessAsync(cancel.Token);
        }

        private async Task ProcessAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        StopCore();
                        break;
                    }

                    string target;
                    string current;
                    bool playing;
                    int delayMs;
                    int holdMs;
                    int fadeMs;
                    lock (sync)
                    {
                        target = desiredFile;
                        current = CurrentFile;
                        playing = IsPlaying;
                        delayMs = desiredDelayMs;
                        holdMs = desiredHoldMs;
                        fadeMs = desiredFadeMs;
                    }

                    bool sameFile =
                        playing &&
                        string.Equals(current, target, StringComparison.OrdinalIgnoreCase);

                    if (sameFile)
                        break;

                    if (playing)
                    {
                        // Different target (or stopping). If a hold was requested,
                        // let the current song keep playing for a bit before the
                        // fade-out (used to linger music when leaving a level).
                        if (holdMs > 0 && !string.IsNullOrEmpty(target))
                        {
                            try { await PlayCurrentForAsync(holdMs, token); }
                            catch (OperationCanceledException) { StopCore(); break; }

                            if (token.IsCancellationRequested)
                                break;
                        }

                        // Fade out what's playing.
                        await FadeOutAsync(token, fadeMs);
                        lock (sync) StopCore();
                        continue;
                    }

                    // Nothing is playing right now.
                    if (string.IsNullOrEmpty(target))
                        break; // requested stop and nothing is playing

                    // Delay before starting music.
                    try { await Task.Delay(delayMs, token); }
                    catch (OperationCanceledException) { StopCore(); break; }

                    if (token.IsCancellationRequested)
                        break;

                    lock (sync) target = desiredFile;
                    if (string.IsNullOrEmpty(target))
                        continue; // stop requested during the delay

                    bool started;
                    lock (sync)
                    {
                        try
                        {
                            StartPlay(target);
                            started = true;
                        }
                        catch
                        {
                            StopCore();
                            started = false;
                        }
                    }

                    if (!started)
                        break;

                    await FadeInAsync(token, fadeMs);

                    // Loop back; if the target is unchanged and playing, next pass breaks.
                }
            }
            finally
            {
                lock (sync) workerBusy = false;
            }
        }

        private void StartPlay(string filePath)
        {
            audioFile = new AudioFileReader(filePath) { Volume = 0f };
            loopStream = new LoopStream(audioFile);
            output = new WaveOutEvent();
            output.Init(loopStream);
            output.Volume = 1f;
            output.Play();
            CurrentFile = filePath;
        }

        private void StopCore()
        {
            if (output != null)
            {
                try { output.Stop(); } catch { }
                try { output.Dispose(); } catch { }
                output = null;
            }

            if (loopStream != null)
            {
                try { loopStream.Dispose(); } catch { }
                loopStream = null;
            }

            // LoopStream disposes the AudioFileReader.
            audioFile = null;

            CurrentFile = null;
        }

        private async Task FadeOutAsync(CancellationToken token, int fadeMs)
        {
            float start;
            lock (sync) { start = audioFile != null ? audioFile.Volume : 0f; }
            await FadeAsync(start, 0f, fadeMs, token);
        }

        private async Task FadeInAsync(CancellationToken token, int fadeMs)
        {
            float targetVol;
            lock (sync) { targetVol = volume; }
            await FadeAsync(0f, targetVol, fadeMs, token);
        }

        private async Task PlayCurrentForAsync(int ms, CancellationToken token)
        {
            // Just wait the requested duration while the current song keeps
            // looping; then token cancellation leaves cleanup to the caller.
            await Task.Delay(ms, token);
        }

        private async Task FadeAsync(float from, float to, int fadeMs, CancellationToken token)
        {
            if (fadeMs <= 0)
            {
                lock (sync) { if (audioFile != null) audioFile.Volume = to; }
                return;
            }

            var sw = Stopwatch.StartNew();

            while (true)
            {
                try { await Task.Delay(15, token); }
                catch (OperationCanceledException) { break; }

                float t = (float)(sw.Elapsed.TotalMilliseconds / fadeMs);
                if (t > 1f) t = 1f;

                float v = from + (to - from) * t;

                lock (sync) { if (audioFile != null) audioFile.Volume = Math.Max(0f, Math.Min(1f, v)); }

                if (t >= 1f)
                    break;
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                desiredFile = null;
                if (cancel != null)
                {
                    try { cancel.Cancel(); } catch { }
                    cancel.Dispose();
                    cancel = null;
                }
            }

            StopCore();
        }
    }

    internal class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
        }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    sourceStream.Position = 0;
                }
                else
                {
                    totalBytesRead += bytesRead;
                }
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { sourceStream.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
