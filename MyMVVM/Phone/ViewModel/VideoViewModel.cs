using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Phone.Utils;
using pjsua2xamarin.pjsua2;

namespace MyMVVM.Phone.ViewModel
{
    public class VideoViewModel : ViewModelsBase
    {

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// 软键盘输入
        /// </summary>
        /// <param name="number"></param>
		public void AppendNumber(string number)
        {
            Phone += number;
        }


        /// <summary>
        /// 软键盘删除
        /// </summary>
		public void Backspace()
        {
            if (Phone.Length > 0)
            {
                Phone = Phone.Substring(0, Phone.Length - 1);
            }
        }

        /// <summary>
        /// 软键盘清空
        /// </summary>
		public void Clear()
        {
            Phone = string.Empty;
        }
        public static string CallReminder { get; set; }


        public VideoViewModel()
        {
            if (CallReminder == null) { CallReminder = Phone; }

        }

        //接听
        public ICommand AnswerBut => new ViewModelCommand(param =>
        {
            if (SipUtil.incomingcallid != -1 && SipUtil.account != null)
            {
                CallOpParam p = new CallOpParam();
                p.statusCode = pjsip_status_code.PJSIP_SC_OK;
                SipUtil.call.answer(p);
            }
        });

        // 呼叫
        public ICommand CalloutBut => new ViewModelCommand(param =>
        {
            if (Phone == null || Phone == "")
            {
                DMMessageBox.Show("警告", "请输入号码", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }
            if (SipUtil.phoneNumber == "")
            {
                DMMessageBox.Show("警告", "未配置视频通话用户，请前往网页端进行配置", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }
            SipUtil.call = new MyCall(SipUtil.account, -1);
            CallOpParam prm = new CallOpParam(true);
            prm.opt.videoCount = 0;
            SipUtil.call.makeCall("sip:" + Phone + $"@{DMVariable.SSHIP}", prm);
        });

        //视频
        public ICommand VideoBut => new ViewModelCommand(param =>
        {
            if (Phone == null || Phone == "")
            {
                DMMessageBox.Show("警告", "请输入号码", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }

            if (SipUtil.phoneNumber == "")
            {
                DMMessageBox.Show("警告", "未配置视频通话用户，请前往网页端进行配置", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }
            SipUtil.call = new MyCall(SipUtil.account, -1);
            CallOpParam prm = new CallOpParam(true);
            prm.opt.videoCount = 1;
            SipUtil.call.makeCall("sip:" + Phone + $"@{DMVariable.SSHIP}", prm);
        });

        //挂断
        public ICommand HangUpBut => new ViewModelCommand(param =>
        {
            if (Phone == null || Phone == "")
            {
                DMMessageBox.Show("警告", "请输入号码", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }

            if (SipUtil.phoneNumber == "")
            {
                DMMessageBox.Show("警告", "未配置视频通话用户，请前往网页端进行配置", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                return;
            }
            if (SipUtil.call != null)
            {
                CallOpParam x = new CallOpParam(true);
                x.statusCode = pjsip_status_code.PJSIP_SC_DECLINE;
                SipUtil.call.hangup(x);
                SipUtil.call.Dispose();
            }
            SipUtil.incomingcallid = -1;
        });

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
