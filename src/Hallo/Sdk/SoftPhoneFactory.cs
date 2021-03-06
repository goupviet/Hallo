using System.Net;
using Hallo.Sdk.SoftPhoneStates;
using Hallo.Sip;
using Hallo.Component;
using Hallo.Sip.Stack;

namespace Hallo.Sdk
{
    public class SoftPhoneFactory
    {
        
        private SoftPhoneFactory()
        {
        }

        public static ISoftPhone CreateSoftPhone(IPEndPoint listeningPoint)
        {
            var sipStack = new SipStack();
            var sipListeningPoint = sipStack.CreateUdpListeningPoint(listeningPoint);
            var provider = sipStack.CreateSipProvider(sipListeningPoint);
            return new SoftPhone(provider, sipStack.CreateMessageFactory(), sipStack.CreateHeaderFactory(), sipStack.CreateAddressFactory(), new SoftPhoneStateProvider(), new TimerFactory(), sipListeningPoint);
        }
    }
}