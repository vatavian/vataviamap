using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace GPS_API
{
    public class RIL
    {
        // string used to store the CellID string
        private static string celltowerinfo = "";
        private static RILCELLTOWERINFO rilCellTowerInfo;

        /*
         * Uses RIL to get CellID from the phone.
         */ 
        public static string GetCellTowerString()
        {
            // initialise handles
            IntPtr hRil = IntPtr.Zero;
            IntPtr hRes = IntPtr.Zero;

            // initialise result
            celltowerinfo = "";

            // initialise RIL
            hRes = RIL_Initialize(1,                                        // RIL port 1
                                  new RILRESULTCALLBACK(rilResultCallback), // function to call with result
                                  null,                                     // function to call with notify
                                  0,                                        // classes of notification to enable
                                  0,                                        // RIL parameters
                                  out hRil);                                // RIL handle returned

            if (hRes != IntPtr.Zero)
            {
                return "0.0.0.0";
            }

            // initialised successfully

            // use RIL to get cell tower info with the RIL handle just created
            hRes = RIL_GetCellTowerInfo(hRil);

            // wait for cell tower info to be returned
            waithandle.WaitOne();

            // finished - release the RIL handle
            RIL_Deinitialize(hRil);

            // return the result from GetCellTowerInfo
            return celltowerinfo;
        }

        /*
         * Uses RIL to get CellID from the phone.
         */
        public static RILCELLTOWERINFO GetCellTowerInfo()
        {
            // initialise handles
            IntPtr hRil = IntPtr.Zero;
            IntPtr hRes = IntPtr.Zero;

            // initialise result
            celltowerinfo = "";

            // initialise RIL
            hRes = RIL_Initialize(1,                                        // RIL port 1
                                  new RILRESULTCALLBACK(rilResultCallback), // function to call with result
                                  null,                                     // function to call with notify
                                  0,                                        // classes of notification to enable
                                  0,                                        // RIL parameters
                                  out hRil);                                // RIL handle returned

            if (hRes != IntPtr.Zero)
            {
                rilCellTowerInfo = new RILCELLTOWERINFO();
                return rilCellTowerInfo;
            }

            // initialised successfully

            // use RIL to get cell tower info with the RIL handle just created
            hRes = RIL_GetCellTowerInfo(hRil);

            // wait for cell tower info to be returned
            waithandle.WaitOne();

            // finished - release the RIL handle
            RIL_Deinitialize(hRil);

            // return the result from GetCellTowerInfo
            return rilCellTowerInfo;
        }

        // event used to notify user function that a response has
        //  been received from RIL
        private static AutoResetEvent waithandle = new AutoResetEvent(false);

        
        public static void rilResultCallback(uint dwCode, 
                                             IntPtr hrCmdID, 
                                             IntPtr lpData, 
                                             uint cbData, 
                                             uint dwParam)
        {
            // create empty structure to store cell tower info in
            rilCellTowerInfo = new RILCELLTOWERINFO();

            // copy result returned from RIL into structure
            Marshal.PtrToStructure(lpData, rilCellTowerInfo);

            // get the bits out of the RIL cell tower response that we want
            celltowerinfo = rilCellTowerInfo.dwCellID + "." +
                            rilCellTowerInfo.dwLocationAreaCode + "." +
                            rilCellTowerInfo.dwMobileCountryCode + "." +
                            rilCellTowerInfo.dwMobileNetworkCode;

            // notify caller function that we have a result
            waithandle.Set();
        }



        // -------------------------------------------------------------------
        //  RIL function definitions
        // -------------------------------------------------------------------

        /* 
         * Function definition converted from the definition 
         *  RILRESULTCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa920069.aspx
         */
        public delegate void RILRESULTCALLBACK(uint dwCode,
                                               IntPtr hrCmdID,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);


        /*
         * Function definition converted from the definition 
         *  RILNOTIFYCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa922465.aspx
         */
        public delegate void RILNOTIFYCALLBACK(uint dwCode,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);

        /*
         * Class definition converted from the struct definition 
         *  RILCELLTOWERINFO from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa921533.aspx
         */
        public class RILCELLTOWERINFO
        {
            public uint cbSize = 0;                    // Structure size, in bytes.
            public uint dwParams = 0;                  // valid parameters. Must be one or a combination of the RILCELLTOWERINFO parameter constants
            public uint dwMobileCountryCode = 0;       // country/region code        (MCC)
            public uint dwMobileNetworkCode = 0;       // code of the mobile network (MNC)
            public uint dwLocationAreaCode = 0;        // area code of the current location
            public uint dwCellID = 0;                  // ID of the cellular tower
            public uint dwBaseStationID = 0;           // ID of the base station
            public uint dwBroadcastControlChannel = 0; // Broadcast Control Channel (BCCH)
            public uint dwRxLevel = 0;                 // received signal level
            public uint dwRxLevelFull = 0;             // received signal level in the full network
            public uint dwRxLevelSub = 0;              // received signal level in the subsystem
            public uint dwRxQuality = 0;               // received signal quality
            public uint dwRxQualityFull = 0;           // received signal quality in the full network
            public uint dwRxQualitySub = 0;            // received signal quality in the subsystem
            public uint dwIdleTimeSlot = 0;            // idle timeslot
            public uint dwTimingAdvance = 0;           // timing advance
            public uint dwGPRSCellID = 0;              // ID of the GPRS cellular tower
            public uint dwGPRSBaseStationID = 0;       // ID of the GPRS base station
            public uint dwNumBCCH = 0;                 // number of the BCCH
        }

        // -------------------------------------------------------------------
        //  RIL DLL functions 
        // -------------------------------------------------------------------

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919106.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Initialize(uint dwIndex, 
                                                    RILRESULTCALLBACK pfnResult, 
                                                    RILNOTIFYCALLBACK pfnNotify, 
                                                    uint dwNotificationClasses, 
                                                    uint dwParam, 
                                                    out IntPtr lphRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa923065.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_GetCellTowerInfo(IntPtr hRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919624.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Deinitialize(IntPtr hRil);
    }
}
