using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asio4split.Core {
    class AudioRouter {
        /// <summary>
        /// ルーティング元のデバイス
        /// </summary>
        public MMDevice CaptureDevice { get; private set; }
        /// <summary>
        /// ルーティング先のデバイス
        /// </summary>
        public MMDevice RenderDevice { get; private set; }
        /// <summary>
        /// 利用可能なデバイスを取得します
        /// </summary>
        /// <returns></returns>
        public static MMDeviceCollection GetEndPoints(DataFlow dataFlow, DeviceState deviceState = DeviceState.Active) =>
            (new MMDeviceEnumerator()).EnumerateAudioEndPoints(dataFlow, deviceState);
    }
}
