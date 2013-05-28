﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Hallo.Sip.Headers;
using Hallo.Sip.Stack.Dialogs;
using Hallo.Sip.Util;
using Hallo.Util;

namespace Hallo.Sip.Stack.Transactions.InviteClient
{
    public partial class SipInviteClientTransaction : SipAbstractClientTransaction
    {
        private readonly SipHeaderFactory _headerFactory;
        private readonly SipMessageFactory _messageFactory;
        internal static readonly CallingCtxState CallingState = new CallingCtxState();
        internal static readonly ProceedingCtxState ProceedingState = new ProceedingCtxState();
        internal static readonly CompletedCtxState CompletedState = new CompletedCtxState();
        internal static readonly TerminatedCtxState TerminatedState = new TerminatedCtxState();
        private SipAbstractDialog _dialog;

        internal SipInviteClientTransaction(
            SipClientTransactionTable table,
            ISipMessageSender messageSender,
            ISipListener listener, 
            SipRequest request, 
            ITimerFactory timerFactory, 
            SipHeaderFactory headerFactory,
            SipMessageFactory messageFactory)
            : base(table, request, messageSender, listener, timerFactory)
        {
            Check.Require(headerFactory, "headerFactory");
            Check.Require(messageFactory, "messageFactory");

            Check.IsTrue(request.RequestLine.Method == SipMethods.Invite, "Method other then 'INVITE' is not allowed");

            _headerFactory = headerFactory;
            _messageFactory = messageFactory;

            ReTransmitTimer = timerFactory.CreateInviteCtxRetransmitTimer(OnReTransmit);
            TimeOutTimer = timerFactory.CreateInviteCtxTimeOutTimer(OnTimeOut);
            EndCompletedTimer = timerFactory.CreateInviteCtxEndCompletedTimer(OnCompletedEnded);
        }
        
        private void OnReTransmit()
        {
            lock (_lock)
            {
                /* ignore callbacks when the timer is disposed. These can potentially happen when
                * timercallbacks, queued by the threadpool, are invoked (with a certain delay !!)*/

                if(!ReTransmitTimer.IsDisposed) State.Retransmit(this);
            }
        }

        private void OnTimeOut()
        {
            ChangeState(SipInviteClientTransaction.TerminatedState);

            Dispose();

            _listener.ProcessTimeOut(new SipClientTxTimeOutEvent() { Request = Request, ClientTransaction = this });
        }

        private void OnCompletedEnded()
        {
            ChangeState(SipInviteClientTransaction.TerminatedState);
            Dispose();
        }

        public override void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed) return;
                _isDisposed = true;
                ReTransmitTimer.Dispose();
                TimeOutTimer.Dispose();
                EndCompletedTimer.Dispose();
                
                SipAbstractClientTransaction tx;
                _table.TryRemove(this.GetId(), out tx);

                //State = TerminatedState; is done outside of this method
            }

            if (_stateObserver != null)
            {
                //_stateObserver.OnNext(CreateStateInfo(State.Name));
                _stateObserver.OnCompleted();
            }
        }

        internal AbstractCtxState State { get; private set; }
        internal SipResponse LatestResponse { get; set; }
        private ITimer ReTransmitTimer { get; set; }
        private ITimer TimeOutTimer { get; set; }
        private ITimer EndCompletedTimer { get; set; }

       
        public override void SendRequest()
        {
            if (!_table.TryAdd(this.GetId(), this))
                throw new Exception(
                    string.Format("Could not add client transaction. The id already exists. Id:'{0}'. .", this.GetId()));
            
            ChangeState(CallingState);

            SendRequestInternal();
        }

        public override void ProcessResponse(SipResponseEvent responseEvent)
        {
            StateResult result;
            lock (_lock)
            {
                LatestResponse = responseEvent.Response;
                result = State.HandleResponse(this, responseEvent.Response);
            }

            if (result.InformToUser)
            {
                responseEvent.ClientTransaction = this;
                _listener.ProcessResponse(responseEvent);
            }
            if(result.Dispose)
            {
                Dispose();
            }
        }
        
        internal void ChangeState(AbstractCtxState newstate)
        {
            /*attention: no locking. Is already called thread safely via call to AdaptToResponse*/
            State = newstate;
            newstate.Initialize(this);

            if(_stateObserver != null) _stateObserver.OnNext(CreateStateInfo(newstate.Name));
        }
        
        internal void SendAck()
        {
            var ackRequest = CreateAckRequest();
            _messageSender.SendRequest(ackRequest);
        }

        internal SipRequest CreateAckRequest()
        {
            if (State != CompletedState) throw new InvalidOperationException(string.Format("The Tx is unable to create an 'ACK' Request. To be able to create an 'ACK' request, the Tx must be in 'Completed' state. CurrentState:{0}", State.Name));

            Check.Require(LatestResponse, "LatestResponse");

            var requestUri = Request.RequestLine.Uri.Clone();
            var callIdheader = (SipCallIdHeader)Request.CallId.Clone();
            var cseqHeader = _headerFactory.CreateSCeqHeader(SipMethods.Ack, Request.CSeq.Sequence);
            var fromHeader = (SipFromHeader)Request.From.Clone();
            var toHeader = (SipToHeader) LatestResponse.To.Clone();
            var viaHeader = (SipViaHeader) Request.Vias.GetTopMost().Clone();
            var maxForwardsHeader = _headerFactory.CreateMaxForwardsHeader();
            var ackRequest = _messageFactory.CreateRequest(
                requestUri,
                SipMethods.Ack,
                callIdheader,
                cseqHeader,
                fromHeader,
                toHeader,
                viaHeader,
                maxForwardsHeader);
            
            //TODO: not clear from the rfc if the routes have to be copied from request or response
            foreach (var route in Request.Routes)
            {
                ackRequest.Routes.Add((SipRouteHeader) route.Clone());
            }

            return ackRequest;
        }

        public override SipTransactionType Type
        {
            get { return SipTransactionType.InviteClient; }
        }

        internal void SetDialog(SipAbstractDialog dialog)
        {
            _dialog = dialog;
        }

        public SipAbstractDialog GetDialog()
        {
            return _dialog;
        }
    }
}
