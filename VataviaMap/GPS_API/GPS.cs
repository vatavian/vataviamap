//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//
// Use of this sample source code is subject to the terms of the Microsoft
// license agreement under which you licensed this sample source code. If
// you did not accept the terms of the license agreement, you are not
// authorized to use this sample source code. For the terms of the license,
// please see the license agreement between you and Microsoft or, if applicable,
// see the LICENSE.RTF on your install media or the root of your tools installation.
// THE SAMPLE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;


namespace GPS_API
{
    public delegate void LocationChangedEventHandler(object sender, LocationChangedEventArgs args);
    public delegate void DeviceStateChangedEventHandler(object sender, DeviceStateChangedEventArgs args);

    /// <summary>
    /// Summary description for GPS.
    /// </summary>
    public class GPS
    {
        // handle to the gps device
        IntPtr gpsHandle = IntPtr.Zero;

        // handle to the native event that is signalled when the GPS
        // devices gets a new location
        IntPtr newLocationHandle = IntPtr.Zero;

        // handle to the native event that is signalled when the GPS
        // device state changes
        IntPtr deviceStateChangedHandle = IntPtr.Zero;

        // handle to the native event that we use to stop our event
        bool listening = false;

        // holds our event thread instance
        System.Threading.Thread gpsEventThread = null;


        event LocationChangedEventHandler locationChanged;

        /// <summary>
        /// Event that is raised when the GPS locaction data changes
        /// </summary>
        public event LocationChangedEventHandler LocationChanged
        {
            add
            {
                locationChanged += value;

                // create our event thread only if the user decides to listen
                CreateGpsEventThread();
            }
            remove
            {
                locationChanged -= value;
            }
        }


        event DeviceStateChangedEventHandler deviceStateChanged;

        /// <summary>
        /// Event that is raised when the GPS device state changes
        /// </summary>
        public event DeviceStateChangedEventHandler DeviceStateChanged
        {
            add
            {
                deviceStateChanged += value;

                // create our event thread only if the user decides to listen
                CreateGpsEventThread();
            }
            remove
            {
                deviceStateChanged -= value;
            }
        }

        /// <summary>
        /// True: The GPS device has been opened. False: It has not been opened
        /// </summary>
        public bool Opened
        {
            get { return gpsHandle != IntPtr.Zero; }
        }

        public GPS()
        {
        }

        ~GPS()
        {
            // make sure that the GPS was closed.
            Close();
        }

        /// <summary>
        /// Opens the GPS device and prepares to receive data from it.
        /// </summary>
        public void Open()
        {
            if (!Opened)
            {
                // create handles for GPS events
                newLocationHandle = CreateEvent(IntPtr.Zero, 0, 0, null);
                deviceStateChangedHandle = CreateEvent(IntPtr.Zero, 0, 0, null);
                listening = true;

                gpsHandle = GPSOpenDevice(newLocationHandle, deviceStateChangedHandle, null, 0);

                // if events were hooked up before the device was opened, we'll need
                // to create the gps event thread.
                if (locationChanged != null || deviceStateChanged != null)
                {
                    CreateGpsEventThread();
                }
            }
        }

        /// <summary>
        /// Closes the gps device.
        /// </summary>
        public void Close()
        {
            listening = false;

            System.Threading.Thread.Sleep(500);
            
            if (gpsHandle != IntPtr.Zero)
            {
                GPSCloseDevice(gpsHandle);
                gpsHandle = IntPtr.Zero;
            }

            // block until our event thread is finished before
            // we close our native event handles

            //below lock was often blocking forever, so now we try sleeping "long enough" instead
            System.Threading.Thread.Sleep(1000);
            //lock (this)
            {
                if (newLocationHandle != IntPtr.Zero)
                {
                    CloseHandle(newLocationHandle);
                    newLocationHandle = IntPtr.Zero;
                }

                if (deviceStateChangedHandle != IntPtr.Zero)
                {
                    CloseHandle(deviceStateChangedHandle);
                    deviceStateChangedHandle = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Get the position reported by the GPS receiver
        /// </summary>
        /// <returns>GpsPosition class with all the position details</returns>
        public GpsPosition GetPosition()
        {
            return GetPosition(TimeSpan.Zero);
        }


        /// <summary>
        /// Get the position reported by the GPS receiver that is no older than
        /// the maxAge passed in
        /// </summary>
        /// <param name="maxAge">Max age of the gps position data that you want back. 
        /// If there is no data within the required age, null is returned.  
        /// if maxAge == TimeSpan.Zero, then the age of the data is ignored</param>
        /// <returns>GpsPosition class with all the position details</returns>
        public GpsPosition GetPosition(TimeSpan maxAge)
        {
            GpsPosition gpsPosition = null;
            if (Opened)
            {
                // allocate the necessary memory on the native side.  We have a class (GpsPosition) that 
                // has the same memory layout as its native counterpart
                IntPtr ptr = Utils.LocalAlloc(Marshal.SizeOf(typeof(GpsPosition)));

                // fill in the required fields 
                gpsPosition = new GpsPosition();
                gpsPosition.dwVersion = 1;
                gpsPosition.dwSize = Marshal.SizeOf(typeof(GpsPosition));

                // Marshal our data to the native pointer we allocated.
                Marshal.StructureToPtr(gpsPosition, ptr, false);

                // call native method passing in our native buffer
                int dwMaxAge;
                if (maxAge == TimeSpan.Zero)
                    dwMaxAge = Int32.MaxValue;
                else
                    dwMaxAge = (int)maxAge.TotalMilliseconds;

                if (GPSGetPosition(gpsHandle, ptr, dwMaxAge, 0) == 0)
                {
                    // native call succeeded, marshal native data to our managed data
                    gpsPosition = (GpsPosition)Marshal.PtrToStructure(ptr, typeof(GpsPosition));

                    if (maxAge != TimeSpan.Zero)
                    {
                        // check to see if the data is recent enough.
                        if (!gpsPosition.TimeValid || DateTime.UtcNow - maxAge > gpsPosition.Time)
                        {
                            gpsPosition = null;
                        }
                    }
                }

                // free our native memory
                Utils.LocalFree(ptr);
            }

            return gpsPosition;            
        }

        /// <summary>
        /// Queries the device state.
        /// </summary>
        /// <returns>Device state information</returns>
        public GpsDeviceState GetDeviceState()
        {
            GpsDeviceState device = null;

            // allocate a buffer on the native side.  Since the
            IntPtr pGpsDevice = Utils.LocalAlloc(GpsDeviceState.GpsDeviceStructureSize);
            
            // GPS_DEVICE structure has arrays of characters, it's easier to just
            // write directly into memory rather than create a managed structure with
            // the same layout.
            Marshal.WriteInt32(pGpsDevice, 1);	// write out GPS version of 1
            Marshal.WriteInt32(pGpsDevice, 4, GpsDeviceState.GpsDeviceStructureSize);	// write out dwSize of structure

            int result = GPSGetDeviceState(pGpsDevice);

            if (result == 0)
            {
                // instantiate the GpsDeviceState class passing in the native pointer
                device = new GpsDeviceState(pGpsDevice);
            }

            // free our native memory
            Utils.LocalFree(pGpsDevice);

            return device;
        }

        /// <summary>
        /// Creates our event thread that will receive native events
        /// </summary>
        private void CreateGpsEventThread()
        {
            // we only want to create the thread if we don't have one created already 
            // and we have opened the gps device
            if (gpsEventThread == null && gpsHandle != IntPtr.Zero)
            {
                // Create and start thread to listen for GPS events
                gpsEventThread = new System.Threading.Thread(new System.Threading.ThreadStart(WaitForGpsEvents));
                gpsEventThread.Start();
            }
        }

        /// <summary>
        /// Method used to listen for native events from the GPS. 
        /// </summary>
        private void WaitForGpsEvents()
        {
            lock (this)
            {
                // allocate 3 handles worth of memory to pass to WaitForMultipleObjects
                IntPtr handles = Utils.LocalAlloc(8);

                // write the handles we are listening for.
                Marshal.WriteInt32(handles, 0, deviceStateChangedHandle.ToInt32());
                Marshal.WriteInt32(handles, 4, newLocationHandle.ToInt32());

                while (listening)
                {
                    int obj = WaitForMultipleObjects(2, handles, 0, 2000);
                    switch (obj)
                    {
                        case 0:
                            // device state has changed
                            if (deviceStateChanged != null)
                            {
                                deviceStateChanged(this, new DeviceStateChangedEventArgs(GetDeviceState()));
                            }
                            break;
                        case 1:
                            // location has changed
                            if (locationChanged != null)
                            {
                                locationChanged(this, new LocationChangedEventArgs(GetPosition()));
                            }
                            break;
                    }
                }

                // free the memory we allocated for the native handles
                Utils.LocalFree(handles);

                // clear our gpsEventThread so that we can recreate this thread again
                // if the events are hooked up again.
                gpsEventThread = null;
            }
        }

        #region PInvokes to gpsapi.dll
        [DllImport("gpsapi.dll")]
        static extern IntPtr GPSOpenDevice(IntPtr hNewLocationData, IntPtr hDeviceStateChange, string szDeviceName, int dwFlags);

        [DllImport("gpsapi.dll")]
        static extern int  GPSCloseDevice(IntPtr hGPSDevice);

        [DllImport("gpsapi.dll")]
        static extern int  GPSGetPosition(IntPtr hGPSDevice, IntPtr pGPSPosition, int dwMaximumAge, int dwFlags);
        // dwMaximumAge = Maximum age, in milliseconds, of location information. 
        //                The GPS Intermediate Driver only returns information that has been received within the time specified by this parameter. 
        //                Any information that is older than this age is not returned. 
        //                The elements of the GPS_POSITION instance that are valid for the given dwMaximumAge value are specified in the 
        //                dwValidFields element of the instance.

        [DllImport("gpsapi.dll")]
        static extern int  GPSGetDeviceState(IntPtr pGPSDevice);
        #endregion

        #region PInvokes to coredll.dll
        [DllImport("coredll.dll")]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, int bManualReset, int bInitialState, StringBuilder lpName);

        [DllImport("coredll.dll")]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("coredll.dll")]
        static extern int WaitForMultipleObjects(int nCount, IntPtr lpHandles, int fWaitAll, int dwMilliseconds);

        const int eventSet = 3;
        [DllImport("coredll.dll")]
        static extern int EventModify(IntPtr hHandle, int dwFunc);
        
#endregion

    }
}
