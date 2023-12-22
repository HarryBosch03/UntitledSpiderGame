using UnityEngine;
using UnityEngine.InputSystem;
using UntitledSpiderGame.Runtime.Mods;
using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Tools
{
    public class Commands : MonoBehaviour
    {
        private bool active;
        private string input;

        private void Update()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (active)
                {
                    active = false;
                    PlayerController.FreezeInput = false;

                    foreach (var command in input.Split(';')) Submit(command);
                }
                else
                {
                    active = true;
                    PlayerController.FreezeInput = true;
                    input = "";
                }
            }
        }

        private void OnGUI()
        {
            if (!active) return;

            var rect = new Rect(0.0f, 0.0f, Screen.width, 30.0f);
            GUI.SetNextControlName("CommandText");
            input = GUI.TextArea(rect, input);
            GUI.FocusControl("CommandText");
        }

        private void Submit(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            input = input.Replace("\n", "");

            var args = input.Split(' ');
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = args[i].ToLower().Trim();
            }

            if (args.Length == 0) return;

            var spider = PlayerController.All.Count > 0 ? PlayerController.All[0] : null;

            switch (args[0])
            {
                case "give":
                {
                    if (spider && args.Length == 2)
                    {
                        var mod = Mod.Find(args[1]);
                        if (mod)
                        {
                            mod.Attach(PlayerController.All[0].ActiveSpider);
                        }
                    }

                    break;
                }
                case "clear":
                {
                    if (spider && args.Length == 1)
                    {
                        var mods = spider.GetComponentsInChildren<Mod>();
                        foreach (var mod in mods)
                        {
                            Destroy(mod.gameObject);
                        }
                    }

                    break;
                }
            }
        }
    }
}