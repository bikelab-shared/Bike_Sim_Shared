/*
 * Copyright (C) 2012-2021 Motion Systems
 * 
 * This file is part of ForceSeat motion system.
 *
 * www.motionsystems.eu
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;

namespace MotionSystems
{
	///
	/// This structure defines position of top frame (table) in logical units (percents).
	/// It does not use Inverse Kinematics module so rotation and movements are not always linear.
	///
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TopTablePositionLogical
	{
		// Put here sizeof(FSMI_TopTablePositionLogical).
		// NOTE: This field is mandatory.
		public byte structSize;

		// Set bits in the mask to tell the motion software which fields in this stucture are provided by your application.
		public uint mask;

		// Only single bit is used in current version.
		//  (state & 0x01) == 0 -> no pause
		//  (state & 0x01) == 1 -> paused
		// NOTE: If the field is provided, set FSMI_POS_BIT.STATE in the mask.
		public byte state;

		// NOTE: If below fields are provided, set FSMI_POS_BIT.POSITION in the mask.
		public short roll;       // -32k max left,   +32k max right
		public short pitch;      // -32k max rear,   +32k max front
		public short yaw;        // -32k max left,   +32k max right
		public short heave;      // -32k max bottom, +32k max top
		public short sway;       // -32k max left,   +32k max right
		public short surge;      // -32k max rear,   +32k max front

		// 0 to 64k, actual speed is not always equal to max speed due to ramps.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.MAX_SPEED in the mask.
		public ushort maxSpeed;

		// State of buttons, gun triggers, etc. It is passed directly to the motion platform.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.TRIGGERS in the mask.
		public byte triggers;

		// Custom use field that can be used in script in ForceSeatPM.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.AUX in the mask.
		public float aux1;
		public float aux2;
		public float aux3;
		public float aux4;
		public float aux5;
		public float aux6;
		public float aux7;
		public float aux8;
	}

	///
	/// This structure defines position of top frame (table) in physical units (rad, mm).
	/// It uses Inverse Kinematics module and it might not be supported by all motion platforms.
	/// By default BestMatch strategy is used.
	///
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TopTablePositionPhysical
	{
		// Put here sizeof(FSMI_TopTablePositionPhysical).
		// NOTE: This field is mandatory.
		public byte structSize;

		// Set bits in the mask to tell the motion software which fields in this stucture are provided by your application.
		public uint mask;

		// Only single bit is used in current version.
		//  (state & 0x01) == 0 -> no pause
		//  (state & 0x01) == 1 -> paused
		// NOTE: If the field is provided, set FSMI_POS_BIT.STATE in the mask.
		public byte state;

		// NOTE: If below fields are provided, set FSMI_POS_BIT.POSITION in the mask.
		public float roll;       // in radians, roll  < 0 = left,  roll > 0  = right
		public float pitch;      // in radians, pitch < 0 = front, pitch > 0 = rear
		public float yaw;        // in radians, yaw   < 0 = right, yaw > 0   = left
		public float heave;      // in mm, heave < 0 - down, heave > 0 - top
		public float sway;       // in mm, sway  < 0 - left, sway  > 0 - right
		public float surge;      // in mm, surge < 0 - rear, surge > 0 - front

		// 0 to 64k, actual speed is not always equal to max speed due to ramps.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.MAX_SPEED in the mask.
		public ushort maxSpeed;

		// State of buttons, gun triggers, etc. It is passed directly to the motion platform.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.TRIGGERS in the mask.
		public byte triggers;

		// Custom use field that can be used in script in ForceSeatPM.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.AUX in the mask.
		public float aux1;
		public float aux2;
		public float aux3;
		public float aux4;
		public float aux5;
		public float aux6;
		public float aux7;
		public float aux8;
	}

	///
	/// This structure defines position of top frame (table) in physical units (rad, mm) by specifing transformation matrix.
	/// It uses Inverse Kinematics module and it is dedicated for 6DoF motion platforms.
	/// If matrix transformation is specified, the Inverse Kinematics module always uses FullMatch strategy.
	///
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TopTableMatrixPhysical
	{
		// Put here sizeof(FSMI_TopTableMatrixPhysical).
		// NOTE: This field is mandatory.
		public byte structSize;

		// Set bits in the mask to tell the motion software which fields in this stucture are provided by your application.
		public uint mask;

		// Only single bit is used in current version.
		//  (state & 0x01) == 0 -> no pause
		//  (state & 0x01) == 1 -> paused
		// NOTE: If the field is provided, set FSMI_POS_BIT.STATE in the mask.
		public byte state;

		// 3D transformation matrix
		// OFFSET (in mm):
		// x axis = left-right movement, sway,  x < 0 - left, x > 0 - right
		// y axis = rear-front movement, surge, y < 0 - rear, y > 0 - front
		// z axis = down-top movement,   heave, z < 0 - down, z > 0 - top
		// 
		// ROTATION (in radians):
		// x axis, pitch = x < 0 = front, x > 0 = rear
		// y axis, roll = y < 0  = left,  y > 0 = right
		// z axis, yaw  = z < 0  = right, z > 0 = left
		//
		// NOTE: If below fields are provided, set FSMI_POS_BIT.MATRIX in the mask.
		public float m11;        // 3D transformation matrix, 1st row
		public float m12;        // 3D transformation matrix, 1st row
		public float m13;        // 3D transformation matrix, 1st row
		public float m14;        // 3D transformation matrix, 1st row
		
		public float m21;        // 3D transformation matrix, 2nd row
		public float m22;        // 3D transformation matrix, 2nd row
		public float m23;        // 3D transformation matrix, 2nd row
		public float m24;        // 3D transformation matrix, 2nd row
		
		public float m31;        // 3D transformation matrix, 3rd row
		public float m32;        // 3D transformation matrix, 3rd row
		public float m33;        // 3D transformation matrix, 3rd row
		public float m34;        // 3D transformation matrix, 3rd row
		
		public float m41;        // 3D transformation matrix, 4rd row
		public float m42;        // 3D transformation matrix, 4rd row
		public float m43;        // 3D transformation matrix, 4rd row
		public float m44;        // 3D transformation matrix, 4rd row

		// 0 to 64k, actual speed is not always equal to max speed due to ramps.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.MAX_SPEED in the mask.
		public ushort maxSpeed;

		// State of buttons, gun triggers, etc. It is passed directly to the motion platform.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.TRIGGERS in the mask.
		public byte triggers;

		// Custom use field that can be used in script in ForceSeatPM.
		// NOTE: If below fields are provided, set FSMI_POS_BIT.AUX in the mask.
		public float aux1;
		public float aux2;
		public float aux3;
		public float aux4;
		public float aux5;
		public float aux6;
		public float aux7;
		public float aux8;
	}

	/// Helpers for mask bits
	public struct FSMI_POS_BIT
	{
		public const int STATE     = (1 << 1);
		public const int POSITION  = (1 << 2);
		public const int MATRIX    = (1 << 2);
		public const int MAX_SPEED = (1 << 3);
		public const int TRIGGERS  = (1 << 4);
		public const int AUX       = (1 << 5);
	}
}
