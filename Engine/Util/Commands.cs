using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace TestShader
{

    //TODO: Split this file sensibly

    /// <summary>
    /// Class for handling debug terminal<br/>
    /// </summary>
    public class Commands
    {

        //-----------------------Momentary Variables----------------------------------------
        /// <summary>
        /// Whether or not the terminal is enabled<br/>
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Whether or not the terminal is open at the moment<br/>
        /// </summary>
        public bool Open;

        /// <summary>
        /// Whether or not the terminal can open this frame<br/>
        /// </summary>
        private bool canOpen;

        /// <summary>
        /// The cursor position in the terminal (From the end)<br/>
        /// </summary>
        private int cursorPos = 0;
        /// <summary>
        /// The current text in the terminal<br/>
        /// </summary>
        private string currentText = "";


        //-----------------------------Commands and function keys----------------------
        /// <summary>
        /// The list of action delegates to execute when a function key is pressed<br/>
        /// </summary>
        public Action[] FunctionKeyActions { get; private set; }



        /// <summary>
        /// The list of commands in the terminal<br/>
        /// </summary>
        public Dictionary<string, CommandInfo> commands;

        //---------------------------Other settings---------------------------

        /// <summary>
        /// The maximum number of commands saved in the history<br/>
        /// </summary>
        private readonly int MaxCommand = 500;

        private bool private_supress = true;
        /// <summary>
        /// Whether or not the terminal should supress inputs to the game<br/>
        /// If set to false, it stops InputManager being disabled<br/>
        /// </summary>
        public bool _supress
        {
            get { return private_supress; }
            set
            {
                private_supress = value;
                if (value == false) InputManager._disabled = false;
            }
        }

        /// <summary>
        /// Whether or not the terminal should supress log commands<br/>
        /// </summary>
        public bool surpressLogs = false;


        //----------------------Handle keyboard states-----------------------

        /// <summary>
        /// States of the keyboard before this update<br/>
        /// </summary>
        private KeyboardState oldState;

        /// <summary>
        /// The current state of the keyboard<br/>
        /// </summary>
        private KeyboardState currentState;


        //-----------------Command History------------------
        /// <summary>
        /// The list of commands used in the terminal<br/>
        /// (Accessible through the up/down key)<br/>
        /// </summary>
        private List<string> commandHistory;

        /// <summary>
        /// The index when brosing the command history with up/down<br/>
        /// </summary>
        private int seekIndex = -1;




        //------------------Handling visuals-----------------
        /// <summary>  
        /// The opacity of the terminal<br/>
        /// </summary>
        private const float OPACITY = .8f;

        /// <summary>
        /// The size of the text in the terminal<br/>
        /// </summary>
        public float textSize = 1;

        /// <summary>
        /// The font used in the terminal<br/>
        /// </summary>
        public SpriteFont _font;



        /// <summary>
        /// The time it takes for an underscore to appear<br/>
        /// </summary>
        private const float UNDERSCORE_TIME = .5f;

        /// <summary>
        /// Weither or not the cursor underscore is shown at the moment<br/>
        /// </summary>
        private bool underscore;

        /// <summary>
        /// Timer for updating to show the underscore<br/>
        /// </summary>
        private float underscoreCounter;



        /// <summary>
        /// List of lines to be drawn in the terminal<br/>
        /// </summary>
        private List<Line> drawCommands;

        /// <summary>
        /// The current position of scrolling in the terminal<br/>
        /// </summary>
        private int scrollHeight = 0;



        //------------------Dummy variables-----------------

        /// <summary>
        /// A list of dummy variables that can be used in call commands<br/>
        /// </summary>
        public List<object> Variables = new List<object>();


        //-----------------Call Code completion-------------------------

        /// <summary>
        /// The list of possible start of a call command<br/>
        /// All accesible static commands will be added<br/>
        /// All Commands will be added<br/>
        /// </summary>
        private List<string> baseSorted;

        /// <summary>
        /// The list of possible continuation of a call command<br/>
        /// </summary>
        private List<string> sorted;


        /// <summary>
        /// The index in the list of possible continuation of commands<br/>
        /// -1 is the default value<br/>
        /// </summary>
        private int tabIndex = -1;

        /// <summary>
        /// The start of the possible continuation of commands we are searching for<br/>
        /// </summary>
        /// <example>
        /// If we search for "Game.I._g|ra", with '|' the cursor, the tabSearch will be "Game.I._g"
        /// </example>
        private string tabSearch;

        /// <summary>
        /// The text after the end of the current string we try to complete in the command<br/>
        /// </summary>
        /// /// <example>
        /// If we search for "ChangeScene(_gra|phi)", with '|' the cursor. the tabSearchEnd will be "phi"
        /// </example>
        private string tabSearchEnd;

        /// <summary>
        /// The list of loaded modules for finding static functions for completion<br/>
        /// </summary>
        static List<Module> LoadedModules = new List<Module>();


        //----------------------NOT IMPLEMENTED--------------------------

        /// <summary>
        /// Count the number of times the same key is pressed (NOT IMPLEMENTED)<br/>
        /// If the same key is pressed enough times, it will be repeated faster<br/>
        /// </summary>
        //private float repeatCounter = 0;

        /// <summary>
        /// The key that is being repeated (NOT IMPLEMENTED)<br/>
        /// </summary>
        //private Keys? repeatKey = null;

        /// <summary>
        /// The time it takes for an underscore to reappear<br/>
        /// </summary>
        //private const float REPEAT_DELAY = .5f;
        /// <summary>
        /// The time it takes for underscore vanishing to repeat<br/>
        /// </summary>
        //private const float REPEAT_EVERY = 1 / 30f;

        /// <summary>
        /// The language of the terminal keyboard (Unused)<br/>
        /// </summary>
        //private const string language="HU";



        public Commands()
        {
            //Initialize lists
            commandHistory = new List<string>();
            drawCommands = new List<Line>();
            commands = new Dictionary<string, CommandInfo>();
            sorted = new List<string>();
            baseSorted = new List<string>();

            //Initialize delegates for action keys
            FunctionKeyActions = new Action[12];

            //Set up event listener for text input
            Game.I.Window.TextInput += CharEntered;

            //Add modules to LoadedModules
            LoadedModules = Assembly.GetCallingAssembly().GetLoadedModules(true).Union(Assembly.GetExecutingAssembly().GetLoadedModules(true)).ToList();

            //Load font
            _font = Game.I.Content.Load<SpriteFont>("myFont");

            //Build list of commands of static functions dynamically with Command tag
            BuildCommandsList();
        }

        /// <summary>
        /// Log a message to the terminal<br/>
        /// </summary>
        /// <param name="obj"> Object to log</param>
        /// <param name="color"> Color of the text</param>
        public void Log(object obj, Color color)
        {
            //Don't log if we are supressing logs
            if (surpressLogs) return;

            //Build the message string
            string str;
            if (obj == null)
                str = "null";
            else
                str = obj.ToString();

            //If there are newline split the text instead
            if (str.Contains("\n"))
            {
                var all = str.Split('\n');
                //Split the string to lines for each \n and log each
                foreach (var line in all)
                    Log(line, color);
                return;
            }

            //Split the string if you overhang horizontally
            int maxWidth = Game.I._canvasWidth - 40;
            while (_font.MeasureString(str).X > maxWidth)
            {
                // Split the string at the last space that fits
                int split = -1;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == ' ')
                    {
                        if (_font.MeasureString(str.Substring(0, i)).X <= maxWidth)
                            split = i;
                        else
                            break;
                    }
                }

                //If all spaces fit, break
                if (split == -1)
                    break;

                //Insert split line into drawCommands
                drawCommands.Insert(0, new Line(str.Substring(0, split), color));
                //Remove the split part from the string
                str = str.Substring(split + 1);
            }

            //Add the rest to the drawCommands
            drawCommands.Insert(0, new Line(str, color));

            //Remove old commands if needed
            int maxCommands = MaxCommand;
            while (drawCommands.Count > maxCommands)
                drawCommands.RemoveAt(drawCommands.Count - 1);
        }

        /// <summary>
        /// Log a message to the terminal in white<br/>
        /// </summary>
        /// <param name="obj"> Object to log</param>
        public void Log(object obj)
        {
            Log(obj, Color.White);
        }

        #region Updating and Rendering

        /// <summary>
        /// Update the command manager when terminal is closed<br/>
        /// </summary>
        internal void UpdateClosed()
        {
            //Set canOpen true (From next frame on terminal can open)
            if (!canOpen)
                canOpen = true;
            //Open terminal on key press
            else if (InputManager.Keyboard.Pressed(Keys.NumPad5, Keys.OemPlus, Keys.Escape))
            {

                Open = true;
                currentState = Keyboard.GetState();

                //Reset scroll to 0
                scrollHeight = 0;
            }

            //Execute function keys if pressed
            for (int i = 0; i < FunctionKeyActions.Length; i++)
                if (InputManager.Keyboard.Pressed((Keys)(Keys.F1 + i)))
                    ExecuteFunctionKeyAction(i);
        }

        /// <summary>
        /// Update the command manager when terminal is open<br/>
        /// </summary>
        internal void UpdateOpen()
        {
            //If supressing, disable input
            if (_supress) InputManager._disabled = true;

            // Update keyboard states
            oldState = currentState;
            currentState = Keyboard.GetState();

            //Calculate timig for cursor underscore animation
            underscoreCounter += Game.I._deltaTime;
            while (underscoreCounter >= UNDERSCORE_TIME)
            {
                underscoreCounter -= UNDERSCORE_TIME;
                underscore = !underscore;
            }

            /*
            //Repeat keys old implementation
            if (repeatKey.HasValue)
            {
                if (currentState[repeatKey.Value] == KeyState.Down)
                {
                    repeatCounter += TestGame.game._deltaTime;

                    while (repeatCounter >= REPEAT_DELAY)
                    {
                        HandleKey(repeatKey.Value);
                        repeatCounter -= REPEAT_EVERY;
                    }
                }
                else
                    repeatKey = null;
            }
            */

            //Handle keystrokes
            foreach (Keys key in currentState.GetPressedKeys())
            {
                // Check if the key was just pressed
                if (oldState[key] == KeyState.Up)
                {
                    //Handle keystrokes not registered as TextInput
                    switch (key)
                    {
                        // On key Up we go back in command history
                        case Keys.Up:
                            cursorPos = 0;
                            if (seekIndex < commandHistory.Count - 1)
                            {
                                seekIndex++;
                                currentText = string.Join(" ", commandHistory[seekIndex]);
                            }
                            break;
                        // On key Left or Right we move the cursor
                        case Keys.Left:
                            if (currentText.Length - cursorPos > 0)
                            {
                                cursorPos++;
                            }
                            //Reset the code completion search
                            tabIndex = -1;
                            //Update possible continuations
                            UpdateSorted();

                            //Reset underscore as visible
                            underscore = true;
                            underscoreCounter = 0;
                            break;
                        // On key Left or Right we move the cursor
                        case Keys.Right:
                            if (cursorPos > 0)
                            {
                                cursorPos--;
                            }
                            //Reset the code completion search
                            tabIndex = -1;
                            //Update possible continuations
                            UpdateSorted();

                            //Reset underscore as visible
                            underscore = true;
                            underscoreCounter = 0;
                            break;
                        // On key Down we go forward in command history
                        case Keys.Down:
                            cursorPos = 0;
                            if (seekIndex > -1)
                            {
                                seekIndex--;
                                if (seekIndex == -1)
                                    // If we reached the start of the list, we clear the text
                                    currentText = "";
                                else
                                    currentText = string.Join(" ", commandHistory[seekIndex]);
                            }
                            break;
                        // On key 5 we open the terminal
                        case Keys.NumPad5:

                            // We close the terminal and set canOpen to false, not to open again this frame
                            Open = canOpen = false;
                            // We reset the cursor
                            cursorPos = 0;
                            // If we are supressing, we reenable input
                            if (_supress) InputManager._disabled = false;
                            break;
                    }
                    break;
                }
            }

            //Update scroll height
            scrollHeight += InputManager.Mouse.WheelDelta / 10;
            //Clamp scroll height
            scrollHeight = Math.Max(scrollHeight, 0);
        }


        /// <summary>
        /// Handle keystrokes from text input events<br/>
        /// </summary>
        public void CharEntered(object sender, TextInputEventArgs c)
        {
            // Only handle input if terminal is open
            if (!Open)
            {
                return;
            }
            Keys key = c.Key;

            //Handle keystrokes
            switch (key)
            {
                // On key Backspace we remove the last character
                case Keys.Back:
                    // Check if we are not at the start of the text
                    if (currentText.Length > 0 && currentText.Length - cursorPos - 1 >= 0)
                        // Remove the last character
                        currentText = currentText.Remove(currentText.Length - cursorPos - 1, 1);

                    //Reset underscore as visible
                    underscore = true;
                    underscoreCounter = 0;
                    break;
                // On key Delete we reset the text
                case Keys.Delete:
                    currentText = "";
                    cursorPos = 0;
                    break;
                // On key shift+Tab cycle through possible completions in the negative direction
                case Keys.Tab:
                    if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    {
                        // If we haven't started cycling, we first create list of possible completions
                        if (tabIndex == -1)
                        {
                            // We set the search string
                            tabSearch = currentText.Substring(0, currentText.Length - cursorPos);

                            // We find the cursor position in text (remember cursorPos counted from the end)
                            int lastIndexAfter = currentText.Length - cursorPos;

                            // We search for the end of the word we want to complete
                            for (int i = currentText.Length - cursorPos; i < currentText.Length; i++)
                            {

                                if (currentText[i] == '=' || currentText[i] == ')' || currentText[i] == '.' || currentText[i] == ',' || currentText[i] == '(' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^')
                                {
                                    lastIndexAfter = i;
                                    break;
                                }
                                if ((currentText[i] == '=' || currentText[i] == '(' || currentText[i] == ',' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^'))
                                {
                                    lastIndexAfter = i;
                                    break;
                                }
                            }

                            // Calculate the rest of the word (after the cursor) already typed
                            tabSearchEnd = currentText.Substring(lastIndexAfter);

                            // We find the last possible completion
                            FindLastTab();
                        }
                        else
                        {
                            // If we already updated tabSearch, we cycle through possible completions in the negative direction
                            tabIndex--;

                            // If we are at the start of the list, we find the last possible completion again
                            if (tabIndex < 0 || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                                FindLastTab();
                        }
                    }
                    else
                    {
                        // We repeat the same thing in the positive direction
                        if (tabIndex == -1)
                        {
                            int lastIndexAfter = currentText.Length - cursorPos;

                            for (int i = currentText.Length - cursorPos; i < currentText.Length; i++)
                            {

                                if (currentText[i] == '=' || currentText[i] == ')' || currentText[i] == '.' || currentText[i] == ',' || currentText[i] == '(' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^')
                                {
                                    lastIndexAfter = i;
                                    break;
                                }
                                if ((currentText[i] == '=' || currentText[i] == '(' || currentText[i] == ',' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^'))
                                {
                                    lastIndexAfter = i;
                                    break;
                                }
                            }

                            tabSearchEnd = currentText.Substring(lastIndexAfter);
                            tabSearch = currentText.Substring(0, currentText.Length - cursorPos);
                            FindFirstTab();
                        }
                        else
                        {
                            tabIndex++;
                            if (tabIndex >= sorted.Count || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                                FindFirstTab();
                        }
                    }

                    // If we have found a possible completion
                    if (tabIndex != -1)
                    {
                        int oldCursorPos = cursorPos;

                        //We put the cursor at the very end
                        cursorPos = 0;

                        //Step back until we reach the end of the word we just inserted
                        try
                        {
                            while (sorted[tabIndex].Length - 1 - cursorPos >= 0 && tabSearchEnd.Length - 1 - cursorPos >= 0 && sorted[tabIndex][sorted[tabIndex].Length - 1 - cursorPos] == tabSearchEnd[tabSearchEnd.Length - 1 - cursorPos]) cursorPos++;
                        }
                        catch (Exception e)
                        {
                            cursorPos = oldCursorPos + sorted[tabIndex].Length - currentText.Length;
                        }

                        //We update the text
                        currentText = sorted[tabIndex];
                    }
                    break;

                // We execute the action delegate corresponding to the function key
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                    ExecuteFunctionKeyAction((int)(key - Keys.F1));
                    break;

                // We execute commands on enter
                case Keys.Enter:
                    if (currentText.Length > 0)
                    {
                        //We reset the cursor at the very end
                        cursorPos = 0;
                        //We execute the command currently typed
                        EnterCommand();
                    }
                    break;

                //Close the terminal on these keys
                case Keys.OemTilde:
                case Keys.NumPad5:
                case Keys.Escape:
                    //We reset the cursor at the very end
                    cursorPos = 0;
                    //We close the terminal and set canOpen to false to not open it again this frame
                    Open = canOpen = false;
                    //If supress is true, we reenable input
                    if (_supress) InputManager._disabled = false;
                    break;

                default:
                    // For all other keys, we insert the character
                    if (_font.Characters.Contains(c.Character))
                        currentText = currentText.Insert(currentText.Length - cursorPos, c.Character.ToString()); //Insert the char
                    break;

            }
            // If we used a non tab, non shift, non alt, non control key
            // We reset the tab index and update the list of possible completions
            // We also reset the scroll
            if (key != Keys.Tab && key != Keys.LeftShift && key != Keys.RightShift && key != Keys.RightAlt && key != Keys.LeftAlt && key != Keys.RightControl && key != Keys.LeftControl)
            {
                tabIndex = -1;
                UpdateSorted();

                scrollHeight = 0;
            }



        }

        /// <summary>
        /// Updates the list of possible completions<br/>
        /// </summary>
        private void UpdateSorted()
        {
            //We use commands of the debug terminal, so we surpress logs until we are done
            surpressLogs = true;

            //Check if it is the very beginning
            if (currentText.Length < 5 || currentText.Substring(0, 5) != "call ")
            {
                //If so, we use baseSorted
                sorted = baseSorted;
                surpressLogs = false;
                return;
            }
            try
            {
                // If cursor is at the very beginning, we don't have completions
                if (currentText.Length - cursorPos < 5)
                {
                    sorted.Clear();
                    return;
                }

                //Calculate the depth of the cursor in terms of parenthesis
                int endDepth = 0;
                for (int i = 0; i < currentText.Length - cursorPos; i++)
                {
                    if (currentText[i] == '(') endDepth++;
                    if (currentText[i] == ')') endDepth--;
                }

                //TODO: Rewrite this part

                //Calculate the last index before the current word starts 
                int lastIndex = 4;

                //Calculate the last bracket or = or . or , or ( or + or - or * or / or % or & or | or ^ on the same depth as the cursor
                int lastBrOrEqOrCo = 4;

                //Calculate the last character before the word
                char lastChar = ' ';

                int depth = 0;
                for (int i = 0; i < currentText.Length - cursorPos; i++)
                {
                    // Manage depth
                    if (currentText[i] == '(') depth++;
                    if (currentText[i] == ')') depth--;

                    // Calculate start of word
                    if (currentText[i] == '=' || currentText[i] == ')' || currentText[i] == '.' || currentText[i] == ',' || currentText[i] == '(' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^')
                    {
                        lastChar = currentText[i];
                        lastIndex = i;
                    }
                    // Calculate lastBrOrEqOrCo to get the start of the current C# expression
                    if ((currentText[i] == '=' || currentText[i] == '(' || currentText[i] == ',' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^') && endDepth == depth)
                    {
                        lastBrOrEqOrCo = i;
                    }
                }

                //Calculate the index of the end of the word
                int lastIndexAfter = currentText.Length - cursorPos;

                for (int i = currentText.Length - cursorPos; i < currentText.Length; i++)
                {
                    // End of word
                    if (currentText[i] == '=' || currentText[i] == ')' || currentText[i] == '.' || currentText[i] == ',' || currentText[i] == '(' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^')
                    {
                        lastIndexAfter = i;
                        break;
                    }
                    // End of word only if it is on the same depth
                    if ((currentText[i] == '=' || currentText[i] == '(' || currentText[i] == ',' || currentText[i] == '+' || currentText[i] == '-' || currentText[i] == '*' || currentText[i] == '/' || currentText[i] == '%' || currentText[i] == '&' || currentText[i] == '|' || currentText[i] == '^'))
                    {
                        lastIndexAfter = i;
                        break;
                    }
                }

                //Handle if current word is the start of a C# expression
                if (lastIndex == 4 || lastChar == '=' || lastChar == ',' || lastChar == '(' || lastChar == '#' || lastChar == ')')
                {
                    Object obj;

                    // Use the subcall method with @ to give list of all members of the current Game
                    obj = SubCall(Game.I, "@");

                    // Cast the object to a list of members
                    List<MemberInfo> list = obj as List<MemberInfo>;

                    List<char> forbiddenLetters = new List<char>() { '=', ',', '.', '<' };
                    // Find all members that do not contain forbidden letters
                    List<string> names = list.Where(x => !(x as MemberInfo).Name.Intersect(forbiddenLetters).Any()).Select(x => currentText.Substring(0, lastIndex + 1) + (x as MemberInfo).Name + currentText.Substring(lastIndexAfter)).ToList();
                    names.Sort();

                    //Make the list of possible completions this list
                    sorted = names;
                }



                //Handle if current word is the member of the previous C# expression
                if (lastChar == '.')
                {

                    Object obj;
                    // Use the subcall method with @ to give list of all members of the expression before the '.'
                    obj = SubCall(Game.I, currentText.Substring(lastBrOrEqOrCo + 1, lastIndex - lastBrOrEqOrCo) + "@");

                    List<MemberInfo> list = obj as List<MemberInfo>;
                    List<char> forbiddenLetters = new List<char>() { '=', ',', '.', '<' };
                    // Find all members that do not contain forbidden letters
                    List<string> names = list.Where(x => !(x as MemberInfo).Name.Intersect(forbiddenLetters).Any()).Select(x => currentText.Substring(0, lastIndex + 1) + (x as MemberInfo).Name + currentText.Substring(lastIndexAfter)).ToList();
                    names.Sort();

                    //Make the list of possible completions this list
                    sorted = names;
                }

            }
            catch (Exception e)
            {

            }

            //Reset surpressLogs
            surpressLogs = false;

        }

        /// <summary>
        /// Executes the command written by the user<br/>
        /// </summary>
        private void EnterCommand()
        {
            // Split the text by spaces
            string[] data = currentText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // If the command is empty, don't execute
            if (data.Length == 0) { return; }

            // Add the command to the history
            if (commandHistory.Count == 0 || commandHistory[0] != currentText)
                commandHistory.Insert(0, currentText);
            drawCommands.Insert(0, new Line(currentText, Color.Aqua));

            // Reset the text and tab index
            currentText = "";
            seekIndex = -1;

            // Create the arguments array
            string[] args = new string[data.Length - 1];
            for (int i = 1; i < data.Length; i++)
                args[i - 1] = data[i];

            // Execute the command
            ExecuteCommand(data[0].ToLower(), args);
        }

        /// <summary>
        /// Finds the first completion for the current word at the cursor<br/>
        /// </summary>
        private void FindFirstTab()
        {
            tabIndex = -1;
            for (int i = 0; i < sorted.Count; i++)
            {
                // If the search string is empty or the current completion starts with the search string, we stop
                if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
                {
                    tabIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Finds the last completion for the current word<br/>
        /// </summary>
        private void FindLastTab()
        {
            tabIndex = -1;
            for (int i = 0; i < sorted.Count; i++)
                /// If the search string is empty or the current completion starts with the search string, we set the tab index
                if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
                    tabIndex = i;

        }


        /// <summary>
        /// Draws the terminal to the screen<br/>
        /// </summary>
        /// <param name="spriteBatch"> The spritebatch to draw to</param>
        internal void Draw(SpriteBatch spriteBatch)
        {
            // Get the screen size
            int screenWidth = Game.I.GraphicsDevice.Viewport.Width;
            int screenHeight = Game.I.GraphicsDevice.Viewport.Height;

            // Begin drawing
            spriteBatch.Begin();

            // Create the background Texture
            Texture2D _texture;
            _texture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            _texture.SetData(new Color[] { Color.Black * OPACITY });

            // Draw the background
            spriteBatch.Draw(_texture, new Rectangle(10, screenHeight - (int)(50 * textSize), screenWidth - 20, (int)(40 * textSize)), Color.White);

            // Create the text string (including the underscore)
            String currentTextUnderscore = new String(currentText + " ");

            // Add the underscore to the string if needed
            if (underscore)
            {
                //The underscore combines with the previous character
                currentTextUnderscore = currentTextUnderscore.Insert(currentTextUnderscore.Length - cursorPos, "\u0332");
                //currentTextUnderscore=currentTextUnderscore.Insert(currentTextUnderscore.Length-cursorPos,"_");
            }


            // Wrap the text first by '.', then by ',', then by '=', then by '(' 
            // To fit on the screen
            string currentTextBroken = Tools.WrapText(_font, currentTextUnderscore, screenWidth - (int)(40 * textSize), '.', textSize);
            currentTextBroken = Tools.WrapText(_font, currentTextBroken, screenWidth - (int)(40 * textSize), ',', textSize);
            currentTextBroken = Tools.WrapText(_font, currentTextBroken, screenWidth - (int)(40 * textSize), '=', textSize);
            currentTextBroken = Tools.WrapText(_font, currentTextBroken, screenWidth - (int)(40 * textSize), '(', textSize);

            // Draw the text
            spriteBatch.DrawString(_font, ">" + currentTextBroken, new Vector2(20, screenHeight - (int)(42 * textSize)), Color.White, 0, new Vector2(0, 0), textSize, SpriteEffects.None, 1f);
            //Draw.SpriteBatch.DrawString(Draw.DefaultFont, ">" + currentText, new Vector2(20, screenHeight - 42), Color.White);


            // Draw the command history as well
            if (drawCommands.Count > 0)
            {
                // Calculate the height of the command history
                int height = (int)(10 * textSize) + (int)(30 * drawCommands.Count * textSize);


                // Draw the command history background
                spriteBatch.Draw(_texture, new Rectangle(10, screenHeight - height - (int)(60 * textSize), screenWidth - 20, height), Color.White);
                //spriteBatch.Rect(10, screenHeight - height - 60, screenWidth - 20, height, Color.Black * OPACITY);

                //Create the cut rectangle for the command history
                Rectangle cutRect = new Rectangle(10, screenHeight - height - (int)(60 * textSize), screenWidth - 20, height);

                // Apply the cut rectangle by restarting rendering
                spriteBatch.End();
                RasterizerState _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = spriteBatch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = spriteBatch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = spriteBatch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = spriteBatch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = spriteBatch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = spriteBatch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                Rectangle _globalCutRect = spriteBatch.GraphicsDevice.ScissorRectangle;
                spriteBatch.GraphicsDevice.ScissorRectangle = cutRect;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rats);

                // Draw the command history
                for (int i = 0; i < drawCommands.Count; i++)
                    spriteBatch.DrawString(_font, drawCommands[i].Text, new Vector2(20, screenHeight - 92 * textSize - (30 * textSize * i) + scrollHeight), drawCommands[i].Color, 0, new Vector2(0, 0), textSize, SpriteEffects.None, 1f);

                // Remove cut rectangle by restarting rendering
                spriteBatch.End();
                _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = spriteBatch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = spriteBatch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = spriteBatch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = spriteBatch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = spriteBatch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = spriteBatch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                spriteBatch.GraphicsDevice.ScissorRectangle = _globalCutRect;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rats);


                //Draw.SpriteBatch.DrawString(Draw.DefaultFont, drawCommands[i].Text, new Vector2(20, screenHeight - 92 - (30 * i)), drawCommands[i].Color);
            }

            // Render to the screen
            spriteBatch.End();
        }

        #endregion

        #region Execute

        /// <summary>
        /// Execute a command written by the user<br/>
        /// </summary>
        /// <param name="command"> Name of the command</param>
        /// <param name="args"> Arguments of the command as a string array</param>
        public void ExecuteCommand(string command, string[] args)
        {
            // Check if the command is valid
            if (commands.ContainsKey(command))
                // Execute the command as an action delegate
                commands[command].Action(args);
            else
                // Log if the command is not valid
                Log("Command '" + command + "' not found! Type 'help' for list of commands", Color.Yellow);
        }

        /// <summary>
        /// Execute a function key action if not null<br/>
        /// </summary>
        /// <param name="num"> The number of the function key</param>
        public void ExecuteFunctionKeyAction(int num)
        {
            if (FunctionKeyActions[num] != null)
                FunctionKeyActions[num]();
        }

        #endregion

        #region Parse Commands

        /// <summary>
        /// Builds the list of commands<br/>
        /// </summary>
        private void BuildCommandsList()
        {
            //Check MonoGame for Commands
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    ProcessMethod(method);

            //Check the calling assembly for Commands
            foreach (var type in Assembly.GetEntryAssembly().GetTypes())
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    ProcessMethod(method);

            //Maintain the sorted command list
            foreach (var command in commands)
                baseSorted.Add(command.Key);
            baseSorted.Sort();
            sorted = baseSorted;
        }

        /// <summary>
        /// Processes a method as a command if it has the Command attribute<br/>
        /// </summary>
        /// <param name="method"> The method to process</param>
        /// <exception cref="Exception"> If the method is not static or has a non-void argument type</exception>
        private void ProcessMethod(MethodInfo method)
        {
            // Get the command attribute
            Command attr = null;
            {
                var attrs = method.GetCustomAttributes(typeof(Command), false);
                if (attrs.Length > 0)
                    attr = attrs[0] as Command;
            }

            // Only process methods with the Command attribute
            if (attr == null)
            {
                return;
            }

            // Commands must be static
            if (!method.IsStatic)
            {
                throw new Exception(method.DeclaringType.Name + "." + method.Name + " is marked as a command, but is not static");
            }
            else
            {

                // Create the command info
                CommandInfo info = new CommandInfo();
                info.Help = attr.Help;

                var parameters = method.GetParameters();
                var defaults = new object[parameters.Length];
                string[] usage = new string[parameters.Length];


                // Build the usage string array of parameters
                for (int i = 0; i < parameters.Length; i++)
                {
                    // For each parameter we list the name and the type
                    var p = parameters[i];
                    usage[i] = p.Name + ":";

                    if (p.ParameterType == typeof(string))
                        usage[i] += "string";
                    else if (p.ParameterType == typeof(int))
                        usage[i] += "int";
                    else if (p.ParameterType == typeof(float))
                        usage[i] += "float";
                    else if (p.ParameterType == typeof(bool))
                        usage[i] += "bool";
                    else
                        // Throw an exception if the type is not valid
                        throw new Exception(method.DeclaringType.Name + "." + method.Name + " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool");

                    // Add the default values
                    // If the parameter has no default value set it to null
                    if (p.DefaultValue == DBNull.Value)
                        defaults[i] = null;
                    else if (p.DefaultValue != null)
                    {
                        defaults[i] = p.DefaultValue;
                        // Add the default value to the usage string array as well
                        if (p.ParameterType == typeof(string))
                            usage[i] += "=\"" + p.DefaultValue + "\"";
                        else
                            usage[i] += "=" + p.DefaultValue;
                    }
                    else
                        defaults[i] = null;
                }

                // If there are parameters we create the usage string
                if (usage.Length == 0)
                    info.Usage = "";
                else
                    info.Usage = "[" + string.Join(" ", usage) + "]";


                // Create the delegate for the command
                info.Action = (args) =>
                    {
                        // If there are no arguments we invoke the method
                        if (parameters.Length == 0)
                            InvokeMethod(method);
                        else
                        {
                            // If there are arguments we create an array for the parameters (first with default values)
                            object[] param = (object[])defaults.Clone();

                            // We determine the value of the parameters for each type
                            for (int i = 0; i < param.Length && i < args.Length; i++)
                            {
                                if (parameters[i].ParameterType == typeof(string))
                                    param[i] = ArgString(args[i]);
                                else if (parameters[i].ParameterType == typeof(int))
                                    param[i] = ArgInt(args[i]);
                                else if (parameters[i].ParameterType == typeof(float))
                                    param[i] = ArgFloat(args[i]);
                                else if (parameters[i].ParameterType == typeof(bool))
                                    param[i] = ArgBool(args[i]);
                            }

                            // We invoke the method with the parameters
                            InvokeMethod(method, param);
                        }
                    };

                // Add the command info to the dictionary
                commands[attr.Name] = info;
            }
        }

        /// <summary>
        /// Invokes a method with the given parameters from MethodInfo<br/>
        /// </summary>
        /// <param name="method"> The method to invoke</param>
        /// <param name="param"> The parameters</param>
        private void InvokeMethod(MethodInfo method, object[] param = null)
        {
            try
            {
                method.Invoke(null, param);
            }
            catch (Exception e)
            {
                // In case of an exception we log the exception and the stack trace
                Game.I.commands.Log(e.InnerException.Message, Color.Yellow);
                LogStackTrace(e.InnerException.StackTrace);
            }
        }

        /// <summary>
        /// Log the stack trace given as parameter<br/>
        /// </summary>
        /// <param name="stackTrace"> The stack trace as string</param>
        private void LogStackTrace(string stackTrace)
        {
            // Only log if we are not surpressing logs
            if (surpressLogs) return;

            // Handle the stack trace line by line
            foreach (var call in stackTrace.Split('\n'))
            {
                string log = call;

                //Remove File Path
                {
                    var from = log.LastIndexOf(" in ") + 4;
                    var to = log.LastIndexOf('\\') + 1;
                    if (from != -1 && to != -1)
                        log = log.Substring(0, from) + log.Substring(to);
                }

                //Remove arguments list
                {
                    var from = log.IndexOf('(') + 1;
                    var to = log.IndexOf(')');
                    if (from != -1 && to != -1)
                        log = log.Substring(0, from) + log.Substring(to);
                }

                //Space out the colon line number
                var colon = log.LastIndexOf(':');
                if (colon != -1)
                    log = log.Insert(colon + 1, " ").Insert(colon, " ");

                // Format the log
                log = log.TrimStart();
                log = "-> " + log;

                // Log the line
                Game.I.commands.Log(log, Color.White);
            }
        }

        /// <summary>
        /// Command information<br/>
        /// Contains the action delegate, help and usage<br/>
        /// </summary>
        public struct CommandInfo
        {
            public Action<string[]> Action;
            public string Help;
            public string Usage;
        }

        #region Parsing Arguments

        /// <summary>
        /// Parses an argument as a string<br/>
        /// </summary>
        /// <param name="arg"> The argument</param>
        /// <returns> The argument as a string</returns>
        private static string ArgString(string arg)
        {
            if (arg == null)
                return "";
            else
                return arg;
        }

        /// <summary>
        /// Parses an argument as a bool<br/>
        /// </summary>
        /// <param name="arg"> The argument</param>
        /// <returns> The argument as a bool</returns>
        private static bool ArgBool(string arg)
        {
            if (arg != null)
                return !(arg == "0" || arg.ToLower() == "false" || arg.ToLower() == "f");
            else
                return false;
        }


        /// <summary>
        /// Parses an argument as an int<br/>
        /// </summary>
        /// <param name="arg"> The argument</param>
        /// <returns> The argument as an int</returns>
        private static int ArgInt(string arg)
        {
            try
            {
                return Convert.ToInt32(arg);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Parses an argument as a float<br/>
        /// </summary>
        /// <param name="arg"> The argument</param>
        /// <returns> The argument as a float</returns>
        private static float ArgFloat(string arg)
        {
            try
            {
                return Convert.ToSingle(arg);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #endregion

        #region Built-In Commands

        /// <summary>
        /// Clears the terminal<br/>
        /// </summary>
        [Command("clear", "Clears the terminal")]
        public static void Clear()
        {
            Game.I.commands.drawCommands.Clear();
        }

        /// <summary>
        /// Exits the game<br/>
        /// </summary>
        [Command("exit", "Exits the game")]
        private static void Exit()
        {
            Game.I.Exit();
        }

        /// <summary>
        /// Enables or disables vertical sync<br/>
        /// </summary>
        [Command("vsync", "Enables or disables vertical sync")]
        private static void Vsync(bool enabled = true)
        {
            Game.I._graphics.SynchronizeWithVerticalRetrace = enabled;
            Game.I._graphics.ApplyChanges();
            Game.I.commands.Log("Vertical Sync " + (enabled ? "Enabled" : "Disabled"));
        }

        /// <summary>
        /// Enables or disables fixed time step<br/>
        /// </summary>
        [Command("fixed", "Enables or disables fixed time step")]
        private static void Fixed(bool enabled = true)
        {
            Game.I.IsFixedTimeStep = enabled;
            Game.I.commands.Log("Fixed Time Step " + (enabled ? "Enabled" : "Disabled"));
        }

        /// <summary>
        /// Sets the target framerate<br/>
        /// </summary>
        [Command("framerate", "Sets the target framerate")]
        private static void Framerate(float target)
        {
            if (target == 0) return;
            Game.I.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / target);
        }


        //TODO: Break up SubTypeCall function 

        /// <summary>
        /// Handles a call of a member of a type from a string<br/>
        /// Used recursively in the call command<br/>
        /// Use @ to get a list of members (both returned and logged)<br/>
        /// </summary>
        /// <param name="obj">The type whose member we call</param>
        /// <param name="_string">The string to resolve. Contains the entire code string after the "." </param>
        /// <returns> The result of the call </returns>
        public static object SubTypeCall(Type obj, string _string)
        {
            // If the Type is null we return null
            if (obj == null) return null;

            // If we use special command @ we create a list of members
            if (_string[0] == '@')
            {
                // Create a list of members
                List<MemberInfo> members = new List<MemberInfo>();

                // Add all fields of the type
                foreach (FieldInfo type in obj.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
                {
                    Game.I.commands.Log(type.Name);
                    members.Add(type);
                }
                // Add all properties of the type
                foreach (PropertyInfo prop in obj.GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
                {
                    Game.I.commands.Log(prop.Name);
                    members.Add(prop);
                }
                // Add all methods of the type
                foreach (Type type in obj.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public))
                {
                    Game.I.commands.Log(type.Name);
                    members.Add(type);
                }
                // Add all methods of the type
                foreach (MethodInfo method in obj.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
                {

                    Game.I.commands.Log(method.Name);
                    // Also Log the parameters' types
                    foreach (var param in method.GetParameters())
                    {
                        Game.I.commands.Log(param.ParameterType);
                    }
                    Game.I.commands.Log("");
                    members.Add(method);
                }
                return members;
            }

            // Find first separator in call
            int firstDot;
            for (firstDot = 0; firstDot < _string.Length; firstDot++)
            {
                if (_string[firstDot] == '.' || _string[firstDot] == '(' || _string[firstDot] == '=' || _string[firstDot] == '+' || _string[firstDot] == '-' || _string[firstDot] == '*' || _string[firstDot] == '/' || _string[firstDot] == '&' || _string[firstDot] == '|' || _string[firstDot] == '^' || _string[firstDot] == '<' || _string[firstDot] == '>' || _string[firstDot] == '!' || _string[firstDot] == '%') break;
            }

            // Separate the first single expression
            // We will first handle this separately and then recursively handle the rest
            // If it is a method we will call it in this call
            string firstPart = _string.Substring(0, firstDot);


            // Log command (Unused)
            //if(_string.Length==firstDot) TestGame.game.Commands.Log(_string, Color.Wheat);
            //else TestGame.game.Commands.Log(_string[firstDot],Color.Wheat);

            // If the firstPart is a field propert or nested type
            if (firstDot == _string.Length || _string[firstDot] == '.' || _string[firstDot] == '=' || _string[firstDot] == '+' || _string[firstDot] == '-' || _string[firstDot] == '*' || _string[firstDot] == '/' || _string[firstDot] == '&' || _string[firstDot] == '|' || _string[firstDot] == '^' || _string[firstDot] == '<' || _string[firstDot] == '>' || _string[firstDot] == '!' || _string[firstDot] == '%')
            {
                // Get the field prop or nested type
                FieldInfo field = obj.GetField(firstPart, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                PropertyInfo prop = obj.GetProperty(firstPart, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                Type type = obj.GetNestedType(firstPart, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                if (field == null && prop == null && type == null)
                {
                    // Throw error if not found
                    throw new ArgumentException("This is not a field or prop");
                }

                //Handle the operator after the firstPart
                #region handleOperators

                // If we are at the end, return the field's, prop's or nested type's value
                if (firstDot == _string.Length)
                {

                    if (field != null)
                        return field.GetValue(null);
                    else if (prop != null)
                        return prop.GetValue(null);
                    else if (type != null)
                        return type;
                }
                // If we are accessing a member, handle it via subcall (SubCall or SubTypeCall depending on the member being an object or a type)
                else if (_string[firstDot] == '.')
                {
                    if (field != null)
                        return SubCall(field.GetValue(null), _string.Substring(firstDot + 1));
                    else if (prop != null)
                        return SubCall(prop.GetValue(null), _string.Substring(firstDot + 1));
                    else if (type != null)
                        return SubTypeCall(type, _string.Substring(firstDot + 1)); ;
                }
                // Handle default operators by accesing the operator method  (starting with "op_")
                // Handle calculating the value after recursively each by calling SubCall 
                else if (_string[firstDot] == '=' && _string[firstDot + 1] == '=')
                {

                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '!' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '<' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '>' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '<' && _string[firstDot + 1] == '<')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '>' && _string[firstDot + 1] == '>')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '+' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    Game.I.commands.Log(_string.Substring(firstDot + 2));
                    if (field != null && field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));

                    return proposedValue;
                }
                else if (_string[firstDot] == '-' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '*' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '/' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '&' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '|' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '^' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(null, field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(null, prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue }));
                    return proposedValue;
                }
                // Handle assignment 
                else if (_string[firstDot] == '=')
                {

                    // Calculate the value to assign recursively by calling the function SubCall
                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));

                    // Set the value of the field or property after converting it to the correct type
                    if (field != null)
                        field.SetValue(null, Convert.ChangeType(proposedValue, field.FieldType), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null);
                    else if (prop != null)
                        prop.SetValue(null, Convert.ChangeType(proposedValue, prop.PropertyType), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null, null);

                    return proposedValue;
                }
                else if (_string[firstDot] == '+')
                {

                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '-')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '*')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '/')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '&')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '|')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '^')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '%')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '<')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '>')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    //TestGame.game.Commands.Log(proposedValue);
                    if (field != null && field.FieldType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(null), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(null), proposedValue });

                    return proposedValue;
                }

                #endregion
            }
            // Handel the case the string is a method call
            else if (_string[firstDot] == '(')
            {

                // Find the closing bracket
                int closingBracket;
                // Find the last comma
                int lastComma = firstDot;


                int depth = 0;
                //Calculate the list of parameters given to the function
                List<Object> parameters = new List<object>();
                for (closingBracket = firstDot; closingBracket < _string.Length; closingBracket++)
                {
                    // Update depth
                    if (_string[closingBracket] == '(') depth++;
                    if (_string[closingBracket] == ')') depth--;

                    // If found closing bracket, break
                    if (_string[closingBracket] == ')' && depth == 0) { break; }

                    // Found comma on highest level, thus separating parameters of the function
                    if (_string[closingBracket] == ',' && depth == 1)
                    {
                        // if not the last comma
                        if (closingBracket - lastComma - 1 != 0)
                            // Add the result of the recursive call to the text between the last comma and the current to the list of parameters
                            parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));

                        //Update the latest comma we found
                        lastComma = closingBracket;
                    }
                }
                // If the brackets don't match we throw an exception
                if (closingBracket == _string.Length)
                {
                    throw new ArgumentException("Brackets don't seem to match");
                }
                // Add the result of the recursive call to the text between the last comma and the closing bracket to the lis of parameters
                if (closingBracket - lastComma - 1 != 0)
                    parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));


                // Call the static function (as an element of a type it must be static)
                Object returnValue = CallStaticMethod(firstPart, obj, parameters);


                // Handle what to do with return value
                #region handleOperators

                // In case the method call is the end of the string we return the return value
                if (closingBracket + 1 == _string.Length)
                {
                    return returnValue;
                }
                // In case we access members of return value we call SubCall or SubTypeCall depending on the return value
                else if (_string[closingBracket + 1] == '.')
                {
                    if (returnValue is Type)
                    {
                        return SubTypeCall(returnValue as Type, _string.Substring(closingBracket + 2));
                    }
                    return SubCall(returnValue, _string.Substring(closingBracket + 2));
                }
                // We handle the operators the same as above
                else if (_string[closingBracket + 1] == '=' && _string[closingBracket + 2] == '=')
                {
                    //TestGame.game.Commands.Log(_string.Substring(closingBracket + 3));
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '!' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '<')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '>')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }


                else if (_string[closingBracket + 1] == '+')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '-')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '*')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '/')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '&')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '|')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '^')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '%')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                //TODO: handle assignment to the result via references if applicable
                // Now we just return the second value in case of assignment
                else if (_string[closingBracket + 1] == '=')
                {
                    returnValue = Convert.ChangeType(SubCall(Game.I, _string.Substring(closingBracket + 2)), returnValue.GetType());
                }
                #endregion
            }

            // If we get here, we didn't find a match and we return null
            // We don't throw an exception here because we don't want to break the game in case of an error in a debugging command 
            return null;
        }

        /// <summary>
        /// Extracts a cast method from a type to another type from a given type<br/>
        /// </summary>
        /// <param name="typeWithMethod">Type (or any of its base types) containing the method</param>
        /// <param name="srcType">The type to cast from</param>
        /// <param name="destType">The type to cast to</param>
        /// <returns>The MethodInfo for the cast or null if none found</returns>
        private static MethodInfo GetCastMethod(Type typeWithMethod, Type srcType, Type destType)
        {
            // Cycle through the base types of typeWithMethod
            while (typeWithMethod != typeof(object))
            {
                // Cycle through the methods of type 
                foreach (MethodInfo method in typeWithMethod.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    // Find cast with the correct source and destination types
                    if (method.ReturnType == destType && (method.Name == "op_Explicit" || method.Name == "op_Implicit"))
                    {
                        ParameterInfo[] parms = method.GetParameters();
                        // If found, return it
                        if (parms != null && parms.Length == 1 && parms[0].ParameterType == srcType)
                            return method;
                    }
                }
                typeWithMethod = typeWithMethod.BaseType;
            }

            // Otherwise return null
            return null;
        }



        // TODO: Break up SubCall function
        //?TODO:Can we utilize GetMember to clean up the code?
        //?TODO:As, is , for, foreach, while, if
        //?TODO:Adding lines post functions with event listeners?
        //?TODO:Scale?
        //?TODO:Pause game?
        //DONE:Add handling of parentheses (for example (new a).x() doesn't parse, nor is there any working equivalent)
        //DONE:Set objects to other objects!!(DONE)
        //DONE:Is it a string, number float, bool, double!!(DONE)
        //DONE:Is it a list! (kinda done get_item works)
        //DONE:Is it a new!(DONE), delete
        //DONE:Is it a cast!
        //DONE:System.Types! (Like Rectangle) (Type.GetType(string) should work)(DONE)
        //DONE:Tab completeion in call! (shouldn't be too hard) (DONE)
        //DONE:Is it static!
        //DONE:Break Long command lines!
        //DONE:Throw errors  instead of nulls, try anc catch ! (DONE)
        //DONE:Draw Rect!(DONE)
        //DONE:Scroll!(DONE)
        //DONE:Access private! (DONE)
        //DONE:Update Keyboard layout! (We can use the text input of monogame.. YES)(DONE)
        //DONE:Is it an arithmetic operation
        //DONE:Variables (kinda done)
        //DONE:Surpress keys in game?




        // Formerly added to hide SubCall from debugger to not break the game 
        // [System.Diagnostics.DebuggerHidden]
        /// <summary>
        /// Handles a call from a string from an objects<br/>
        /// The start of a call is treated as a call of the Game object<br/>
        /// Used recursively in the call command<br/>
        /// Use @ to get a list of members (both returned and logged)<br/>
        /// </summary>
        /// <param name="obj"> The object whose member we call</param>
        /// <param name="_string"> The string to resolve. Contains the entire code string after the "."</param>
        /// <returns> The result of the call</returns>
        public static object SubCall(object obj, string _string)
        {
            Game.I.commands.Log(obj);
            Game.I.commands.Log(_string);
            // If the object is null, return null
            if (obj == null) return null;

            // If the string is empty, throw an exception (This should never happen, even if the input command in call is erronous)
            if (_string.Length == 0)
            {
                throw new ArgumentException("Error, string length 0");
            }
            // If the string is "this", return the Game object (calls are treated as calls of the Game object)
            if (_string == "this")
            {
                return Game.I;
            }

            // If the string starts with @, create a list of members
            // We both return and log the members
            // In case we are at a call for the Game object we also include all accessible static members, to be able to call them too
            if (_string[0] == '@')
            {

                // Create a list of members
                List<MemberInfo> members = new List<MemberInfo>();

                // Add fields of the object to the list
                foreach (FieldInfo type in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    Game.I.commands.Log(type.Name);
                    members.Add(type);
                }
                // Add properties of the object to the list
                foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    Game.I.commands.Log(prop.Name);
                    members.Add(prop);
                }
                // Add nested types of the object to the list
                foreach (Type type in obj.GetType().GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
                {
                    Game.I.commands.Log(type.Name);
                    members.Add(type);
                }
                // Add methods of the object to the list
                foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {

                    Game.I.commands.Log(method.Name);
                    // Also log the parameters
                    foreach (var param in method.GetParameters())
                    {
                        Game.I.commands.Log(param.ParameterType);
                    }
                    Game.I.commands.Log("");
                    members.Add(method);
                }

                // If we are at a call for the Game object we also include all accessible static members
                if (obj == Game.I)
                {
                    // Get the members of the calling assembly
                    foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
                        members.Add(type);

                    //type = Assembly.GetCallingAssembly().GetType(firstPart);

                    // Get the members of the entry assembly
                    foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
                        members.Add(type);
                }

                // Return the list
                return members;
            }






            // We will find the end of the first part of the call
            int firstDot = 0;
            
            //Before that we have to handle casting if we start with '('
            //It might also be a parenthesis for the order of exection, we will handle both
            if (_string[0] == '(')
            {

                //Find first ) and make it's position firstDot
                int depth = 0;

                for (int i = 0; i < _string.Length; i++)
                {
                    // Manage depth
                    if (_string[i] == '(') depth++;
                    if (_string[i] == ')') depth--;

                    // Find the last closing bracket
                    if (_string[i] == ')' && depth == 0) { firstDot = i; break; }
                }
                int closingBracket = firstDot;


                //We use SubCall to get the type to cast to (Remember SubTypeCall is for calling members of types)
                Object type = SubCall(Game.I, _string.Substring(1, firstDot - 1));

                //We check if we got a type
                if (type is Type)
                {
                    //We use SubCall to get the value to cast
                    Object value = SubCall(Game.I, _string.Substring(firstDot + 1, _string.Length - (firstDot + 1)));

                    //We try to cast
                    try
                    {
                        //var cast = (type as Type).GetMethod("op_Explicit", new Type[] { value.GetType() });
                        //var result = cast.Invoke(null, new object[] { value });


                        Object result = null;
                        Type destType = type as Type;
                        Type srcType = value.GetType();
                        Object source = value;

                        //Check if the types are the same
                        if (srcType == destType)
                        {
                            result = source;
                        }

                        // Get a cast method between srcType and destType, iterating through base types of the source to find the strongest match
                        MethodInfo cast = null;
                        while (cast == null && srcType != typeof(object))
                        {
                            cast = GetCastMethod(srcType, srcType, destType);
                            if (cast == null) cast = GetCastMethod(destType, srcType, destType);
                            srcType = srcType.BaseType;
                        }

                        // If we found a cast, invoke it
                        if (cast != null)
                        {
                            result = cast.Invoke(null, new object[] { source });

                        }

                        // If destType is an enum, convert the result to it
                        if (destType.IsEnum)
                        {
                            result = Enum.ToObject(destType, source);

                        }
                        // If we got no result, return the value, and hope it works
                        if (result == null) return value;

                        return result;
                    }
                    catch (Exception e)
                    {
                        // If casting fails, return the value and hope it works
                        return value;
                    }
                }
                else
                {
                    // If it is not a type it might just be a parentesis for order of execution
                    object returnValue = type;
                    #region handleOperators

                    // If it isthe end of the call, we return the value
                    if (closingBracket + 1 == _string.Length)
                    {
                        //TestGame.game.Commands.Log("Value is " + returnValue);
                        return returnValue;
                    }
                    // If we have a . we call a member by SubCall
                    else if (_string[closingBracket + 1] == '.')
                    {
                        return SubCall(returnValue, _string.Substring(closingBracket + 2));
                    }

                    //TODO: Implement assignments for objects in brackets
                    //We do not implement assignments
                    /*else if (_string[closingBracket + 1] == '=')
                    {
                        returnValue = Convert.ChangeType(SubCall(TestGame.game, _string.Substring(closingBracket + 2)), returnValue.GetType());
                    }*/
                    //ToReplace
                    //GetMethod\("(\w+)", BindingFlags\.NonPublic \| BindingFlags\.Static \| BindingFlags\.Public\)
                    //GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public| BindingFlags.FlattenHierarchy).First(w => w.Name == "$1")
                    //.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public| BindingFlags.FlattenHierarchy).First(w => w.Name == nameof(myContext.Set))

                    // We handle operators the same as below
                    // We use SubCall to get the value of the expression after the operator
                    // Use the operator methods (starting with "op_") to apply them
                    else if (_string[closingBracket + 1] == '=' && _string[closingBracket + 2] == '=')
                    {
                        //TestGame.game.Commands.Log(_string.Substring(closingBracket + 3));
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }


                    else if (_string[closingBracket + 1] == '!' && _string[closingBracket + 2] == '=')
                    {

                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '=')
                    {

                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '=')
                    {

                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '<')
                    {

                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '>')
                    {

                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                        if (field != null && field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }


                    else if (_string[closingBracket + 1] == '+')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '-')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '*')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '/')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '&')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '|')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '^')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '%')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '<')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    else if (_string[closingBracket + 1] == '>')
                    {
                        Type field = returnValue.GetType();
                        Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                        if (field != null && field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                            return field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                        return proposedValue;
                    }
                    #endregion

                }

                throw new NotImplementedException("This doesn't seem to be a type (a lot aren't implemented yet)");
            }






            // Handle expressions creating new objects for a type
            if (_string.Length > 3 && _string.Substring(0, 4) == "new#")
            {
                Game.I.commands.Log(_string.Substring(4, _string.Length - 4));


                //Find last ( and make it's position firstDot
                //The text before will be treated as the type to make a new object of
                int depth = 0;

                for (int i = 4; i < _string.Length; i++)
                {
                    if (_string[i] == '(' && depth == 0) { firstDot = i; }
                    if (_string[i] == '(') depth++;
                    if (_string[i] == ')') depth--;

                }

                //We have to take constructor parameters of the type subcall
                //Find closing bracket
                int closingBracket;
                //Find last , betweens parameters
                int lastComma = firstDot;
                depth = 0;
                List<Object> parameters = new List<object>();
                for (closingBracket = firstDot; closingBracket < _string.Length; closingBracket++)
                {
                    // Track depth
                    if (_string[closingBracket] == '(') depth++;
                    if (_string[closingBracket] == ')') depth--;

                    // Find last )
                    if (_string[closingBracket] == ')' && depth == 0) { break; }

                    // When we find a , we add the parameter between this and the last comma to the list
                    if (_string[closingBracket] == ',' && depth == 1)
                    {
                        if (closingBracket - lastComma - 1 != 0)
                            //Use a subcall to get the parameter
                            parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));

                        //Track last comma
                        lastComma = closingBracket;
                    }
                }
                // If we don't have a closing bracket, throw an exception as the brackets don't match 
                if (closingBracket == _string.Length)
                {
                    throw new ArgumentException("Brackets don't seem to match");
                }
                // Add the last parameter
                if (closingBracket - lastComma - 1 != 0)
                    parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));

                Object type = SubCall(Game.I, _string.Substring(4, firstDot - 4));

                //Execute the function to get the type
                if (type is Type)
                {
                    // If it's a type, create it with CreateInstance calling the constructor with the parameters
                    return Activator.CreateInstance((type as Type), parameters.ToArray());
                }

                // If it's not a type, throw an exception
                throw new NotImplementedException("This doesn't seem to be a type (a lot isn't implemented yet)");
            }


            // If expression is just an object like (3,"fire" or true) we convert it to the object 
            object conv = ConvertString(_string);
            if (conv != Default)
            {

                return conv;
            }



            //We find the end of the first part of the expression
            for (firstDot = 0; firstDot < _string.Length; firstDot++)
            {
                if (_string[firstDot] == '.' || _string[firstDot] == '(' || _string[firstDot] == '=' || _string[firstDot] == '+' || _string[firstDot] == '-' || _string[firstDot] == '*' || _string[firstDot] == '/' || _string[firstDot] == '&' || _string[firstDot] == '|' || _string[firstDot] == '^' || _string[firstDot] == '<' || _string[firstDot] == '>' || _string[firstDot] == '!' || _string[firstDot] == '%')
                    break;
            }


            // Get the type of the object we use to call the string
            Type _type = obj.GetType();

            //Find the first part of the string
            string firstPart = _string.Substring(0, firstDot);

            //Handle if the first part is a member not a method
            if (firstDot == _string.Length || _string[firstDot] == '.' || _string[firstDot] == '=' || _string[firstDot] == '+' || _string[firstDot] == '-' || _string[firstDot] == '*' || _string[firstDot] == '/' || _string[firstDot] == '&' || _string[firstDot] == '|' || _string[firstDot] == '^' || _string[firstDot] == '<' || _string[firstDot] == '>' || _string[firstDot] == '!' || _string[firstDot] == '%')
            {
                //Try to get the member as a field, property or a nested type
                FieldInfo field = _type.GetField(firstPart, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo prop = _type.GetProperty(firstPart, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                Type type = _type.GetNestedType(firstPart, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                if (field == null && prop == null && type == null)
                {
                    //TODO: Is this a good idea?
                    //If all are null, we try to treat obj as the Game to get all Static members accessible
                    if (obj == Game.I)
                    {

                        #region Finding the System types
                        Debug.WriteLine(firstPart);

                        //Try to get the System types from every relevant namespace
                        if (type == null) type = Type.GetType("Monogame.Framework." + firstPart);//These don't work 
                        if (type == null) type = Type.GetType("Monogame.Framework.Graphics." + firstPart);
                        if (type == null) type = Type.GetType("Monogame.Framework.Input." + firstPart);
                        if (type == null) type = Type.GetType("Monogame.Framework.Audio." + firstPart);
                        if (type == null) type = Type.GetType("Microsoft.Xna.Framework." + firstPart);//These don't work 
                        if (type == null) type = Type.GetType("Microsoft.Xna.Framework.Graphics." + firstPart);
                        if (type == null) type = Type.GetType("Microsoft.Xna.Framework.Input." + firstPart);
                        if (type == null) type = Type.GetType("Microsoft.Xna.Framework.Audio." + firstPart);
                        if (type == null) type = Type.GetType("System." + firstPart);
                        if (type == null) type = Type.GetType("System.Collections." + firstPart);
                        if (type == null) type = Type.GetType("System.Collections.Generic." + firstPart);
                        if (type == null) type = Type.GetType("System.ComponentModel." + firstPart);
                        if (type == null) type = Type.GetType("System.Data." + firstPart);
                        if (type == null) type = Type.GetType("System.Diagnostics." + firstPart);
                        if (type == null) type = Type.GetType("System.Linq." + firstPart);
                        if (type == null) type = Type.GetType("System.Reflection." + firstPart);
                        if (type == null) type = Type.GetType("System.Reflection.Metadata.Ecma335." + firstPart);
                        if (type == null) type = Type.GetType("System.Text." + firstPart);
                        if (type == null) type = Type.GetType(firstPart);
                        if (type == null) type = Type.GetType(firstPart + ",Microsoft.Xna.Framework");//These don't work 
                        if (type == null) type = Type.GetType(firstPart + ",Microsoft.Xna.Framework.Graphics");
                        if (type == null) type = Type.GetType(firstPart + ",Microsoft.Xna.Framework.Input");
                        if (type == null) type = Type.GetType(firstPart + ",Microsoft.Xna.Framework.Audio");
                        if (type == null) type = Type.GetType(firstPart + ",Monogame.Framework");//These don't work 
                        if (type == null) type = Type.GetType(firstPart + ",Monogame.Framework.Graphics");
                        if (type == null) type = Type.GetType(firstPart + ",Monogame.Framework.Input");
                        if (type == null) type = Type.GetType(firstPart + ",Monogame.Framework.Audio");
                        if (type == null) type = Type.GetType(firstPart + ",System");
                        if (type == null) type = Type.GetType(firstPart + ",System.Collections");
                        if (type == null) type = Type.GetType(firstPart + ",System.Collections.Generic");
                        if (type == null) type = Type.GetType(firstPart + ",System.ComponentModel");
                        if (type == null) type = Type.GetType(firstPart + ",System.Data");
                        if (type == null) type = Type.GetType(firstPart + ",System.Diagnostics");
                        if (type == null) type = Type.GetType(firstPart + ",System.Linq");
                        if (type == null) type = Type.GetType(firstPart + ",System.Reflection");
                        if (type == null) type = Type.GetType(firstPart + ",System.Reflection.Metadata.Ecma335");
                        if (type == null) type = Type.GetType(firstPart + ",System.Text");
                        if (type == null) type = Type.GetType(firstPart);
                        try
                        {
                            //Try to get it from each loaded module as well
                            foreach (var module in LoadedModules)
                            {
                                if (type == null) type = module.GetType(firstPart);
                                if (type != null) break;

                            }
                        }
                        catch (Exception e)
                        {

                        }
                        //More solutions!
                        if (type == null)
                            if (Assembly.GetCallingAssembly().GetExportedTypes().Where(t => t.Name == firstPart).Any())
                                type = Assembly.GetCallingAssembly().GetExportedTypes().Where(t => t.Name == firstPart).First();

                        if (type == null)
                            if (Assembly.GetEntryAssembly().GetExportedTypes().Where(t => t.Name == firstPart).Any())
                                type = Assembly.GetEntryAssembly().GetExportedTypes().Where(t => t.Name == firstPart).First();
                        if (type == null)
                            if (Assembly.GetCallingAssembly().GetTypes().Where(t => t.Name == firstPart).Any())
                                type = Assembly.GetCallingAssembly().GetTypes().Where(t => t.Name == firstPart).First();

                        if (type == null)
                            if (Assembly.GetEntryAssembly().GetTypes().Where(t => t.Name == firstPart).Any())
                                type = Assembly.GetEntryAssembly().GetTypes().Where(t => t.Name == firstPart).First();

                        //Try all assemblies exported types as well
                        foreach (Assembly b in AppDomain.CurrentDomain.GetAssemblies().Reverse())
                        {
                            //TestGame.game.Commands.Log(b);    
                            //if (type == null) type = b.GetType(firstPart);
                            //TestGame.game.commands.Log(b);
                            //TestGame.game.commands.Log(type);
                            foreach (Type t in b.GetExportedTypes())
                            {
                                if (type != null) break;
                                if (type == null && t.Name == firstPart)
                                {
                                    type = t; break;
                                }
                            }
                            if (type != null) break;
                        }

                        #endregion


                        if (type == null)
                            throw new ArgumentException(firstPart + ":This is not a field or prop");


                    }
                    else throw new ArgumentException("This is not a field or prop");
                }

                //Handle the output of the method
                #region handleOperators
                //If we are at the end of the string we return the value
                if (firstDot == _string.Length)
                {
                    if (field != null)
                        return field.GetValue(obj);
                    else if (prop != null)
                        return prop.GetValue(obj);
                    else if (type != null)
                        return type;
                }
                //If we have a dot after the closing bracket access the member with a subcall
                else if (_string[firstDot] == '.')
                {
                    if (field != null)
                        return SubCall(field.GetValue(obj), _string.Substring(firstDot + 1));
                    else if (prop != null)
                        return SubCall(prop.GetValue(obj), _string.Substring(firstDot + 1));
                    else if (type != null)
                        return SubTypeCall(type, _string.Substring(firstDot + 1)); ;
                }
                //We handle the operators by a SubCall on the part after the operator
                //We use the operator by applying the operator function (starting with "op_") to the return value
                else if (_string[firstDot] == '=' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '!' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '<' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '>' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '<' && _string[firstDot + 1] == '<')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '>' && _string[firstDot + 1] == '>')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });
                    else if (type != null)
                        return proposedValue;
                }
                else if (_string[firstDot] == '+' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                    Game.I.commands.Log(_string.Substring(firstDot + 2));
                    if (field != null && field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));

                    return proposedValue;
                }
                else if (_string[firstDot] == '-' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                 
                    if (field != null && field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '*' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '/' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                 
                    if (field != null && field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '&' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '|' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                
                    if (field != null && field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                else if (_string[firstDot] == '^' && _string[firstDot + 1] == '=')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 2));
                 
                    if (field != null && field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        field.SetValue(obj, field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue }));
                    else if (prop != null && prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        prop.SetValue(obj, prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue }));
                    return proposedValue;
                }
                // We handle assignment if element is followed by '='
                else if (_string[firstDot] == '=')
                {

                    // We calculate the value to assign with a subcall
                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    
                    // We try to convert the valuetype to match if possible
                    //TODO: Unify methods to convert types
                    if (typeof(IConvertible).IsAssignableFrom(proposedValue.GetType()))
                    {
                        if (field != null)
                            field.SetValue(obj, Convert.ChangeType(proposedValue, field.FieldType), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null);
                        else if (prop != null)
                            prop.SetValue(obj, Convert.ChangeType(proposedValue, prop.PropertyType), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null, null);

                    }
                    else
                    {
                        if (field != null)
                            field.SetValue(obj, proposedValue, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null);
                        else if (prop != null)
                            prop.SetValue(obj, proposedValue, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null, null);
                    }



                    return proposedValue;
                }
                else if (_string[firstDot] == '+')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    
                    if (field != null && field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '-')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    
                    if (field != null && field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '*')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                    
                    if (field != null && field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '/')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '&')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '|')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '^')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '%')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                 
                    if (field != null && field.FieldType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '<')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }
                else if (_string[firstDot] == '>')
                {


                    Object proposedValue = SubCall(Game.I, _string.Substring(firstDot + 1));
                
                    if (field != null && field.FieldType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.FieldType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { field.GetValue(obj), proposedValue });
                    else if (prop != null && prop.PropertyType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return prop.PropertyType.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { prop.GetValue(obj), proposedValue });

                    return proposedValue;
                }

                #endregion

            }

            // Handle if the member is a method
            else if (_string[firstDot] == '(')
            {

                // Find the closing bracket
                int closingBracket;

                // Find the last comma before the closing bracket
                int lastComma = firstDot;

                //We find a list of parameters
                int depth = 0;
                List<Object> parameters = new List<object>();
                for (closingBracket = firstDot; closingBracket < _string.Length; closingBracket++)
                {
                    //Manage depth
                    if (_string[closingBracket] == '(') depth++;
                    if (_string[closingBracket] == ')') depth--;

                    //Find the last comma
                    if (_string[closingBracket] == ')' && depth == 0) { break; }

                    //For each comma (on the right level), we calulcate the parameter between the this comma and the last
                    if (_string[closingBracket] == ',' && depth == 1)
                    {
                        if (closingBracket - lastComma - 1 != 0)
                            //We use a SubCall to get the parameter value
                            parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));

                        //Track the last comma
                        lastComma = closingBracket;
                    }
                }
                //If there is no closing bracket, we throw an exception
                if (closingBracket == _string.Length)
                {
                    throw new ArgumentException("Brackets don't seem to match");
                }
                //We add the last parameter
                if (closingBracket - lastComma - 1 != 0)
                    parameters.Add(SubCall(Game.I, _string.Substring(lastComma + 1, closingBracket - lastComma - 1)));



                //Call the method with the parameters and store the return value
                Object returnValue = CallMethod(firstPart, _type, obj, parameters);

                //TODO: What about static methods of Game?

                //Handle the output of the method
                #region handleOperators
                //If we are at the end of the string we return the value
                if (closingBracket + 1 == _string.Length)
                {
                    //TestGame.game.Commands.Log("Value is " + returnValue);
                    return returnValue;
                }
                //If we have a dot after the closing bracket access the member with a subcall
                else if (_string[closingBracket + 1] == '.')
                {
                    //TODO: Shouldn't this be a SubTypeCall instead of a SubCall depending on the returnValue?
                    return SubCall(returnValue, _string.Substring(closingBracket + 2));
                }

                //TODO: Define the assignment operator for function return value by references or something

                //We handle the operators by a SubCall on the part after the operator
                //We use the operator by applying the operator function (starting with "op_") to the return value
                else if (_string[closingBracket + 1] == '=' && _string[closingBracket + 2] == '=')
                {
                    //TestGame.game.Commands.Log(_string.Substring(closingBracket + 3));
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Equality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }


                else if (_string[closingBracket + 1] == '!' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Inequality", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LessThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '=')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_GreaterThanOrEqual", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<' && _string[closingBracket + 2] == '<')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LeftShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>' && _string[closingBracket + 2] == '>')
                {

                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 3));
                    if (field != null && field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_RightShift", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }


                else if (_string[closingBracket + 1] == '+')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Addition", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '-')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Subtraction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '*')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Multiply", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '/')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Division", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '&')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_BitwiseAnd", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '|')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_BitwiseOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '^')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_ExclusiveOr", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '%')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_Modulus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '<')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_LessThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                else if (_string[closingBracket + 1] == '>')
                {
                    Type field = returnValue.GetType();
                    Object proposedValue = SubCall(Game.I, _string.Substring(closingBracket + 2));
                    if (field != null && field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                        return field.GetMethod("op_GreaterThan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { returnValue, proposedValue });
                    return proposedValue;
                }
                #endregion

            }

            return null;
        }



        /// <summary>
        /// Calls a static method with the given parameters<br/>
        /// </summary>
        /// <param name="name"> The name of the method.</param>
        /// <param name="_base"> The type of object to call the method on.</param>
        /// <param name="values"> The values of the parameters.</param>
        /// <returns> The return value of the method.</returns>
        /// <exception cref="ArgumentException"> If the method is not found.</exception>
        public static object CallStaticMethod(String name, Type _base, List<Object> values)
        {
            // Get all static methods of the base type
            MethodInfo[] methods = _base.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            // Get the parameters of each method (Unused)
            //Type[][]=methods.Select(t => t.GetParameters().Select(w => w.ParameterType).ToArray()).ToArray();

            // Cycle through each method
            foreach (var method in methods)
            {
                if (method.Name != name) continue;

                // Get the parameters of the method
                ParameterInfo[] parameters = method.GetParameters();

                // Check if the parameters are compatible
                bool compatible = true;
                List<Object> args = new List<Object>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    // If the parameter value is null and the parameter is optional or has a default value, it is compatible
                    if (values[i] == null && (parameters[i].IsOptional || parameters[i].HasDefaultValue || Nullable.GetUnderlyingType(parameters[i].ParameterType) != null))
                    {
                        args.Add(values[i]);
                    }
                    // If the parameter is assignable from the value, it is compatible
                    else if (parameters[i].ParameterType.IsAssignableFrom(values[i].GetType()))
                    {
                        args.Add(values[i]);
                    }
                    // If the parameter can be converted from the value, it is compatible
                    else if (TypeDescriptor.GetConverter(parameters[i].ParameterType).CanConvertFrom(values[i].GetType()))
                    {
                        args.Add(TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFrom(values[i]));
                    }
                    // Otherwise, it is not compatible
                    else
                    {
                        compatible = false;
                    }
                }
                if (compatible)
                {
                    // If they are compatible, invoke the method with the arguments
                    return method.Invoke(null, args.ToArray());
                }
            }

            // If the method is not found, throw an exception
            throw new ArgumentException("This is not a method");
        }


        /// <summary>
        /// Calls a method with the given parameters<br/>
        /// </summary>
        /// <param name="name"> The name of the method.</param>
        /// <param name="_base"> The type of object to call the method on.</param>
        /// <param name="obj"> The object to call the method on.</param>
        /// <param name="values"> The values of the parameters</param>
        /// <returns> The return value of the method.</returns>
        /// <exception cref="ArgumentException"> If the method is not found.</exception>
        public static object CallMethod(String name, Type _base, Object obj, List<Object> values)
        {
            //Get all methods of the base type
            MethodInfo[] methods = _base.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            //Get the parameters of each method (Unused)
            //Type[][]=methods.Select(t => t.GetParameters().Select(w => w.ParameterType).ToArray()).ToArray();

            //Check each method
            foreach (var method in methods)
            {
                if (method.Name != name) continue;

                // Get the parameters of the method
                ParameterInfo[] parameters = method.GetParameters();

                // Check if the parameters are compatible
                bool compatible = true;
                List<Object> args = new List<Object>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    // If the parameter value is null and the parameter is optional or has a default value, it is compatible
                    if (values[i] == null && (parameters[i].IsOptional || parameters[i].HasDefaultValue || Nullable.GetUnderlyingType(parameters[i].ParameterType) != null))
                    {
                        args.Add(values[i]);
                    }
                    // If the parameter is assignable from the value, it is compatible
                    else if (parameters[i].ParameterType.IsAssignableFrom(values[i].GetType()))
                    {
                        args.Add(values[i]);
                    }
                    // If the parameter can be converted from the value, it is compatible
                    else if (TypeDescriptor.GetConverter(parameters[i].ParameterType).CanConvertFrom(values[i].GetType()))
                    {
                        args.Add(TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFrom(values[i]));
                    }
                    // Otherwise, it is not compatible
                    else
                    {
                        compatible = false;
                    }
                }
                if (compatible)
                {
                    // If they are compatible, invoke the method with the arguments
                    return method.Invoke(obj, args.ToArray());
                }
            }

            throw new ArgumentException("This is not a method");
        }




        static object Default = new object();
        /// <summary>
        /// Converts a string to an object for a primitive value<br/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object ConvertString(string input)
        {
            //Handle true, false, null
            if (input == "true")
            {
                return true;
            }
            else if (input == "false")
            {
                return false;
            }
            else if (input == "null")
            {
                return null;
            }

            // Try to parse the input as an integer
            try
            {
                // First, try to parse the input as an integer
                if (int.TryParse(input, out int intResult))
                {
                    return intResult;
                }

            }
            catch (Exception e) { }

            // Next, try to parse the input as a float
            try
            {
                if (input.EndsWith("f"))
                {
                    return (float)float.Parse(input.Remove(input.Length - 1));
                }
            }
            catch (Exception w) { }

            // Next, try to parse the input as a double
            try
            {
                if (input.EndsWith("d"))
                {
                    return double.Parse(input.Remove(input.Length - 1));
                }
            }
            catch (Exception w) { }

            // Next, check if the input is surrounded by double quotes. If it is, remove the quotes and return the string value.
            try
            {
                if (input.StartsWith("\"") && input.EndsWith("\""))
                {
                    return input.Substring(1, input.Length - 2);
                }

            }
            catch (Exception w) { }


            // If the input couldn't be parsed as any of the other types, return a default value
            return Default;
        }


        /// <summary>
        /// Do a function call runtime. Replace spaces in the code by #<br/>
        /// Any object.@ presents the members of the object<br/>
        /// </summary>
        /// <param name="_string"> The string to execute</param>
        [Command("call", "Do a function call runtime. Replace spaces in the code by #")]
        public static void Call(String _string)
        {
            try
            {
                // Try to execute the function and log the result
                Game.I.commands.Log(SubCall(Game.I, _string));
            }
            catch (Exception e)
            {
                // In case of an exception we log the exception and the stack trace
                if (e.InnerException != null && e.InnerException.StackTrace != null)
                {
                    Game.I.commands.Log(e.InnerException.Message, Color.Yellow);
                    Game.I.commands.LogStackTrace(e.InnerException.StackTrace);
                }
                else
                {
                    // In case InnerException.StackTrace is null we try to log the message in InnerException
                    if (e.InnerException != null) Game.I.commands.Log(e.InnerException.Message, Color.Yellow);

                    // We log the message and the stack trace from the exception directly
                    Game.I.commands.Log(e.Message, Color.Yellow);
                    Game.I.commands.LogStackTrace(e.StackTrace);
                }
            }
        }



        /*[Command("count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag")]
        private void Count(int tagIndex = -1)
        {
            if (Engine.Scene == null)
            {
                Engine.Commands.Log("Current Scene is null!");
                return;
            }

            if (tagIndex < 0)
                Engine.Commands.Log(Engine.Scene.Entities.Count.ToString());
            else
                Engine.Commands.Log(Engine.Scene.TagLists[tagIndex].Count.ToString());
        }*/




        //TODO: Tracker for entities
        /*[Command("tracker", "Logs all tracked objects in the scene. Set mode to 'e' for just entities, 'c' for just components, or 'cc' for just collidable components")]
        private void Tracker(string mode)
        {
            if (Engine.Scene == null)
            {
                Engine.Commands.Log("Current Scene is null!");
                return;
            }

            switch (mode)
            {
                default:
                    Engine.Commands.Log("-- Entities --");
                    Engine.Scene.Tracker.LogEntities();
                    Engine.Commands.Log("-- Components --");
                    Engine.Scene.Tracker.LogComponents();
                    Engine.Commands.Log("-- Collidable Components --");
                    Engine.Scene.Tracker.LogCollidableComponents();
                    break;

                case "e":
                    Engine.Scene.Tracker.LogEntities();
                    break;

                case "c":
                    Engine.Scene.Tracker.LogComponents();
                    break;

                case "cc":
                    Engine.Scene.Tracker.LogCollidableComponents();
                    break;
            }
        }*/

        /*[Command("pooler", "Logs the pooled Entity counts")]
        private void Pooler()
        {
            Engine.Pooler.Log();
        }*/

        /*
        [Command("fullscreen", "Switches to fullscreen mode")]
        private static void Fullscreen()
        {
            Engine.SetFullscreen();
        }
        */

        /*
        [Command("window", "Switches to window mode")]
        private static void Window(int scale = 1)
        {
            Engine.SetWindowed(Engine.Width * scale, Engine.Height * scale);
        }
        */

        /// <summary>
        /// Shows usage help for a given command<br/>
        /// </summary>
        /// <param name="command"> The command to show help for </param>
        [Command("help", "Shows usage help for a given command")]
        private static void Help(string command)
        {
            // Check if the command is contained in the list
            if (Game.I.commands.baseSorted.Contains(command))
            {
                // Build the usage string
                var c = Game.I.commands.commands[command];
                StringBuilder str = new StringBuilder();


                //Title
                str.Append(":: ");
                str.Append(command);

                //Usage
                if (!string.IsNullOrEmpty(c.Usage))
                {
                    str.Append(" ");
                    str.Append(c.Usage);
                }

                // Log the string
                Game.I.commands.Log(str.ToString());

                // Show help info if it exists
                if (string.IsNullOrEmpty(c.Help))
                    Game.I.commands.Log("No help info set");
                else
                    Game.I.commands.Log(c.Help);
            }
            else
            {
                // If the command is not contained, log a command list
                StringBuilder str = new StringBuilder();
                str.Append("Commands list: ");
                str.Append(string.Join(", ", Game.I.commands.baseSorted));
                Game.I.commands.Log(str.ToString());
                Game.I.commands.Log("Type 'help command' for more info on that command!");
            }
        }
        #endregion


        /// <summary>
        /// Creates a delegate from the name of a method of Commands<br/>
        /// </summary>
        /// <param name="text">Name of the method</param>
        /// <returns> The created delegate</returns>
        public static Action callDelagate(string text)
        {
            return (Action)typeof(Commands).GetMethod(nameof(Commands.Call)).CreateDelegate(typeof(Action), text);

        }

        /// <summary>
        /// Adds a function key action delegate<br/>
        /// </summary>
        /// <param name="i"> Which function key</param>
        /// <param name="text"> The name of the method to make into a delegate</param>
        public static void AddFunctionKeyActions(int i, string text)
        {
            Game.I.commands.FunctionKeyActions[i] += callDelagate(text);
        }

        /// <summary>
        /// Removes a function key action delegate by replacing it with a default delegate<br/>
        /// </summary>
        /// <param name="i"> Which function key</param>
        public static void RemoveAllFunctionKeyActions(int i)
        {
            Game.I.commands.FunctionKeyActions[i] = default(Action);
        }

        /// <summary>
        /// Represents a line in the command history<br/>
        /// Contains the text and the color<br/>
        /// </summary>
        private struct Line
        {
            public string Text;
            public Color Color;

            public Line(string text)
            {
                Text = text;
                Color = Color.White;
            }

            public Line(string text, Color color)
            {
                Text = text;
                Color = color;
            }
        }
    }

    /// <summary>
    /// Represents a command<br/>
    /// As an attribute it is used to specify if a static method is a command<br/>
    /// Such commands are automatically added to the command list by buildCommandsList<br/>
    /// </summary>
    public class Command : Attribute
    {
        public string Name;
        public string Help;

        public Command(string name, string help)
        {
            Name = name;
            Help = help;
        }
    }
}





#region OldHandleKey

/*
private void HandleKey(Keys key)
{

    scrollHeight = 0;
    if (key != Keys.Tab && key != Keys.LeftShift && key != Keys.RightShift && key != Keys.RightAlt && key != Keys.LeftAlt && key != Keys.RightControl && key != Keys.LeftControl)
        tabIndex = -1;

    if (key != Keys.OemTilde && key != Keys.Oem8 && key != Keys.Enter && repeatKey != key)
    {
        repeatKey = key;
        repeatCounter = 0;
    }

    if(language=="EN")
        switch (key)
        {
            default:
                if (key.ToString().Length == 1)
                {
                    if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        currentText += key.ToString();
                    else if (currentState[Keys.RightAlt] == KeyState.Down)
                    {
                        switch (key)
                        {
                            case (Keys.F):
                                currentText += '[';
                                break;
                            case (Keys.G):
                                currentText += ']';
                                break;
                        }

                    }
                    else
                        currentText += key.ToString().ToLower();


                }
                break;

            case (Keys.D1):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '!';
                else
                    currentText += '1';
                break;
            case (Keys.D2):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '@';
                else
                    currentText += '2';
                break;
            case (Keys.D3):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '#';
                else
                    currentText += '3';
                break;
            case (Keys.D4):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '$';
                else
                    currentText += '4';
                break;
            case (Keys.D5):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '%';
                else
                    currentText += '5';
                break;
            case (Keys.D6):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '^';
                else
                    currentText += '6';
                break;
            case (Keys.D7):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '=';
                else
                    currentText += '7';
                break;
            case (Keys.D8):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '*';
                else
                    currentText += '8';
                break;
            case (Keys.D9):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '(';
                else
                    currentText += '9';
                break;
            case (Keys.D0):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += ')';
                else
                    currentText += '0';
                break;
            case (Keys.OemComma):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '<';
                else
                    currentText += ',';
                break;
            case Keys.OemPeriod:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '>';
                else
                    currentText += '.';
                break;
            case Keys.OemQuestion:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '?';
                else
                    currentText += '/';
                break;
            case Keys.OemSemicolon:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += ':';
                else
                    currentText += ';';
                break;
            case Keys.OemQuotes:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '"';
                else
                    currentText += '\'';
                break;
            case Keys.OemBackslash:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '|';
                else
                    currentText += '\\';
                break;
            case Keys.OemOpenBrackets:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '{';
                else
                    currentText += '[';
                break;
            case Keys.OemCloseBrackets:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '}';
                else
                    currentText += ']';
                break;
            case Keys.OemMinus:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '_';
                else
                    currentText += '-';
                break;
            case Keys.OemPlus:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '+';
                else
                    currentText += '=';
                break;

            case Keys.Space:
                currentText += " ";
                break;
            case Keys.Back:
                if (currentText.Length > 0)
                    currentText = currentText.Substring(0, currentText.Length - 1);
                break;
            case Keys.Delete:
                currentText = "";
                break;

            case Keys.Up:
                if (seekIndex < commandHistory.Count - 1)
                {
                    seekIndex++;
                    currentText = string.Join(" ", commandHistory[seekIndex]);
                }
                break;
            case Keys.Down:
                if (seekIndex > -1)
                {
                    seekIndex--;
                    if (seekIndex == -1)
                        currentText = "";
                    else
                        currentText = string.Join(" ", commandHistory[seekIndex]);
                }
                break;

            case Keys.Tab:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                {
                    if (tabIndex == -1)
                    {
                        tabSearch = currentText;
                        FindLastTab();
                    }
                    else
                    {
                        tabIndex--;
                        if (tabIndex < 0 || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                            FindLastTab();
                    }
                }
                else
                {
                    if (tabIndex == -1)
                    {
                        tabSearch = currentText;
                        FindFirstTab();
                    }
                    else
                    {
                        tabIndex++;
                        if (tabIndex >= sorted.Count || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                            FindFirstTab();
                    }
                }
                if (tabIndex != -1)
                    currentText = sorted[tabIndex];
                break;

            case Keys.F1:
            case Keys.F2:
            case Keys.F3:
            case Keys.F4:
            case Keys.F5:
            case Keys.F6:
            case Keys.F7:
            case Keys.F8:
            case Keys.F9:
            case Keys.F10:
            case Keys.F11:
            case Keys.F12:
                ExecuteFunctionKeyAction((int)(key - Keys.F1));
                break;

            case Keys.Enter:
                if (currentText.Length > 0)
                    EnterCommand();
                break;

            case Keys.Oem8:
            case Keys.OemTilde:
            case Keys.NumPad5:
                Open = canOpen = false;
                break;
        }
    else if(language=="HU")
        switch (key)
        {
            default:
                if (key.ToString().Length == 1)
                {
                    if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        currentText += key.ToString();
                    else if (currentState[Keys.RightAlt] == KeyState.Down)
                    {
                        switch (key)
                        {
                            case (Keys.F):
                                currentText += '[';
                                break;
                            case (Keys.G):
                                currentText += ']';
                                break;
                            case (Keys.V):
                                currentText += '@';
                                break;
                            case (Keys.Q):
                                currentText += '\\';
                                break;
                            case (Keys.OemComma):
                                currentText += ';';
                                break;
                            case (Keys.OemMinus):
                                currentText += '*';
                                break;
                        }

                    }
                    else
                        currentText += key.ToString().ToLower();


                }
                break;

            case (Keys.D1):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '\'';
                else
                    currentText += '1';
                break;
            case (Keys.D2):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '"';
                else
                    currentText += '2';
                break;
            case (Keys.D3):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '+';
                else
                    currentText += '3';
                break;
            case (Keys.D4):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '!';
                else
                    currentText += '4';
                break;
            case (Keys.D5):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '%';
                else
                    currentText += '5';
                break;
            case (Keys.D6):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '/';
                else
                    currentText += '6';
                break;
            case (Keys.D7):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '=';
                else
                    currentText += '7';
                break;
            case (Keys.D8):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '(';
                else
                    currentText += '8';
                break;
            case (Keys.D9):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += ')';
                else
                    currentText += '9';
                break;
            case (Keys.D0):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += 'Ö';
                else
                    currentText += 'ö';
                break;
            case (Keys.OemComma):
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '?';
                else
                    currentText += ',';
                break;
            case Keys.OemPeriod:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += ':';
                else
                    currentText += '.';
                break;
            case Keys.OemQuestion:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '?';
                else
                    currentText += '/';
                break;
            case Keys.OemSemicolon:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += ':';
                else
                    currentText += ';';
                break;
            case Keys.OemQuotes:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '"';
                else
                    currentText += '\'';
                break;
            case Keys.OemBackslash:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '|';
                else
                    currentText += '\\';
                break;
            case Keys.OemOpenBrackets:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '{';
                else
                    currentText += '[';
                break;
            case Keys.OemCloseBrackets:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '}';
                else
                    currentText += ']';
                break;
            case Keys.OemMinus:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '_';
                else
                    currentText += '-';
                break;
            case Keys.OemPlus:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                    currentText += '+';
                else
                    currentText += '=';
                break;

            case Keys.Space:
                currentText += " ";
                break;
            case Keys.Back:
                if (currentText.Length > 0)
                    currentText = currentText.Substring(0, currentText.Length - 1);
                break;
            case Keys.Delete:
                currentText = "";
                break;

            case Keys.Up:
                if (seekIndex < commandHistory.Count - 1)
                {
                    seekIndex++;
                    currentText = string.Join(" ", commandHistory[seekIndex]);
                }
                break;
            case Keys.Down:
                if (seekIndex > -1)
                {
                    seekIndex--;
                    if (seekIndex == -1)
                        currentText = "";
                    else
                        currentText = string.Join(" ", commandHistory[seekIndex]);
                }
                break;

            case Keys.Tab:
                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                {
                    if (tabIndex == -1)
                    {
                        tabSearch = currentText;
                        FindLastTab();
                    }
                    else
                    {
                        tabIndex--;
                        if (tabIndex < 0 || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                            FindLastTab();
                    }
                }
                else
                {
                    if (tabIndex == -1)
                    {
                        tabSearch = currentText;
                        FindFirstTab();
                    }
                    else
                    {
                        tabIndex++;
                        if (tabIndex >= sorted.Count || (tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0))
                            FindFirstTab();
                    }
                }
                if (tabIndex != -1)
                    currentText = sorted[tabIndex];
                break;

            case Keys.F1:
            case Keys.F2:
            case Keys.F3:
            case Keys.F4:
            case Keys.F5:
            case Keys.F6:
            case Keys.F7:
            case Keys.F8:
            case Keys.F9:
            case Keys.F10:
            case Keys.F11:
            case Keys.F12:
                ExecuteFunctionKeyAction((int)(key - Keys.F1));
                break;

            case Keys.Enter:
                if (currentText.Length > 0)
                    EnterCommand();
                break;

            case Keys.None:
            case Keys.OemTilde:
            case Keys.NumPad5:
                Open = canOpen = false;
                break;
        }
}
*/
#endregion
