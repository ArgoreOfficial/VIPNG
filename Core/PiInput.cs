using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PiInput
{

    public static KeyboardState CurrentKeyState;
    public static KeyboardState PreviousKeyState;
    public static MouseState CurrentMouseState;
    public static MouseState PreviousMouseState;

    public static void Update()
    {
        PreviousKeyState = CurrentKeyState;
        CurrentKeyState = Keyboard.GetState();

        PreviousMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();
    }

    public static Vector2 GetMouseDelta()
    {
        return CurrentMouseState.Position.ToVector2() - PreviousMouseState.Position.ToVector2();
    }

    public static bool LeftJustPressed()
    {
        return CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton != ButtonState.Pressed;
    }
    public static bool RightJustPressed()
    {
        return CurrentMouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton != ButtonState.Pressed;
    }
    public static bool MiddleJustPressed()
    {
        return CurrentMouseState.MiddleButton == ButtonState.Pressed && PreviousMouseState.MiddleButton != ButtonState.Pressed;
    }

    public static bool IsKeyPressed(Keys key, bool oneShot = false)
    {
        if (!oneShot) return CurrentKeyState.IsKeyDown(key);
        return CurrentKeyState.IsKeyDown(key) && !PreviousKeyState.IsKeyDown(key);
    }

}

