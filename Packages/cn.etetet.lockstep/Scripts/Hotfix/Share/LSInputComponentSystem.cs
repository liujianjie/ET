using System;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSInputComponent))]
    [LSEntitySystemOf(typeof(LSInputComponent))]
    public static partial class LSInputComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSInputComponent self)
        {

        }
        
        [LSEntitySystem]
        private static void LSUpdate(this LSInputComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();

            FP baseSpeed = 6;
            FP speed = self.LSInput.IsSprint ? baseSpeed * 2 : baseSpeed;

            Log.Info($"self.LSInput.IsSprint {self.LSInput.IsSprint} , speed: {speed}");

            TSVector2 v2 = self.LSInput.V * speed * FP.EN2;// 50ms一帧，等价于 * 0.05
            if (v2.LengthSquared() > 0.0001f)
            {
                TSVector oldPos = unit.Position;
                unit.Position += new TSVector(v2.x, 0, v2.y);
                unit.Forward = unit.Position - oldPos;
            }

            if (self.LSInput.IsJump && unit.IsOnGround)
            {
                unit.VerticalSpeed = 8;
                unit.IsOnGround = false;
            }

            if (!unit.IsOnGround)
            {
                FP gravity = -20 * FP.EN2;
                unit.VerticalSpeed += gravity;

                TSVector pos = unit.Position;
                pos.y += unit.VerticalSpeed * FP.EN2;
                unit.Position = pos;

                if (pos.y <= 0)
                {
                    pos.y = 0;
                    unit.Position = pos;
                    unit.VerticalSpeed = FP.Zero;
                    unit.IsOnGround = true;
                }
            }
        }
    }
}