using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Speak.Model
{
    public class MessageVO
    {
        public string messageType { get; set; } // 0文字，1语音
        public string messageFromNumber { get; set; }
        public string messageFromName { get; set; }
        public string messageFromTime { get; set; }
        public string messageFromGroupId { get; set; }
        public string messageText { get; set; }
        public string messagePath { get; set; }
        // =======================================
        public string messageFromNameAndFromNumber { get; set; }
        public string messageAudioTimeInfo { get; set; }
        public bool messageIsText { get; set; }
        public bool messageIsAudio { get; set; }
        public bool messageIsImage { get; set; }
        public string messageSendTimeInfo { get; set; }
    }
}
