using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TwinCAT.Ads.Server;
using TwinCAT.Ads;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TwinCAT_XBox_Controller_Service
{
    /*
     * Extend the TcAdsServer class to implement your own ADS server.
     */
    public class AdsXBoxServer : AdsServer
    {

        //const string DLL_PATH = "XBox-XInput.dll";
        const string DLL_PATH = @"C:\Users\John Helfrich\source\repos\TwinCAT-XBox-Controller-Service\x64\Debug\XBox-XInput.dll";

        [DllImport(DLL_PATH)]
        static public extern IntPtr CreateGamepad(int GamepadIndex);

        [DllImport(DLL_PATH)]
        static public extern void DisposeGamepad(IntPtr pGamepadObject);

        [DllImport(DLL_PATH)]
        static public extern int getIndex(IntPtr pGamepadObject);

        [DllImport(DLL_PATH)]
        static public extern bool getConnected(IntPtr pGamepadObject);


        /* Joystick Varaible Struct */
        private struct XBox_Controller_Joystick
        {
            public float x;
            public float y;
        }
        /* XBox/ADS Input Variable Struct */
        private struct ADS_XBox_Inputs
        {
            public int gamepadIndex;
            public XBox_Controller_Joystick left_Joystick;
            public XBox_Controller_Joystick right_Joystick;
            public float left_Trigger;
            public float right_Trigger;
            public short buttons;
            public short state;
        }

        private ADS_XBox_Inputs _xbox_inputs;
        private IServerLogger _serverLogger;
        private IntPtr[] _pGamepads;


        /* Instanstiate an ADS server with a fix ADS port assigned by the ADS router.
        */
        public AdsXBoxServer(ushort port, string portName, ILogger logger) : base(port, portName, logger)
        {
            _serverLogger = new ServerLogger(logger);

        }

        /* Diagnostics output for the server
        */
        protected override void OnConnected()
        {

            _serverLogger.Logger.LogInformation($"Server '{this.GetType()}', Address: {base.ServerAddress} connected!");
            _pGamepads = new IntPtr[] { CreateGamepad(0), CreateGamepad(1), CreateGamepad(2), CreateGamepad(3) };

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("Gamepad Number:\t\t" + (getIndex(_pGamepads[i]) + 1));
                Console.WriteLine("Gamepad Connected?\t" + getConnected(_pGamepads[i]));

                DisposeGamepad(_pGamepads[i]);
                _pGamepads[i] = IntPtr.Zero;
            }
        }

        /* Writing values from the client to this server
         */
        protected override Task<ResultWrite> OnWriteAsync(uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            ResultWrite result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);
        
            switch (indexGroup) /* use index group (and offset) to distinguish between the services
                                    of this server */
            {
                case 0x10000:
                    if (writeData.Length == 4)
                    {
                        //writeData.CopyTo(_dataBuffer.AsMemory(0, 4));
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
                    }
                    break;
                case 0x20000: /* used for the PLC Sample */
                    if (writeData.Length == 4)
                    {
                        uint value = BinaryPrimitives.ReadUInt32LittleEndian(writeData.Span.Slice(0, 4));
        
                        //if (_serverLogger != null)
                        //{
                        //    _serverLogger.Log(String.Format("PLC Counter: {0}", value));
                        //}
                        result = ResultWrite.CreateSuccess();
                    }
        
                    break;
        
                default: /* other services are not supported */
                    result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);
                    break;
            }
            return Task.FromResult(result);
        }

        /* Reading values from the server to a client
        */
        protected override Task<ResultReadBytes> OnReadAsync(uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {
            /* Distinguish between services like in AdsWriteInd */

            ResultReadBytes result = ResultReadBytes.CreateSuccess(getXBoxInputBytes(_xbox_inputs).AsMemory());
            return Task.FromResult(result);
        }

        /* Converts the XBox Data Structure to byte array
         */
        private byte[] getXBoxInputBytes(ADS_XBox_Inputs xbox_input_data)
        {
            int size = Marshal.SizeOf(xbox_input_data);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(xbox_input_data, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        private static short SetBit(short input, int bit)
        {
            return (short)(input | (1 << bit));
        }

        private static short ClearBit(short input, int bit)
        {
            return (short)(input & ~(1 << bit));
        }

        public void UpdateXboxValues()
        {
            Console.WriteLine("Updating the values");
        }

    }
}