using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV.Memory
{
    public unsafe class VehicleControl
    {
        private static int throttlePOffset;
        private static int brakePOffset;
        private static int handbrakeOffset;
        private static int steeringAngleOffset;
        private static int handlingOffset;

        private static int wheelsPtrOffset;
        private static int numWheelsOffset;

        private static int wheelSteeringAngleOffset;

        static VehicleControl()
        {
            byte* addr = Memory.FindPattern("\x74\x0A\xF3\x0F\x11\xB3\x1C\x09\x00\x00\xEB\x25", "xxxxxx????xx");
            throttlePOffset = addr == null ? 0 : *(int*)(addr + 6) + 0x10;
            brakePOffset = addr == null ? 0 : *(int*)(addr + 6) + 0x14;
            steeringAngleOffset = addr == null ? 0 : *(int*)(addr + 6) + 8;

            addr = Memory.FindPattern("\x44\x88\xA3\x00\x00\x00\x00\x45\x8A\xF4", "xxx????xxx");
            handbrakeOffset = addr == null ? 0 : *(int*)(addr + 3);

            addr = Memory.FindPattern("\x3C\x03\x0F\x85\x00\x00\x00\x00\x48\x8B\x41\x20\x48\x8B\x88", "xxxx????xxxxxxx");
            handlingOffset = addr == null ? 0 : *(int*)(addr + 0x16);

            addr = Memory.FindPattern("\x3B\xB7\x48\x0B\x00\x00\x7D\x0D", "xx????xx");
            wheelsPtrOffset = addr == null ? 0 : *(int*)(addr + 2) - 8;
            numWheelsOffset = addr == null ? 0 : *(int*)(addr + 2);

            addr = Memory.FindPattern("\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\xDA", "xx???xxxxxxx");
            wheelSteeringAngleOffset = addr == null ? 0 : *(int*)(addr + 3);
        }

        public static ulong GetHandlingPtr(Vehicle vehicle)
        {
            if (handlingOffset == 0) return 0;
            ulong address = (ulong)vehicle.MemoryAddress;
            return *(ulong*)(address + (ulong)handlingOffset);
        }

        public static ulong GetWheelsPtr(Vehicle vehicle)
        {
            if (wheelsPtrOffset == 0) return 0;
            ulong address = (ulong)vehicle.MemoryAddress;
            return *(ulong*)(address + (ulong)wheelsPtrOffset);
        }

        public static sbyte GetNumWheels(Vehicle vehicle)
        {
            if (numWheelsOffset == 0) return 0;
            sbyte* address = (sbyte*)((ulong)vehicle.MemoryAddress + (ulong)numWheelsOffset);
            return *address;
        }

        public static void SetThrottle(Vehicle vehicle, float throttle)
        {
            if (throttlePOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)throttlePOffset);
            *address = throttle;
        }

        public static float GetThrottle(Vehicle vehicle)
        {
            if (throttlePOffset == 0) return -1f;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)throttlePOffset);
            return *address;
        }

        public static void SetBrake(Vehicle vehicle, bool brake)
        {
            if (brakePOffset == 0) return;
            bool* address = (bool*)((ulong)vehicle.MemoryAddress + (ulong)brakePOffset);
            *address = brake;
        }

        public static bool GetBrake(Vehicle vehicle)
        {
            if (brakePOffset == 0) return false;
            bool* address = (bool*)((ulong)vehicle.MemoryAddress + (ulong)brakePOffset);
            return *address;
        }

        public static void SetSteeringAngle(Vehicle vehicle, float angle)
        {
            if (steeringAngleOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)steeringAngleOffset);
            *address = angle;
        }

        public static float GetSteeringAngle(Vehicle vehicle)
        {
            if (steeringAngleOffset == 0) return -999f;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)steeringAngleOffset);
            return *address;
        }

        public static float GetMaxSteeringAngle(Vehicle vehicle)
        {
            ulong handlingAddr = GetHandlingPtr(vehicle);
            if (handlingAddr == 0) return 0f;
            float* addr = (float*)(handlingAddr + (ulong)0x0080);
            return *addr;
        }

        public static float[] GetWheelSteeringAngles(Vehicle vehicle)
        {
            ulong wheelPtr = GetWheelsPtr(vehicle);
            sbyte numWheels = GetNumWheels(vehicle);

            float[] array = new float[numWheels];

            if (wheelSteeringAngleOffset == 0) return array;

            for(sbyte i = 0; i < numWheels; i++)
            {
                ulong wheelAddr = *(ulong*)(wheelPtr + 0x008 * (ulong)i);
                array[i] = *(float*)(wheelAddr + (ulong)wheelSteeringAngleOffset);
            }

            return array;
        }

        public static float GetLargestSteeringAngle(Vehicle v)
        {
            float largestAngle = 0.0f;
            float[] angles = GetWheelSteeringAngles(v);

            foreach(float angle in angles)
            {
                if (Math.Abs(angle) > Math.Abs(largestAngle))
                {
                    largestAngle = angle;
                }
            }

            return largestAngle;
        }

        public static float CalculateReduction(Vehicle vehicle)
        {
            float mult = 1;
            Vector3 vel = vehicle.Velocity;
            Vector3 pos = vehicle.Position;
            Vector3 motion = vehicle.GetOffsetInWorldCoords(new Vector3(pos.X + vel.X, pos.Y + vel.Y, pos.Z + vel.Z));
            if (motion.Y > 3)
            {
                mult = (0.15f + ((float)Math.Pow((1.0f / 1.13f), ((float)Math.Abs(motion.Y) - 7.2f))));
                if (mult != 0) { mult = (float)Math.Floor(mult * 1000) / 1000; }
                if (mult > 1) { mult = 1; }
            }
            mult = (1 + (mult - 1) * 1.0f);
            return mult;
        }

        public static float CalculateDesiredHeading(Vehicle vehicle, float steeringAngle, float steeringMax, float desiredHeading, float reduction)
        {
            float correction = desiredHeading * reduction;

            Vector3 speedVector = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, vehicle, true);

            if (Math.Abs(speedVector.Y) > 3.0f)
            {
                Vector3 velocityWorld = vehicle.Velocity;
                Vector3 positionWorld = vehicle.Position;
                Vector3 travelWorld = velocityWorld + positionWorld;

                float steeringAngleRelX = speedVector.Y * -(float)Math.Sin(steeringAngle);
                float steeringAngleRelY = speedVector.Y * (float)Math.Cos(steeringAngle);
                Vector3 steeringWorld = vehicle.GetOffsetInWorldCoords(new Vector3(steeringAngleRelX, steeringAngleRelY, 0.0f));

                Vector3 travelNorm = (travelWorld - positionWorld).Normalized;
                Vector3 steerNorm = (steeringWorld - positionWorld).Normalized;
                float travelDir = (float)Math.Atan2(travelNorm.Y, travelNorm.X) + desiredHeading * reduction;
                float steerDir = (float)Math.Atan2(steerNorm.Y, steerNorm.X);

                correction = 2.0f * (float)Math.Atan2(Math.Sin(travelDir - steerDir), (float)Math.Cos(travelDir - steerDir));
            }
            if (correction > steeringMax)
                correction = steeringMax;
            if (correction < -steeringMax)
                correction = -steeringMax;

            return correction;
        }

        public static void GetControls(float limitRadians, out bool handbrake, out float throttle, out bool brake, out float steer)
        {
            handbrake = Game.IsControlPressed(2, Control.VehicleHandbrake);
            throttle = -Game.GetControlNormal(2, Control.MoveUp);

            brake = Game.IsControlPressed(2, Control.MoveDown);
            float left = -Game.GetControlNormal(2, Control.MoveLeft);
            steer = left;
        }
    }
}
