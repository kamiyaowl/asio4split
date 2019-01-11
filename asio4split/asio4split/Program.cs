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
            var bitDepth = 24;
            var byteDepth = bitDepth / 8;
            var devices = AsioOut.GetDriverNames();
            // 使用するデバイスを決定する
            Console.WriteLine("# Input Device");
            var srcIndex = SelectDeviceIndex(devices);
            Console.WriteLine("# Output Device");
            var dstIndex = SelectDeviceIndex(devices);
            // デバイスのルーティング設定
            // TEST: srcのMASTER OUT L, RをdstのLRに当てることにする
            var routingChannels = new int[] { 12, 13 }; // srcのチャンネル番号を並べて書く


            // あとはよしなに
            using (var src = new AsioOut(devices[srcIndex]))
            using (var dst = new AsioOut(devices[dstIndex])) {
                // ルーティングテーブルがイマイチの場合に補完する
                var routingChannelDiff = dst.DriverOutputChannelCount - routingChannels.Length;
                if (routingChannelDiff > 0) {
                    Console.WriteLine($"Warning: RoutingChannel added {routingChannelDiff} empty channles");
                    routingChannels = routingChannels.Concat(Enumerable.Repeat(-1, routingChannelDiff)).ToArray();
                } else if (routingChannelDiff < 0) {
                    Console.WriteLine($"Warning: RoutingChannel resize {routingChannelDiff}");
                    routingChannels = routingChannels.Take(dst.DriverOutputChannelCount).ToArray();
                }
                Console.WriteLine($"RoutingChannels: [{string.Join(",", routingChannels)}]");
                // Output Setting
                var bufferProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, bitDepth, dst.DriverOutputChannelCount));
                dst.Init(bufferProvider);
                dst.Play();
                // Input Setting
                float[] srcBuffer = null;
                byte[] dstBuffer = null;
                src.AudioAvailable += (sx, ex) => {
                    if (srcBuffer == null) {
                        srcBuffer = new float[ex.SamplesPerBuffer * src.DriverInputChannelCount]; // 1ch[0:511], 2ch[512:... nch
                        dstBuffer = new byte[ex.SamplesPerBuffer * dst.DriverOutputChannelCount * byteDepth];

                        Console.WriteLine("# Source");
                        Console.WriteLine($"\tDriverName: {src.DriverName}");
                        Console.WriteLine($"\tDriverInputChannelCount: {src.DriverInputChannelCount}");
                        Console.WriteLine($"\tDriverOutputChannelCount: {src.DriverOutputChannelCount}");
                        Console.WriteLine($"\tSamplePerBuffer: {ex.SamplesPerBuffer}");
                        Console.WriteLine($"\tAsioSampleType: {ex.AsioSampleType}");
                        Console.WriteLine($"\tsrcBufferSize: {srcBuffer.Length}");

                        Console.WriteLine("# Destination");
                        Console.WriteLine($"\tDriverName: {dst.DriverName}");
                        Console.WriteLine($"\tDriverInputChannelCount: {dst.DriverInputChannelCount}");
                        Console.WriteLine($"\tDriverOutputChannelCount: {dst.DriverOutputChannelCount}");
                        Console.WriteLine($"\tdstBufferSize: {dstBuffer.Length}");
                    }
                    ex.GetAsInterleavedSamples(srcBuffer);

                    // データの書き込み src -> dst
                    for (int dataPtr = 0; dataPtr < ex.SamplesPerBuffer; ++dataPtr) {
                        for (int ch = 0; ch < routingChannels.Length; ++ch) {
                            // src 1sampleのデータをbyteDepthに分割して書き込む
                            UInt32 raw;
                            if (routingChannels[ch] == -1) {
                                raw = 0;
                            } else {
                                var srcPtr = dataPtr + (routingChannels[ch] * ex.SamplesPerBuffer);
                                Int32 sample = (Int32)(srcBuffer[srcPtr] * Int32.MaxValue);
                                raw = (UInt32)sample;
                            }

                            var dstPtr = ((dataPtr * routingChannels.Length) + ch) * byteDepth;
                            for (int byteOffset = 0; byteOffset < byteDepth; ++byteOffset) {
                                dstBuffer[dstPtr + byteOffset] = (byte)((raw >> (8 * (3 - byteOffset))) & 0xff);
                            }
                        }
                    }
                    bufferProvider.AddSamples(dstBuffer, 0, dstBuffer.Length);
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
