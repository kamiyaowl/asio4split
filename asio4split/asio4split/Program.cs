using asio4split.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asio4split {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            var sampleRate = 48000;
            var caps = AsioOut.GetDriverNames();
            Console.WriteLine("# Input Device");
            var srcIndex = SelectDeviceIndex(caps);
            Console.WriteLine("# Output Device");
            var dstIndex = SelectDeviceIndex(caps);

            using (var src = new AsioOut(caps[srcIndex]))
            using (var dst = new AsioOut(caps[dstIndex])) {
                float[] buffer = null;
                src.AudioAvailable += (sx, ex) => {
                    if (buffer == null) {
                        buffer = new float[ex.SamplesPerBuffer * src.DriverInputChannelCount];

                        Console.WriteLine($"DriverName: {src.DriverName}");
                        Console.WriteLine($"DriverInputChannelCount: {src.DriverInputChannelCount}");
                        Console.WriteLine($"SamplePerBuffer: {ex.SamplesPerBuffer}");
                        Console.WriteLine($"AsioSampleType: {ex.AsioSampleType}");
                        Console.WriteLine($"bufferSize: {buffer.Length}");
                    }
                    ex.GetAsInterleavedSamples(buffer);
                };
                src.InitRecordAndPlayback(null, src.DriverInputChannelCount, sampleRate);
                src.Play();

                Console.ReadKey();
            }

        }
        /// <summary>
        /// ユーザーに使用するデバイスを選択させます
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        private static int SelectDeviceIndex(string[] devices) {
            foreach (var x in devices.Select((x, i) => new { Index = i, Value = x })) {
                Console.WriteLine($"[{x.Index}] {x.Value}");
            }
            Console.Write($"\r\ndevice index[0 - {devices.Length - 1}] >");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= devices.Length) {
                Console.WriteLine("Invalid index");
                Environment.Exit(1);
            }
            return index;
        }
    }
}
