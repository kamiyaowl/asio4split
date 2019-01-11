using asio4split.Core;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asio4split {
    class Program {
        static void Main(string[] args) {
            var caps = AudioRouter.GetEndPoints(DataFlow.Capture).ToArray();
            var renders = AudioRouter.GetEndPoints(DataFlow.Render).ToArray();

            // TODO: UUIDみたいな固有値でStore/Restoreできるように
            Console.WriteLine("# Capture Device");
            var cap = SelectDevice(caps);
            Console.WriteLine("# Render Device");
            var render = SelectDevice(renders);

            Console.ReadKey();

        }
        /// <summary>
        /// ユーザーに使用するデバイスを選択させます
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        private static MMDevice SelectDevice(MMDevice[] devices) {
            foreach (var x in devices.Select((x, i) => new { Index = i, Value = x })) {
                Console.WriteLine($"[{x.Index}] {x.Value}");
            }
            Console.Write($"\r\ndevice index[0 - {devices.Length - 1}] >");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= devices.Length) {
                Console.WriteLine("Invalid index");
                Environment.Exit(1);
            }
            return devices[index];
        }
    }
}
