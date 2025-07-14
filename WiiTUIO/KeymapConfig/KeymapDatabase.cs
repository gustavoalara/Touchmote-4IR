using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiiTUIO.Properties;

namespace WiiTUIO
{
    class KeymapDatabase
    {
        public string DisableKey { get; private set; }

        private List<KeymapInput> allInputs;
        private List<KeymapOutput> allOutputs;

        private string DEFAULT_JSON_FILENAME = "default.json";
        private string CALIBRATION_JSON_FILENAME = "Calibration.json";

        private static KeymapDatabase currentInstance;
        public static KeymapDatabase Current
        {
            get
            {
                if (currentInstance == null)
                {
                    currentInstance = new KeymapDatabase();
                }
                return currentInstance;
            }
        }

        private KeymapDatabase()
        {
            this.DisableKey = "Deshabilitado";

            allInputs = new List<KeymapInput>();
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero", "Pointer", true, false, true));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Izquierdo", "PointerX-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Derecho", "PointerX+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Arriba", "PointerY-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Abajo", "PointerY+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "A", "A"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "B", "B"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Home", "Home"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Izquierda", "Left"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Derecha", "Right"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Arriba", "Up"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Aabajo", "Down"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Plus", "Plus"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Minus", "Minus"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Uno", "One"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Dos", "Two"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt X-", "AccelX-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt X+", "AccelX+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Y-", "AccelY-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Y+", "AccelY+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Z-", "AccelZ-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Z+", "AccelZ+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Shake", "Shake"));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Extensión", "Extension"));

            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "C", "Nunchuk.C"));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Z", "Nunchuk.Z"));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Arriba", "Nunchuk.StickUp", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Abajo", "Nunchuk.StickDown", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Izquierda", "Nunchuk.StickLeft", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Derecha", "Nunchuk.StickRight", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Rotación+", "Nunchuk.Rotation+"));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Rotación-", "Nunchuk.Rotation-"));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt X-", "Nunchuk.AccelX-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt X+", "Nunchuk.AccelX+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Y-", "Nunchuk.AccelY-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Y+", "Nunchuk.AccelY+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Z-", "Nunchuk.AccelZ-", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Z+", "Nunchuk.AccelZ+", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Shake", "Nunchuk.Shake"));


            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Izquierda", "Classic.Left"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Derecha", "Classic.Right"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Arriba", "Classic.Up"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Abajo", "Classic.Down"));

            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Izquierda", "Classic.StickLLeft", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Derecha", "Classic.StickLRight", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Arriba", "Classic.StickLUp", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Abajo", "Classic.StickLDown", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Izquierda", "Classic.StickRLeft", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Derecha", "Classic.StickRRight", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Arriba", "Classic.StickRUp", true, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Abajo", "Classic.StickRDown", true, true, false));

            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Minus", "Classic.Minus"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Plus", "Classic.Plus"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Home", "Classic.Home"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Y", "Classic.Y"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "X", "Classic.X"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "A", "Classic.A"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "B", "Classic.B"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Izq.", "Classic.TriggerL", false, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Der.", "Classic.TriggerR", false, true, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Izq. Pulsar", "Classic.L"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Der. Pulsar", "Classic.R"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "ZL", "Classic.ZL"));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "ZR", "Classic.ZR"));


            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero", "OffScreen.Pointer", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Izquierda", "OffScreen.PointerX-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Derecha", "OffScreen.PointerX+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Arriba", "OffScreen.PointerY-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.IR, "Puntero Abajo", "OffScreen.PointerY+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "A", "OffScreen.A", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "B", "OffScreen.B", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Home", "OffScreen.Home", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Izquierda", "OffScreen.Left", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Derecha", "OffScreen.Right", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Arriba", "OffScreen.Up", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Abajo", "OffScreen.Down", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Plus", "OffScreen.Plus", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Minus", "OffScreen.Minus", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Uno", "OffScreen.One", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Dos", "OffScreen.Two", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt X-", "OffScreen.AccelX-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt X+", "OffScreen.AccelX+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Y-", "OffScreen.AccelY-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Y+", "OffScreen.AccelY+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Z-", "OffScreen.AccelZ-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Tilt Z+", "OffScreen.AccelZ+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Shake", "OffScreen.Shake", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.WIIMOTE, "Extensión", "OffScreen.Extension", false));

            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "C", "OffScreen.Nunchuk.C", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Z", "OffScreen.Nunchuk.Z", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Arriba", "OffScreen.Nunchuk.StickUp", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Abajo", "OffScreen.Nunchuk.StickDown", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Izquierda", "OffScreen.Nunchuk.StickLeft", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Derecha", "OffScreen.Nunchuk.StickRight", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Rotación+", "OffScreen.Nunchuk.Rotation+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Stick Rotación-", "OffScreen.Nunchuk.Rotation-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt X-", "OffScreen.Nunchuk.AccelX-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt X+", "OffScreen.Nunchuk.AccelX+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Y-", "OffScreen.Nunchuk.AccelY-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Y+", "OffScreen.Nunchuk.AccelY+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Z-", "OffScreen.Nunchuk.AccelZ-", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Tilt Z+", "OffScreen.Nunchuk.AccelZ+", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.NUNCHUK, "Shake", "OffScreen.Nunchuk.Shake", false));

            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Izquierda", "OffScreen.Classic.Left", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Derecha", "OffScreen.Classic.Right", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Arriba", "OffScreen.Classic.Up", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Abajo", "OffScreen.Classic.Down", false));

            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Izquierda", "OffScreen.Classic.StickLLeft", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Derecha", "OffScreen.Classic.StickLRight", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Arriba", "OffScreen.Classic.StickLUp", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Izq. Abajo", "OffScreen.Classic.StickLDown", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Izquierda", "OffScreen.Classic.StickRLeft", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Derecha", "OffScreen.Classic.StickRRight", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Arriba", "OffScreen.Classic.StickRUp", true, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Stick Der. Abajo", "OffScreen.Classic.StickRDown", true, true, false, false));

            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Minus", "OffScreen.Classic.Minus", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Plus", "OffScreen.Classic.Plus", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Home", "OffScreen.Classic.Home", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Y", "OffScreen.Classic.Y", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "X", "OffScreen.Classic.X", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "A", "OffScreen.Classic.A", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "B", "OffScreen.Classic.B", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Izquierdo", "OffScreen.Classic.TriggerL", false, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Derecho", "OffScreen.Classic.TriggerR", false, true, false, false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Izq. Pulsar", "OffScreen.Classic.L", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "Gatillo Der. Pulsar", "OffScreen.Classic.R", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "ZL", "OffScreen.Classic.ZL", false));
            allInputs.Add(new KeymapInput(KeymapInputSource.CLASSIC, "ZR", "OffScreen.Classic.ZR", false));

            allOutputs = new List<KeymapOutput>();
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Cursor Ratón", "mouse", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón FPS", "fpsmouse", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Lightgun", "lightgunmouse", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Izquierdo", "mouseleft"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Medio", "mousemiddle"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Derecho", "mouseright"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Rueda Arriba", "mousewheelup"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Rueda Abajo", "mousewheeldown"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Mover Derecha", "mousex+", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Mover Arriba", "mousey+", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Mover Izquierda", "mousex-", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Mover Abajo", "mousey-", true, true, false, false));
            //allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Mouse Middle", "mbutton"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Extra 1", "mousexbutton1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.MOUSE, "Ratón Extra 2", "mousexbutton2"));


            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Tab", "tab"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Retroceso", "back"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Return", "return"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Mayúsculas", "shift"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Control", "control"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Alt", "menu"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Pause", "pause"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Caps Lock", "capital"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Escape", "escape"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Barra espaciadora", "space"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Pag. Arriba", "prior"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Pag. Abajo", "next"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Fin", "end"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Home", "home"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Flecha Izq.", "left"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Flecha Arriba", "up"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Flecha Der.", "right"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Flecha Abajo", "down"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Imprimir Pantalla", "snapshot"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Insertar", "insert"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Suprimir", "delete"));


            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "0", "vk_0"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "1", "vk_1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "2", "vk_2"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "3", "vk_3"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "4", "vk_4"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "5", "vk_5"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "6", "vk_6"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "7", "vk_7"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "8", "vk_8"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "9", "vk_9"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "A", "vk_a"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "B", "vk_b"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "C", "vk_c"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "D", "vk_d"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "E", "vk_e"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F", "vk_f"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "G", "vk_g"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "H", "vk_h"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "I", "vk_i"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "J", "vk_j"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "K", "vk_k"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "L", "vk_l"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "M", "vk_m"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "N", "vk_n"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "O", "vk_o"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "P", "vk_p"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Q", "vk_q"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "R", "vk_r"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "S", "vk_s"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "T", "vk_t"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "U", "vk_u"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "V", "vk_v"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "W", "vk_w"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "X", "vk_x"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Y", "vk_y"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Z", "vk_z"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Periodo .", "oem_period"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Coma ,", "oem_comma"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Win Izquierdo", "lwin"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Win Derecho", "rwin"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Apps / Menu", "apps"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 0", "numpad0"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 1", "numpad1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 2", "numpad2"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 3", "numpad3"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 4", "numpad4"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 5", "numpad5"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 6", "numpad6"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 7", "numpad7"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 8", "numpad8"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numpad 9", "numpad9"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Multiplicar *", "multiply"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Sumar +", "add"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Restar -", "subtract"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Decimal ,", "decimal"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Dividir /", "divide"));


            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F1", "f1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F2", "f2"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F3", "f3"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F4", "f4"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F5", "f5"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F6", "f6"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F7", "f7"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F8", "f8"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F9", "f9"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F10", "f10"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F11", "f11"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "F12", "f12"));


            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Numlock", "numlock"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Mayúsculas Izq.", "lshift"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Mayúsculas Der.", "rshift"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Control Izq.", "lcontrol"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Control Der.", "rcontrol"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Alt Izq.", "lmenu"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Alt Der.", "rmenu"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Atrás", "browser_back"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Adelante", "browser_forward"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Refrescar", "browser_refresh"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Stop", "browser_stop"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Buscar", "browser_search"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Favoritos", "browser_favorites"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Navegador Home", "browser_home"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Silenciar", "volume_mute"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Volumen +", "volume_up"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Volumen -", "volume_down"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Pista Siguiente", "media_next_track"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Pista Anterior", "media_prev_track"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Stop Media", "media_stop"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Repr./Pausar Media", "media_play_pause"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.KEYBOARD, "Zoom", "zoom"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "A", "360.a"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "B", "360.b"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "X", "360.x"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Y", "360.y"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Izquierda", "360.left", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Derecha", "360.right", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Arriba", "360.up", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Abajo", "360.down", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Back", "360.back"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Start", "360.start"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Guide", "360.guide"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Pulsar", "360.stickpressl"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Pulsar", "360.stickpressr"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq.", "360.stickl", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Light", "360.stickl-light", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Light 4:3", "360.stickl-light-4:3", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Arriba", "360.sticklup", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Abajo", "360.stickldown", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Izq.", "360.sticklleft", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq. Der.", "360.sticklright", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Izq.Centro", "360.sticklcenter"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der.", "360.stickr", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Light", "360.stickr-light", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Light 4:3", "360.stickr-light-4:3", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Arriba", "360.stickrup", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Abajo", "360.stickrdown", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Izq.", "360.stickrleft", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Der.", "360.stickrright", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Stick Der. Centro", "360.stickrcenter"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Gatillo Izq.", "360.triggerl", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Gatillo Der.", "360.triggerr", true, true, false, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Bumper Izq.", "360.bumperl"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.XINPUT, "Bumper Der.", "360.bumperr"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.CURSOR, "Cursor", "cursor", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.CURSOR, "Lightgun Cursor", "lightguncursor", false, false, true, false));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.CURSOR, "Pulsar Cursor", "cursorpress"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Rumble Corto", "rumbleshort"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Rumble Largo", "rumblelong"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Rumble Contínuo", "rumblehold"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Rumble Alternado", "rumblealt"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "LED 1", "led1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "LED 2", "led2"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "LED 3", "led3"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "LED 4", "led4"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Altavoz Sonido 1", "sound1"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Altavoz Sonido 2", "sound2"));
            allOutputs.Add(new KeymapOutput(KeymapOutputType.WIIMOTE, "Altavoz Sonido Bucle", "loop"));

            allOutputs.Add(new KeymapOutput(KeymapOutputType.DISABLE, "Deshabilitado", this.DisableKey));
        }


        public KeymapSettings getKeymapSettings()
        {
            return new KeymapSettings(Settings.Default.keymaps_config);
        }

        public List<Keymap> getAllKeymaps()
        {
            List<Keymap> list = new List<Keymap>();
            string[] files = Directory.GetFiles(Settings.Default.keymaps_path, "*.json");
            string defaultKeymapFilename = this.getKeymapSettings().getDefaultKeymap();

            Keymap defaultKeymap = new Keymap(null, defaultKeymapFilename);
            list.Add(defaultKeymap);

            foreach (string filepath in files)
            {
                string filename = Path.GetFileName(filepath);
                if (filename != Settings.Default.keymaps_config && filename != defaultKeymapFilename)
                {
                    list.Add(new Keymap(defaultKeymap, filename));
                }
            }
            return list;
        }

        public Keymap getKeymap(string filename)
        {
            List<Keymap> list = this.getAllKeymaps();

            foreach (Keymap keymap in list)
            {
                if (keymap.Filename == filename)
                {
                    return keymap;
                }
            }
            return null;
        }

        public Keymap getDefaultKeymap()
        {
            List<Keymap> list = this.getAllKeymaps();
            KeymapSettings settings = this.getKeymapSettings();

            foreach (Keymap keymap in list)
            {
                if (keymap.Filename == settings.getDefaultKeymap())
                {
                    return keymap;
                }
            }
            return null;
        }

        public Keymap getCalibrationKeymap()
        {
            List<Keymap> list = this.getAllKeymaps();
            KeymapSettings settings = this.getKeymapSettings();

            foreach (Keymap keymap in list)
            {
                if (keymap.Filename == settings.getCalibrationKeymap())
                {
                    return keymap;
                }
            }
            return null;
        }

        public List<KeymapInput> getAvailableInputs()
        {
            return allInputs;
        }

        public List<KeymapInput> getAvailableInputs(KeymapInputSource source, bool onscreen)
        {
            List<KeymapInput> list = new List<KeymapInput>();
            foreach (KeymapInput input in allInputs)
            {
                if ((input.Source == source) && ((onscreen && input.OnScreen) || (!onscreen && !input.OnScreen)))
                {
                    list.Add(input);
                }
            }
            return list;
        }

        public List<KeymapOutput> getAvailableOutputs()
        {
            return allOutputs;
        }

        public List<KeymapOutput> getAvailableOutputs(KeymapOutputType type)
        {
            if (type == KeymapOutputType.ALL)
            {
                return allOutputs;
            }
            List<KeymapOutput> list = new List<KeymapOutput>();
            foreach (KeymapOutput output in allOutputs)
            {
                if (output.Type == type)
                {
                    list.Add(output);
                }
            }
            return list;
        }

        public KeymapInput getInput(string key)
        {
            List<KeymapInput> list = this.allInputs;
            foreach (KeymapInput input in list)
            {
                if (input.Key.ToLower() == key.ToLower())
                {
                    return input;
                }
            }
            return null;
        }

        public KeymapOutput getOutput(string key)
        {
            List<KeymapOutput> list = this.allOutputs;
            foreach (KeymapOutput output in list)
            {
                if (output.Key.ToLower() == key.ToLower())
                {
                    return output;
                }
            }
            return null;
        }

        public KeymapOutput getDisableOutput()
        {
            return this.getAvailableOutputs(KeymapOutputType.DISABLE).First();
        }

        public bool deleteKeymap(Keymap keymap)
        {
            if (keymap.Filename == this.getKeymapSettings().getDefaultKeymap())
            {
                return false;
            }
            this.getKeymapSettings().removeFromLayoutChooser(keymap);
            this.getKeymapSettings().removeFromApplicationSearch(keymap);
            File.Delete(Settings.Default.keymaps_path + keymap.Filename);
            return true;
        }

        public Keymap createNewKeymap()
        {
            List<Keymap> list = new List<Keymap>();
            string[] files = Directory.GetFiles(Settings.Default.keymaps_path, "*.json");

            string suggestedFilename = "z_custom.json";

            bool recheck = false;

            int iterations = 0;

            do
            {
                recheck = false;
                foreach (string filepath in files)
                {
                    string filename = Path.GetFileName(filepath);
                    if (suggestedFilename == filename)
                    {
                        suggestedFilename = "z_custom_" + (++iterations) + ".json";
                        recheck = true;
                    }
                }
            } while (recheck);

            return new Keymap(this.getDefaultKeymap(), suggestedFilename);
        }



        public void CreateDefaultFiles()
        {
            this.createDefaultApplicationsJSON();
            this.createDefaultKeymapJSON();
            this.createCalibrationKeymapJSON();
        }

        private static void MergeJSON(JObject receiver, JObject donor)
        {
            foreach (var property in donor)
            {
                JObject receiverValue = receiver[property.Key] as JObject;
                JObject donorValue = property.Value as JObject;
                if (receiverValue != null && donorValue != null)
                    MergeJSON(receiverValue, donorValue);
                else
                    receiver[property.Key] = property.Value;
            }
        }

        private JObject createDefaultApplicationsJSON()
        {
            JArray layouts = new JArray();
            layouts.Add(new JObject(
                new JProperty("Name", "Default"),
                new JProperty("Keymap", DEFAULT_JSON_FILENAME)
            ));

            JArray applications = new JArray();

            JObject applicationList =
                new JObject(
                    new JProperty("LayoutChooser", layouts),
                    new JProperty("Applications", applications),
                    new JProperty("Default", DEFAULT_JSON_FILENAME),
                    new JProperty("Calibration", CALIBRATION_JSON_FILENAME)
                );

            JObject union = applicationList;

            if (File.Exists(Settings.Default.keymaps_path + Settings.Default.keymaps_config))
            {
                StreamReader reader = File.OpenText(Settings.Default.keymaps_path + Settings.Default.keymaps_config);
                try
                {
                    JObject existingConfig = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    reader.Close();

                    MergeJSON(union, existingConfig);
                }
                catch (Exception e)
                {
                    throw new Exception(Settings.Default.keymaps_path + Settings.Default.keymaps_config + " is not valid JSON");
                }
            }

            File.WriteAllText(Settings.Default.keymaps_path + Settings.Default.keymaps_config, union.ToString());
            return union;
        }

        private JObject createDefaultKeymapJSON()
        {
            JObject buttons = new JObject();

            buttons.Add(new JProperty("Pointer", "360.stickl"));

            buttons.Add(new JProperty("A", "360.a"));

            buttons.Add(new JProperty("B", "360.b"));

            buttons.Add(new JProperty("Home", "360.Guide"));

            buttons.Add(new JProperty("Left", "360.Left"));
            buttons.Add(new JProperty("Right", "360.Right"));
            buttons.Add(new JProperty("Up", "360.Up"));
            buttons.Add(new JProperty("Down", "360.Down"));

            buttons.Add(new JProperty("Plus", "360.Start"));

            buttons.Add(new JProperty("Minus", "360.Back"));

            buttons.Add(new JProperty("One", "360.X"));

            buttons.Add(new JProperty("Two", "360.Y"));

            buttons.Add(new JProperty("AccelX+", "disable"));
            buttons.Add(new JProperty("AccelX-", "disable"));
            buttons.Add(new JProperty("AccelY+", "disable"));
            buttons.Add(new JProperty("AccelY-", "disable"));
            buttons.Add(new JProperty("AccelZ+", "disable"));
            buttons.Add(new JProperty("AccelZ-", "disable"));

            buttons.Add(new JProperty("Nunchuk.StickUp", "360.StickRUp"));
            buttons.Add(new JProperty("Nunchuk.StickDown", "360.StickRDown"));
            buttons.Add(new JProperty("Nunchuk.StickLeft", "360.StickRLeft"));
            buttons.Add(new JProperty("Nunchuk.StickRight", "360.StickRRight"));
            buttons.Add(new JProperty("Nunchuk.C", "360.TriggerL"));
            buttons.Add(new JProperty("Nunchuk.Z", "360.TriggerR"));

            buttons.Add(new JProperty("Classic.Left", "360.Left"));
            buttons.Add(new JProperty("Classic.Right", "360.Right"));
            buttons.Add(new JProperty("Classic.Up", "360.Up"));
            buttons.Add(new JProperty("Classic.Down", "360.Down"));
            buttons.Add(new JProperty("Classic.StickLUp", "360.StickLUp"));
            buttons.Add(new JProperty("Classic.StickLDown", "360.StickLDown"));
            buttons.Add(new JProperty("Classic.StickLLeft", "360.StickLLeft"));
            buttons.Add(new JProperty("Classic.StickLRight", "360.StickLRight"));
            buttons.Add(new JProperty("Classic.StickRUp", "360.StickRUp"));
            buttons.Add(new JProperty("Classic.StickRDown", "360.StickRDown"));
            buttons.Add(new JProperty("Classic.StickRLeft", "360.StickRLeft"));
            buttons.Add(new JProperty("Classic.StickRRight", "360.StickRRight"));
            buttons.Add(new JProperty("Classic.Minus", "360.Back"));
            buttons.Add(new JProperty("Classic.Plus", "360.Start"));
            buttons.Add(new JProperty("Classic.Home", "360.Guide"));
            buttons.Add(new JProperty("Classic.Y", "360.Y"));
            buttons.Add(new JProperty("Classic.X", "360.X"));
            buttons.Add(new JProperty("Classic.A", "360.A"));
            buttons.Add(new JProperty("Classic.B", "360.B"));
            buttons.Add(new JProperty("Classic.TriggerL", "360.TriggerL"));
            buttons.Add(new JProperty("Classic.TriggerR", "360.TriggerR"));
            buttons.Add(new JProperty("Classic.L", "360.L"));
            buttons.Add(new JProperty("Classic.R", "360.R"));
            buttons.Add(new JProperty("Classic.ZL", "360.BumperL"));
            buttons.Add(new JProperty("Classic.ZR", "360.BumperR"));

            JObject screen = new JObject();

            screen.Add(new JProperty("OnScreen", buttons));

            JObject union = new JObject();

            union.Add(new JProperty("Title", "Default"));

            union.Add(new JProperty("All", screen));

            if (File.Exists(Settings.Default.keymaps_path + Settings.Default.keymaps_config))
            {
                StreamReader reader = File.OpenText(Settings.Default.keymaps_path + DEFAULT_JSON_FILENAME);
                try
                {
                    JObject existingConfig = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    reader.Close();

                    MergeJSON(union, existingConfig);
                }
                catch (Exception e)
                {
                    throw new Exception(Settings.Default.keymaps_path + DEFAULT_JSON_FILENAME + " is not valid JSON");
                }
            }
            File.WriteAllText(Settings.Default.keymaps_path + DEFAULT_JSON_FILENAME, union.ToString());
            return union;
        }

        private JObject createCalibrationKeymapJSON()
        {
            JObject buttons = new JObject();

            buttons.Add(new JProperty("Pointer", "lightguncursor"));

            buttons.Add(new JProperty("A", "disable"));

            buttons.Add(new JProperty("B", "disable"));

            buttons.Add(new JProperty("Home", "disable"));

            buttons.Add(new JProperty("Left", "disable"));
            buttons.Add(new JProperty("Right", "disable"));
            buttons.Add(new JProperty("Up", "disable"));
            buttons.Add(new JProperty("Down", "disable"));

            buttons.Add(new JProperty("Plus", "disable"));

            buttons.Add(new JProperty("Minus", "disable"));

            buttons.Add(new JProperty("One", "disable"));

            buttons.Add(new JProperty("Two", "disable"));

            buttons.Add(new JProperty("AccelX+", "disable"));
            buttons.Add(new JProperty("AccelX-", "disable"));
            buttons.Add(new JProperty("AccelY+", "disable"));
            buttons.Add(new JProperty("AccelY-", "disable"));
            buttons.Add(new JProperty("AccelZ+", "disable"));
            buttons.Add(new JProperty("AccelZ-", "disable"));

            buttons.Add(new JProperty("Nunchuk.StickUp", "disable"));
            buttons.Add(new JProperty("Nunchuk.StickDown", "disable"));
            buttons.Add(new JProperty("Nunchuk.StickLeft", "disable"));
            buttons.Add(new JProperty("Nunchuk.StickRight", "disable"));
            buttons.Add(new JProperty("Nunchuk.C", "disable"));
            buttons.Add(new JProperty("Nunchuk.Z", "disable"));

            buttons.Add(new JProperty("Classic.Left", "disable"));
            buttons.Add(new JProperty("Classic.Right", "disable"));
            buttons.Add(new JProperty("Classic.Up", "disable"));
            buttons.Add(new JProperty("Classic.Down", "disable"));
            buttons.Add(new JProperty("Classic.StickLUp", "disable"));
            buttons.Add(new JProperty("Classic.StickLDown", "disable"));
            buttons.Add(new JProperty("Classic.StickLLeft", "disable"));
            buttons.Add(new JProperty("Classic.StickLRight", "disable"));
            buttons.Add(new JProperty("Classic.StickRUp", "disable"));
            buttons.Add(new JProperty("Classic.StickRDown", "disable"));
            buttons.Add(new JProperty("Classic.StickRLeft", "disable"));
            buttons.Add(new JProperty("Classic.StickRRight", "disable"));
            buttons.Add(new JProperty("Classic.Minus", "disable"));
            buttons.Add(new JProperty("Classic.Plus", "disable"));
            buttons.Add(new JProperty("Classic.Home", "disable"));
            buttons.Add(new JProperty("Classic.Y", "disable"));
            buttons.Add(new JProperty("Classic.X", "disable"));
            buttons.Add(new JProperty("Classic.A", "disable"));
            buttons.Add(new JProperty("Classic.B", "disable"));
            buttons.Add(new JProperty("Classic.TriggerL", "disable"));
            buttons.Add(new JProperty("Classic.TriggerR", "disable"));
            buttons.Add(new JProperty("Classic.L", "disable"));
            buttons.Add(new JProperty("Classic.R", "disable"));
            buttons.Add(new JProperty("Classic.ZL", "disable"));
            buttons.Add(new JProperty("Classic.ZR", "disable"));

            JObject screen = new JObject();

            screen.Add(new JProperty("OnScreen", buttons));

            JObject union = new JObject();

            union.Add(new JProperty("Title", "Calibration"));

            union.Add(new JProperty("All", screen));

            File.WriteAllText(Settings.Default.keymaps_path + CALIBRATION_JSON_FILENAME, union.ToString()); //Prevent user from editing this
            return union;
        }
    }

    public enum KeymapInputSource
    {
        IR,
        WIIMOTE,
        NUNCHUK,
        CLASSIC
    }

    public class KeymapInput
    {
        public string Name { get; private set; }
        public string Key { get; private set; }
        public KeymapInputSource Source { get; private set; }
        public bool Button { get; private set; }
        public bool Continous { get; private set; }
        public bool Cursor { get; private set; }
        public bool OnScreen { get; private set; }

        public KeymapInput(KeymapInputSource source, string name, string key, bool onScreen = true)
            : this(source, name, key, true, false, false, onScreen)
        {

        }

        public KeymapInput(KeymapInputSource source, string name, string key, bool button, bool continous, bool cursor, bool onScreen = true)
        {
            this.Source = source;
            this.Name = name;
            this.Key = key;
            this.Button = button;
            this.Continous = continous;
            this.Cursor = cursor;
            this.OnScreen = onScreen;
        }

        public bool canHandle(KeymapOutput output)
        {
            return ((this.Button == output.Button || this.Continous == output.Continous) && (this.Button == output.Button || this.Cursor == output.Cursor)) || output.Type == KeymapOutputType.DISABLE;
        }
    }


    public enum KeymapOutputType
    {
        ALL, //Only used in search
        MOUSE,
        XINPUT,
        KEYBOARD,
        WIIMOTE,
        CURSOR,
        DISABLE
    }
    public class KeymapOutput
    {
        public string Name { get; private set; }
        public string Key { get; private set; }
        public KeymapOutputType Type { get; private set; }
        public bool Button { get; private set; }
        public bool Continous { get; private set; }
        public bool Cursor { get; private set; }
        public bool Stackable { get; private set; }

        public KeymapOutput(KeymapOutputType type, string name, string key)
            : this(type, name, key, true, false, false, true)
        {

        }

        public KeymapOutput(KeymapOutputType type, string name, string key, bool button, bool continous, bool cursor, bool stackable)
        {
            this.Type = type;
            this.Name = name;
            this.Key = key;
            this.Button = button;
            this.Continous = continous;
            this.Cursor = cursor;
        }

        public bool canStack(KeymapOutput other)
        {
            return this.Stackable && other.Stackable;
        }
    }


    public class KeymapOutputComparer : IComparer<KeymapOutput>
    {
        StringComparer comparer = StringComparer.CurrentCulture;

        public int Compare(KeymapOutput x, KeymapOutput y)
        {
            if (x.Type - y.Type == 0)
            {
                return comparer.Compare(x.Name, y.Name);
            }
            return x.Type - y.Type;
        }
    }
}
