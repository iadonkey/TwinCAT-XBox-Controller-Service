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

namespace TwinCAT_XBox_Controller_Service
{
    /*
     * Extend the TcAdsServer class to implement ADS Server
     */
    public class AdsXBoxServer : AdsServer
    {
        /*
         * Interop methods - Calling C++ based DLL for XInput Services
         * 
         */
        // DLL Import Path
        const string DLL_PATH = "XBox-XInput.dll";

        [DllImport(DLL_PATH)]
        static public extern IntPtr CreateGamepad(int GamepadIndex);            // Create a gamepad
        [DllImport(DLL_PATH)]
        static public extern void DisposeGamepad(IntPtr pGamepadObject);        // Destroy the gamepad instance
        [DllImport(DLL_PATH)]
        static public extern int GetIndex(IntPtr pGamepadObject);               // Return gamepad index
        [DllImport(DLL_PATH)]
        static public extern bool GetConnected(IntPtr pGamepadObject);          // Return true if gamepad is connected
        [DllImport(DLL_PATH)]
        static public extern void UpdateInputs(IntPtr pGamepadObject);          // Updates the m_State for the controller at that moment
        [DllImport(DLL_PATH)]
        static public extern float GetLeftStick_X(IntPtr pGamepadObject);       // Returns the X value for the left stick
        [DllImport(DLL_PATH)]
        static public extern float GetLeftStick_Y(IntPtr pGamepadObject);       // Returns the Y value for the left stick
        [DllImport(DLL_PATH)]
        static public extern float GetRightStick_X(IntPtr pGamepadObject);      // Returns the X value for the right stick
        [DllImport(DLL_PATH)]
        static public extern float GetRightStick_Y(IntPtr pGamepadObject);      // Returns the Y value for the right stick
        [DllImport(DLL_PATH)]
        static public extern float GetLeftTrigger(IntPtr pGamepadObject);       // Returns the left trigger value
        [DllImport(DLL_PATH)]
        static public extern float GetRightTrigger(IntPtr pGamepadObject);      // Returns the right trigger value
        [DllImport(DLL_PATH)]
        static public extern short GetGamepadButtons(IntPtr pGamepadObject);    // Returns the button values
        [DllImport(DLL_PATH)]
        static public extern short GetGamepadStates(IntPtr pGamepadObject);     // Returns information about the gamepad state
        [DllImport(DLL_PATH)]
        static public extern void SetRumble(IntPtr pGamepadObject, float leftMotor = 0.0f, float rightMotor = 0.0f);

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

        private IServerLogger serverLogger;
        private IntPtr[] pGamepads;

        /* Instanstiate an ADS server with a fix ADS port assigned by the ADS router.
        */
        public AdsXBoxServer(ushort port, string portName, ILogger logger) : base(port, portName, logger)
        {
            serverLogger = new ServerLogger(logger);
            pGamepads = new IntPtr[] { CreateGamepad(0), CreateGamepad(1), CreateGamepad(2), CreateGamepad(3) };
        }

        ~AdsXBoxServer()
        {
            // Destruct Gamepads
            for (int i = 0; i < 4; i++)
            {
                DisposeGamepad(pGamepads[i]);
                pGamepads[i] = IntPtr.Zero;
            }
        }


        /* Diagnostics output for the server
        */
        protected override void OnConnected()
        {
            serverLogger.Logger.LogInformation($"Server '{this.GetType()}', Address: {base.ServerAddress} connected!");
        }

        /* Writing values from the client to this server
         */
        protected override Task<ResultWrite> OnWriteAsync(uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            ResultWrite result = ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported);
            /* use index group (and offset) to distinguish between the servicesof this server */
            switch (indexGroup + indexOffset) 
            {
                case 0x10010:
                    if (writeData.Length == 8 && GetConnected(pGamepads[0]))
                    {
                        byte[] _dataBuffer = new byte[8];
                        writeData.CopyTo(_dataBuffer.AsMemory(0, 8));
                        float left_Motor = System.BitConverter.ToSingle(_dataBuffer, 0);
                        float right_Motor = System.BitConverter.ToSingle(_dataBuffer, 4);
                        SetRumble(pGamepads[0], left_Motor / 100.0f, right_Motor / 100.0f);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
                    }
                    break;
                case 0x20010:
                    if (writeData.Length == 8 && GetConnected(pGamepads[1]))
                    {
                        byte[] _dataBuffer = new byte[8];
                        writeData.CopyTo(_dataBuffer.AsMemory(0, 8));
                        float left_Motor = System.BitConverter.ToSingle(_dataBuffer, 0);
                        float right_Motor = System.BitConverter.ToSingle(_dataBuffer, 4);
                        SetRumble(pGamepads[1], left_Motor / 100.0f, right_Motor / 100.0f);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
                    }
                    break;
                case 0x30010:
                    if (writeData.Length == 8 && GetConnected(pGamepads[2]))
                    {
                        byte[] _dataBuffer = new byte[8];
                        writeData.CopyTo(_dataBuffer.AsMemory(0, 8));
                        float left_Motor = System.BitConverter.ToSingle(_dataBuffer, 0);
                        float right_Motor = System.BitConverter.ToSingle(_dataBuffer, 4);
                        SetRumble(pGamepads[2], left_Motor / 100.0f, right_Motor / 100.0f);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
                    }
                    break;
                case 0x40010:
                    if (writeData.Length == 8 && GetConnected(pGamepads[3]))
                    {
                        byte[] _dataBuffer = new byte[8];
                        writeData.CopyTo(_dataBuffer.AsMemory(0, 8));
                        float left_Motor = System.BitConverter.ToSingle(_dataBuffer, 0);
                        float right_Motor = System.BitConverter.ToSingle(_dataBuffer, 4);
                        SetRumble(pGamepads[3], left_Motor / 100.0f, right_Motor / 100.0f);
                        result = ResultWrite.CreateSuccess();
                    }
                    else
                    {
                        result = ResultWrite.CreateError(AdsErrorCode.DeviceInvalidParam);
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
            ResultReadBytes result;
            /* use index group (and offset) to distinguish between the servicesof this server */
            switch (indexGroup) 
            {
                case 0x10000:
                    result = ResultReadBytes.CreateSuccess(ConvertXBoxInputBytes(GetXboxValues(pGamepads[0])).AsMemory());
                    break;
                case 0x20000:
                    result = ResultReadBytes.CreateSuccess(ConvertXBoxInputBytes(GetXboxValues(pGamepads[1])).AsMemory());
                    break;
                case 0x30000:
                    result = ResultReadBytes.CreateSuccess(ConvertXBoxInputBytes(GetXboxValues(pGamepads[2])).AsMemory());
                    break;
                case 0x40000:
                    result = ResultReadBytes.CreateSuccess(ConvertXBoxInputBytes(GetXboxValues(pGamepads[3])).AsMemory());
                    break;
                default: /* other services are not supported */
                    result = ResultReadBytes.CreateError(AdsErrorCode.DeviceInvalidGroup);        
                    break;
            }
            return Task.FromResult(result);
        }

        /* Converts the XBox Data Structure to byte array
         */
        private byte[] ConvertXBoxInputBytes(ADS_XBox_Inputs xbox_input_data)
        {
            int size = Marshal.SizeOf(xbox_input_data);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(xbox_input_data, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        /* Call the XInput methods to get the Gamepad status
        */
        private ADS_XBox_Inputs GetXboxValues(IntPtr pGamepads)
        {
            ADS_XBox_Inputs xbox_inputs = new ADS_XBox_Inputs();
            if (GetConnected(pGamepads))
            {
                UpdateInputs(pGamepads);
                xbox_inputs.gamepadIndex = GetIndex(pGamepads) + 1;
                xbox_inputs.left_Joystick.x = GetLeftStick_X(pGamepads) * 100.0f;
                xbox_inputs.left_Joystick.y = GetLeftStick_Y(pGamepads) * 100.0f;
                xbox_inputs.right_Joystick.x = GetRightStick_X(pGamepads) * 100.0f;
                xbox_inputs.right_Joystick.y = GetRightStick_Y(pGamepads) * 100.0f;
                xbox_inputs.left_Trigger = GetLeftTrigger(pGamepads) * 100.0f;
                xbox_inputs.right_Trigger = GetRightTrigger(pGamepads) * 100.0f;
                xbox_inputs.buttons = GetGamepadButtons(pGamepads);
                xbox_inputs.state = GetGamepadStates(pGamepads);
            }
            return xbox_inputs;
        }

    }
}