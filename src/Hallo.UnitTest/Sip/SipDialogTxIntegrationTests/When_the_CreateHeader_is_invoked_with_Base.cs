using System;
using System.Net;
using System.Text;
using System.Threading;
using FluentAssertions;
using Hallo.Component;
using Hallo.Sip;
using Hallo.Sip.Headers;
using Hallo.Sip.Stack;
using Hallo.Sip.Util;
using Hallo.UnitTest.Builders;
using Hallo.UnitTest.Helpers;
using Hallo.UnitTest.Stubs;
using NUnit.Framework;

namespace Hallo.UnitTest.Sip.SipDialogTxIntegrationTests
{
    //internal abstract class SoftPhoneSpecificationBase : Specification
    //{
    //    protected FakeNetwork _network;
    //    protected SipRequest _invite;
    //    protected SipRequest _ack;
    //    protected ManualResetEvent _wait = new ManualResetEvent(false);
    //    protected ManualResetEvent _waitingforInviteReceived = new ManualResetEvent(false);
    //    protected decimal _counter;
    //    protected SipProvider _sipProvider1;
    //    protected SipProvider _sipProvider2;
    //    protected TimerFactoryStub _timerFactory;
    //    protected IPEndPoint _testClientUaEndPoint;
    //    protected SipUri _testClientUaUri;
    //    protected SipUri _phoneUaUri;
    //    protected IPEndPoint _phoneUaEndPoint;
    //    protected ManualResetEvent _waitingforOkReceived = new ManualResetEvent(false);
    //    protected ManualResetEvent _waitingforInviteProcessed = new ManualResetEvent(false);

    //    protected SoftPhoneSpecificationBase()
    //    {
    //        _testClientUaUri = TestConstants.EndPoint1Uri;
    //        _testClientUaEndPoint = TestConstants.IpEndPoint1;
    //        _phoneUaUri = TestConstants.EndPoint2Uri;
    //        _phoneUaEndPoint = TestConstants.IpEndPoint2;
    //        _timerFactory = new TimerFactoryStubBuilder().Build();
    //    }

       
    //    protected override void Given()
    //    {
    //        _network = new FakeNetwork();
    //        //create invite that is addresses to the phone's sipuri
    //        _invite = CreateRequest(_testClientUaUri, _phoneUaUri, SipMethods.Invite, totag:null);
    //        //create phone that is located at IpEndPoint2
    //        var fcs1 = new FakeSipContextSource(_phoneUaEndPoint);
    //        var fcs2 = new FakeSipContextSource(_testClientUaEndPoint);
    //        fcs1.AddToNetwork(_network);
    //        fcs2.AddToNetwork(_network);
    //        var stack = new SipStack();

    //        _sipProvider1 = new SipProvider(stack, fcs1);
    //        _sipProvider2 = new SipProvider(stack, fcs2);

    //        _sipProvider1.AddSipListener(new StubSipListener((re)=> { }));

    //        _sipProvider1.SendRequest(_invite);

    //        GivenOverride();
    //    }

    //    protected virtual void _calleePhone_InternalStateChanged(object sender, EventArgs e)
    //    { }

    //    ///// <summary>
    //    ///// creates a ack. this is to be sent by the testclient UA
    //    ///// </summary>
    //    ///// <param name="invite"></param>
    //    ///// <param name="ringing"></param>
    //    ///// <returns></returns>
    //    //protected SipRequest CreateAckRequest(SipRequest invite, SipResponse ringing)
    //    //{
    //    //    var addressFactory = new SipAddressFactory();
    //    //    var headerFactory = new SipHeaderFactory();
    //    //    var messageFactory = new SipMessageFactory();

    //    //    var localSequenceNr = invite.CSeq.Sequence;
    //    //    /*_remoteSequenceNr remains empty */
    //    //    var callId = invite.CallId.Value;
    //    //    var localTag = invite.From.Tag;
    //    //    var remoteUri = invite.To.SipUri;
    //    //    var localUri = invite.From.SipUri;

    //    //    var remoteTag = ringing.To.Tag;
    //    //    var remoteTarget = ringing.Contacts.GetTopMost().SipUri;
    //    //    var routeSet = ringing.RecordRoutes.ToList();//refuse looseroutin-less recordroutes
    //    //    routeSet.Reverse();

    //    //    var cseqHeader = headerFactory.CreateSCeqHeader(SipMethods.Ack, localSequenceNr);
    //    //    var toAddress = addressFactory.CreateAddress(null, remoteUri);
    //    //    var toHeader = headerFactory.CreateToHeader(toAddress, remoteTag);
    //    //    var fromAddress = addressFactory.CreateAddress(null, localUri);
    //    //    var fromHeader = headerFactory.CreateFromHeader(fromAddress, localTag);
    //    //    var callIdheader = headerFactory.CreateCallIdHeader(callId);
    //    //    var viaHeader = invite.Vias.GetTopMost();
    //    //    var requestUri = remoteUri.Clone();

    //    //    var maxForwardsHeader = headerFactory.CreateMaxForwardsHeader();
    //    //    var request = messageFactory.CreateRequest(
    //    //        requestUri,
    //    //        SipMethods.Ack,
    //    //        callIdheader,
    //    //        cseqHeader,
    //    //        fromHeader,
    //    //        toHeader,
    //    //        viaHeader,
    //    //        maxForwardsHeader);

    //    //    foreach (var route in routeSet)
    //    //    {
    //    //        request.Routes.Add(new SipRouteHeader() { SipUri = route.SipUri, Parameters = route.Parameters });
    //    //    }

    //    //    return request;
    //    //}


    //    ///// <summary>
    //    ///// creates a bye. this is to be sent by the testclient UA
    //    ///// </summary>
    //    ///// <param name="invite"></param>
    //    ///// <param name="ringing"></param>
    //    ///// <returns></returns>
    //    //protected SipRequest CreateByeRequest(SipRequest invite, SipResponse ringing)
    //    //{
    //    //    var addressFactory = new SipAddressFactory();
    //    //    var headerFactory = new SipHeaderFactory();
    //    //    var messageFactory = new SipMessageFactory();

    //    //    var localSequenceNr = invite.CSeq.Sequence;
    //    //    /*_remoteSequenceNr remains empty */
    //    //    var callId = invite.CallId.Value;
    //    //    var localTag = invite.From.Tag;
    //    //    var remoteUri = invite.To.SipUri;
    //    //    var localUri = invite.From.SipUri;

    //    //    var remoteTag = ringing.To.Tag;
    //    //    var remoteTarget = ringing.Contacts.GetTopMost().SipUri;
    //    //    var routeSet = ringing.RecordRoutes.ToList();//refuse looseroutin-less recordroutes
    //    //    routeSet.Reverse();

    //    //    var cseqHeader = headerFactory.CreateSCeqHeader(SipMethods.Bye, localSequenceNr + 1);
    //    //    var toAddress = addressFactory.CreateAddress(null, remoteUri);
    //    //    var toHeader = headerFactory.CreateToHeader(toAddress, remoteTag);
    //    //    var fromAddress = addressFactory.CreateAddress(null, localUri);
    //    //    var fromHeader = headerFactory.CreateFromHeader(fromAddress, localTag);
    //    //    var callIdheader = headerFactory.CreateCallIdHeader(callId);
    //    //    var viaHeader = new SipViaHeaderBuilder().WithSentBy(_testClientUaEndPoint).Build();
    //    //    var requestUri = remoteUri.Clone();

    //    //    var maxForwardsHeader = headerFactory.CreateMaxForwardsHeader();
    //    //    var request = messageFactory.CreateRequest(
    //    //        requestUri,
    //    //        SipMethods.Bye,
    //    //        callIdheader,
    //    //        cseqHeader,
    //    //        fromHeader,
    //    //        toHeader,
    //    //        viaHeader,
    //    //        maxForwardsHeader);

    //    //    foreach (var route in routeSet)
    //    //    {
    //    //        request.Routes.Add(new SipRouteHeader() { SipUri = route.SipUri, Parameters = route.Parameters });
    //    //    }

    //    //    return request;
    //    //}

    //    ///// <summary>
    //    ///// creates a ringing. this is to be sent by the testclient UA
    //    ///// </summary>
    //    //protected SipResponse CreateRingingResponse(SipRequest receivedInvite, string toTag)
    //    //{
    //    //    var ringing = receivedInvite.CreateResponse(SipResponseCodes.x180_Ringing);
    //    //    ringing.To.Tag = toTag;
    //    //    var contactUri = _phone.AddressFactory.CreateUri("", _phone.SipProvider.ListeningPoint.ToString());
    //    //    ringing.Contacts.Add(_phone.HeaderFactory.CreateContactHeader(contactUri));

    //    //    return ringing;
    //    //}

    //    ///// <summary>
    //    ///// creates a ringing. this is to be sent by the testclient UA
    //    ///// </summary>
    //    //protected SipResponse CreateOkResponse(SipRequest receivedInvite, string toTag)
    //    //{
    //    //    var r = receivedInvite.CreateResponse(SipResponseCodes.x200_Ok);
    //    //    r.To.Tag = toTag;
    //    //    var contactUri = _phone.AddressFactory.CreateUri("", _phone.SipProvider.ListeningPoint.ToString());
    //    //    r.Contacts.Add(_phone.HeaderFactory.CreateContactHeader(contactUri));

    //    //    return r;
    //    //}

    //    //protected SipResponse CreateResponse(SipRequest receivedInvite, string toTag, string responseCode)
    //    //{
    //    //    var r = receivedInvite.CreateResponse(responseCode);
    //    //    r.To.Tag = toTag;
    //    //    var contactUri = _phone.AddressFactory.CreateUri("", _phone.SipProvider.ListeningPoint.ToString());
    //    //    r.Contacts.Add(_phone.HeaderFactory.CreateContactHeader(contactUri));

    //    //    return r;
    //    //}

       
    //    protected virtual void GivenOverride()
    //    {

    //    }

       
    //    protected SipRequest CreateRequest(SipUri from, SipUri to, string method, string callId = "callid", string totag = "totag", string fromtag ="fromtag")
    //    {
    //        var r = new SipRequestBuilder()
    //            .WithRequestLine(
    //                new SipRequestLineBuilder().WithMethod(method).Build())
    //            .WithCSeq(
    //                new SipCSeqHeaderBuilder().WithCommand(SipMethods.Invite).WithSequence(1).Build())
    //            .WithFrom(
    //                new SipFromHeaderBuilder().WithSipUri(from).WithTag(fromtag).Build())
    //            .WithTo(
    //                new SipToHeaderBuilder().WithSipUri(to).WithTag(totag).Build())
    //            .WithCallId(
    //                new SipCallIdHeaderBuilder().WithValue(SipUtil.CreateCallId()).Build())
    //            .WithContacts(
    //                new SipContactHeaderListBuilder()
    //                    .Add(new SipContactHeaderBuilder().WithSipUri(from).Build())
    //                    .Build())
    //            .WithRecordRoutes(new SipHeaderList<SipRecordRouteHeader>())
    //            .Build();

    //        return r;
    //    }

    //}
}