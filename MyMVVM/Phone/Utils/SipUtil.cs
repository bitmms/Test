using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyMVVM.Common.Utils;
using pjsua2xamarin.pjsua2;

namespace MyMVVM.Phone.Utils
{
    public class SipUtil
    {
        public static string phoneNumber = "";
        public static int incomingcallid = -1;
        public static MyAccount account = new MyAccount();
        public static MyCall call;
        public static Endpoint endpoint = new Endpoint();
        delegate void SetTextCallback();

        public static int CallReminder = 0;

        public static void x_MessageEvent()
        {
            SetTextCallback msgCallback = new SetTextCallback(x_MessageEvent);
            SipUtil.call = new MyCall(SipUtil.account, incomingcallid);
        }

        //初始化设置
        public static void VideoinItialize()
        {
            try
            {
                endpoint.libCreate();
                EpConfig epConfig = new EpConfig();
                epConfig.logConfig.level = 1;
                endpoint.libInit(epConfig);
                TransportConfig tcfg = new TransportConfig();
                tcfg.port = 0;
                endpoint.transportCreate(pjsip_transport_type_e.PJSIP_TRANSPORT_UDP, tcfg);
                endpoint.libStart();

                string host = DMVariable.SSHIP;

                Dictionary<string, string> dic = PhoneDB.GetNumberOfVideoUser();

                string user = dic["softphone"];
                phoneNumber = user;
                string pwd = dic["softphone_pass"];
                AccountConfig accountConfig = new AccountConfig();
                accountConfig.idUri = "sip:" + user + "@" + host;
                accountConfig.regConfig.registrarUri = "sip:" + host;
                accountConfig.sipConfig.authCreds.Add(new AuthCredInfo("digest", "*", user, 0, pwd));
                accountConfig.videoConfig.autoShowIncoming = true;
                accountConfig.videoConfig.autoTransmitOutgoing = true;
                SipUtil.account.create(accountConfig);
                //Console.WriteLine("******************* 注册 *************************");
            }
            catch (Exception e)
            {

            }
        }

        //清理释放
        public static void videoClosing()
        {
            endpoint.hangupAllCalls();
            if (call != null) call.Dispose();
            account.Dispose();
            endpoint.libDestroy();
        }
    }


    //拨号执行前
    public class MyAccount : Account
    {
        override public void onRegState(OnRegStateParam prm)
        {
            SipUtil.x_MessageEvent();
        }
        public override void onIncomingCall(OnIncomingCallParam prm)
        {
            SipUtil.incomingcallid = prm.callId;
            SipUtil.x_MessageEvent();

            MessageBoxResult result = MessageBox.Show("是否确认接听？", "呼叫提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // 用户点击了“是”
                // 执行操作
                if (SipUtil.incomingcallid != -1 && SipUtil.account != null)
                {
                    CallOpParam p = new CallOpParam();
                    p.statusCode = pjsip_status_code.PJSIP_SC_OK;
                    SipUtil.call.answer(p);
                }
            }
            else
            {
                // 用户点击了“否”
                // 执行其他操作或者什么都不做
                if (SipUtil.call != null)
                {
                    CallOpParam x = new CallOpParam(true);
                    x.statusCode = pjsip_status_code.PJSIP_SC_DECLINE;
                    SipUtil.call.hangup(x);
                    SipUtil.call.Dispose();
                }
                SipUtil.incomingcallid = -1;
            }
        }
    }


    //音频
    public class MyCall : Call
    {
        public MyCall(Account acc, int call_id) : base(acc, call_id) { }

        public override void onCallState(OnCallStateParam prm)
        {
            base.onCallState(prm);

        }
        public override void onCallMediaState(OnCallMediaStateParam prm)
        {
            base.onCallMediaState(prm);
            CallInfo ci = getInfo();
            CallMediaInfo[] callminfo = ci.media.ToArray();
            for (int i = 0; i < callminfo.Length; i++)
            {
                CallMediaInfo cmi = callminfo[i];
                if (cmi.type == pjmedia_type.PJMEDIA_TYPE_AUDIO && (cmi.status == pjsua_call_media_status.PJSUA_CALL_MEDIA_ACTIVE || cmi.status == pjsua_call_media_status.PJSUA_CALL_MEDIA_REMOTE_HOLD))
                {
                    Media m = getMedia((uint)i);
                    AudioMedia am = AudioMedia.typecastFromMedia(m);
                    try
                    {
                        SipUtil.endpoint.audDevManager().getCaptureDevMedia().startTransmit(am);
                        SipUtil.endpoint.audDevManager().getCaptureDevMedia().adjustRxLevel(1.0f);
                        SipUtil.endpoint.audDevManager().getCaptureDevMedia().adjustTxLevel(1.0f);
                        am.startTransmit(SipUtil.endpoint.audDevManager().getPlaybackDevMedia());
                        SipUtil.endpoint.audDevManager().getPlaybackDevMedia().adjustRxLevel(1.0f);
                        SipUtil.endpoint.audDevManager().getPlaybackDevMedia().adjustTxLevel(1.0f);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        continue;
                    }
                }
            }
        }
    }


}
