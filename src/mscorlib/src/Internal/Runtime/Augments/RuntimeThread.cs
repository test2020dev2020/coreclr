// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace Internal.Runtime.Augments
{
    public class RuntimeThread : CriticalFinalizerObject
    {
        public static RuntimeThread Create(ThreadStart start) => new Thread(start);
        public static RuntimeThread Create(ThreadStart start, int maxStackSize) => new Thread(start, maxStackSize);
        public static RuntimeThread Create(ParameterizedThreadStart start) => new Thread(start);
        public static RuntimeThread Create(ParameterizedThreadStart start, int maxStackSize) => new Thread(start, maxStackSize);

        private Thread AsThread()
        {
            Contract.Assert(this is Thread);
            return (Thread)this;
        }

        public static RuntimeThread CurrentThread => Thread.CurrentThread;

        /*=========================================================================
        ** Returns true if the thread has been started and is not dead.
        =========================================================================*/
        public extern bool IsAlive
        {
            [SecuritySafeCritical]  // auto-generated
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        /*=========================================================================
        ** Return whether or not this thread is a background thread.  Background
        ** threads do not affect when the Execution Engine shuts down.
        **
        ** Exceptions: ThreadStateException if the thread is dead.
        =========================================================================*/
        public bool IsBackground
        {
            [SecuritySafeCritical]  // auto-generated
            get { return IsBackgroundNative(); }
            [SecuritySafeCritical]  // auto-generated
            [HostProtection(SelfAffectingThreading = true)]
            set { SetBackgroundNative(value); }
        }

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool IsBackgroundNative();

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void SetBackgroundNative(bool isBackground);

        /*=========================================================================
        ** Returns true if the thread is a threadpool thread.
        =========================================================================*/
        public extern bool IsThreadPoolThread
        {
            [SecuritySafeCritical]  // auto-generated
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public int ManagedThreadId => AsThread().ManagedThreadId;
        public string Name { get { return AsThread().Name; } set { AsThread().Name = value; } }

        /*=========================================================================
        ** Returns the priority of the thread.
        **
        ** Exceptions: ThreadStateException if the thread is dead.
        =========================================================================*/
        public ThreadPriority Priority
        {
            [SecuritySafeCritical]  // auto-generated
            get { return (ThreadPriority)GetPriorityNative(); }
            [SecuritySafeCritical]  // auto-generated
            [HostProtection(SelfAffectingThreading = true)]
            set { SetPriorityNative((int)value); }
        }

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int GetPriorityNative();

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void SetPriorityNative(int priority);

        /*=========================================================================
        ** Return the thread state as a consistent set of bits.  This is more
        ** general then IsAlive or IsBackground.
        =========================================================================*/
        public ThreadState ThreadState
        {
            [SecuritySafeCritical]  // auto-generated
            get { return (ThreadState)GetThreadStateNative(); }
        }

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int GetThreadStateNative();

        [SecuritySafeCritical]  // auto-generated
        public ApartmentState GetApartmentState()
        {
#if FEATURE_COMINTEROP_APARTMENT_SUPPORT
            return (ApartmentState)GetApartmentStateNative();
#else // !FEATURE_COMINTEROP_APARTMENT_SUPPORT
            Contract.Assert(false); // the Thread class in CoreFX should have handled this case
            return ApartmentState.MTA;
#endif // FEATURE_COMINTEROP_APARTMENT_SUPPORT
        }

        /*=========================================================================
        ** An unstarted thread can be marked to indicate that it will host a
        ** single-threaded or multi-threaded apartment.
        =========================================================================*/
        [SecuritySafeCritical]  // auto-generated
        [HostProtection(Synchronization = true, SelfAffectingThreading = true)]
        public bool TrySetApartmentState(ApartmentState state)
        {
#if FEATURE_COMINTEROP_APARTMENT_SUPPORT
            return SetApartmentStateHelper(state, false);
#else // !FEATURE_COMINTEROP_APARTMENT_SUPPORT
            Contract.Assert(false); // the Thread class in CoreFX should have handled this case
            return false;
#endif // FEATURE_COMINTEROP_APARTMENT_SUPPORT
        }

#if FEATURE_COMINTEROP_APARTMENT_SUPPORT
        [SecurityCritical]  // auto-generated
        internal bool SetApartmentStateHelper(ApartmentState state, bool fireMDAOnMismatch)
        {
            ApartmentState retState = (ApartmentState)SetApartmentStateNative((int)state, fireMDAOnMismatch);

            // Special case where we pass in Unknown and get back MTA.
            //  Once we CoUninitialize the thread, the OS will still
            //  report the thread as implicitly in the MTA if any
            //  other thread in the process is CoInitialized.
            if ((state == System.Threading.ApartmentState.Unknown) && (retState == System.Threading.ApartmentState.MTA))
                return true;

            if (retState != state)
                return false;

            return true;
        }

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern int GetApartmentStateNative();

        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern int SetApartmentStateNative(int state, bool fireMDAOnMismatch);
#endif // FEATURE_COMINTEROP_APARTMENT_SUPPORT

#if FEATURE_COMINTEROP
        [SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void DisableComObjectEagerCleanup();
#else // !FEATURE_COMINTEROP
        [SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void DisableComObjectEagerCleanup()
        {
            Contract.Assert(false); // the Thread class in CoreFX should have handled this case
        }
#endif // FEATURE_COMINTEROP

        /*=========================================================================
        ** Interrupts a thread that is inside a Wait(), Sleep() or Join().  If that
        ** thread is not currently blocked in that manner, it will be interrupted
        ** when it next begins to block.
        =========================================================================*/
#if FEATURE_CORECLR
        [SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical] // auto-generated
#endif
#pragma warning disable 618 // obsolete types: SecurityPermissionAttribute, SecurityAction
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
#pragma warning restore 618 // obsolete types: SecurityPermissionAttribute, SecurityAction
        public void Interrupt() => InterruptInternal();

        // Internal helper (since we can't place security demands on
        // ecalls/fcalls).
        [SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void InterruptInternal();

        /*=========================================================================
        ** Waits for the thread to die or for timeout milliseconds to elapse.
        ** Returns true if the thread died, or false if the wait timed out. If
        ** Timeout.Infinite is given as the parameter, no timeout will occur.
        **
        ** Exceptions: ArgumentException if timeout < 0.
        **             ThreadInterruptedException if the thread is interrupted while waiting.
        **             ThreadStateException if the thread has not been started yet.
        =========================================================================*/
        [SecuritySafeCritical]
        [HostProtection(Synchronization = true, ExternalThreading = true)]
        public void Join() => JoinInternal(Timeout.Infinite);

        [SecuritySafeCritical]
        [HostProtection(Synchronization = true, ExternalThreading = true)]
        public bool Join(int millisecondsTimeout) => JoinInternal(millisecondsTimeout);

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool JoinInternal(int millisecondsTimeout);

        public static void Sleep(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);
        public static void SpinWait(int iterations) => Thread.SpinWait(iterations);
        public static bool Yield() => Thread.Yield();

        public void Start() => AsThread().Start();
        public void Start(object parameter) => AsThread().Start(parameter);
    }
}
