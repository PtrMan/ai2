using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

using OpenCL.Net;

namespace ComputationBackend.OpenCl
{
    class ComputeContext
    {
        [Serializable]
        public class OpenClError : Exception
        {
            // Constructors
            public OpenClError()
                : base("OpenClError")
            { }

            // Ensure Exception is Serializable
            protected OpenClError(SerializationInfo info, StreamingContext ctxt)
                : base(info, ctxt)
            { }
        }



        public void initialize()
        {
            ErrorCode errorCode;
            

            // http://www.codeproject.com/Articles/502829/GPGPU-image-processing-basics-using-OpenCL-NET
            // license  The Code Project Open License (CPOL)
            // snip ===

            OpenCL.Net.Platform[] platforms = Cl.GetPlatformIDs(out errorCode);
            if (errorCode != ErrorCode.Success)
            {
                throw new OpenClError();
            }

            List<OpenCL.Net.Device> devicesList = new List<OpenCL.Net.Device>();


            foreach (OpenCL.Net.Platform platform in platforms)
            {
                string platformName = Cl.GetPlatformInfo(platform, OpenCL.Net.PlatformInfo.Name, out errorCode).ToString();
                if (errorCode != ErrorCode.Success)
                {
                    throw new OpenClError();
                }

                Console.WriteLine("Platform: " + platformName);

                //We will be looking only for GPU devices
                foreach (OpenCL.Net.Device device in Cl.GetDeviceIDs(platform, OpenCL.Net.DeviceType.Gpu, out errorCode))
                {
                    if (errorCode != ErrorCode.Success)
                    {
                        throw new OpenClError();
                    }

                    Console.WriteLine("Device: " + device.ToString());
                    devicesList.Add(device);
                }
            }

            if (devicesList.Count <= 0)
            {
                Console.WriteLine("No devices found.");
                throw new OpenClError();
            }

            chosenDevice = devicesList[1];
            // end snip ===


            context = Cl.CreateContext(null, 1, new Device[] { chosenDevice }, null, IntPtr.Zero, out errorCode);
            if (errorCode != ErrorCode.Success)
            {
                throw new OpenClError();
            }

            commandQueue = Cl.CreateCommandQueue(context, chosenDevice, CommandQueueProperties.OutOfOrderExecModeEnable, out errorCode);
            if (errorCode != ErrorCode.Success)
            {
                throw new OpenClError();
            }
        }

        public static void throwErrorIfNotSuccessfull(ErrorCode errorCode, string message)
        {
            if (errorCode != ErrorCode.Success)
            {
                throw new OpenClError();
            }
        }

        public OpenCL.Net.Context context;
        public OpenCL.Net.CommandQueue commandQueue;

        public OpenCL.Net.Device chosenDevice;
    }
}
