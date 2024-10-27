using LabFusion.Marrow.Proxies;

namespace LabFusion.Menu;

public static class MenuKeyboardHelper
{
    public static bool KeyboardOpened
    {
        get
        {
            var popups = MenuCreator.MenuPopups;

            if (popups == null)
            {
                return false;
            }

            var keyboard = popups.Keyboard;

            return keyboard.Opened;
        }
    }

    public static void AssignKeyboardToButton(StringElement button)
    {
        var popups = MenuCreator.MenuPopups;

        if (popups == null)
        {
            return;
        }

        var keyboard = popups.Keyboard;

        keyboard.Open();

        keyboard.TemporaryUppercase = false;
        keyboard.Uppercase = false;

        keyboard.Value = button.Value;

        keyboard.OnValueChanged = (value) =>
        {
            button.Value = value;
        };

        keyboard.OnEnter = () =>
        {
            button.Submit();
        };

        keyboard.OnClose += OnAssignedKeyboardClose;
    }

    private static void OnAssignedKeyboardClose()
    {
        var popups = MenuCreator.MenuPopups;

        if (popups == null)
        {
            return;
        }

        var keyboard = popups.Keyboard;

        keyboard.OnClose -= OnAssignedKeyboardClose;
        keyboard.OnValueChanged = null;
        keyboard.OnEnter = null;
    }

    public static void CloseKeyboard()
    {
        var popups = MenuCreator.MenuPopups;

        if (popups == null)
        {
            return;
        }

        var keyboard = popups.Keyboard;

        keyboard.Close();
    }
}
